using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;

public class ShockwaveAbility : MonoBehaviour
{
    public float shockwaveSpeed = 8.0f, shockwaveMaxRadius = 10.0f, heightMultiplier = 3.0f;
    public float shockwaveDuration = 0.5f;
    public Gradient shockwaveGradient;

    private List<GridManager> gridManagers = new List<GridManager>();
    private List<Shockwave> activeShockwaves = new List<Shockwave>();
    private NativeArray<bool> affectedCubes; // Track affected cubes using NativeArray
    private NativeArray<Color> shockwaveGradientColors; // Store shockwave colors

    void Start()
    {
        gridManagers.AddRange(FindObjectsByType<GridManager>(FindObjectsSortMode.None));
        if (gridManagers.Count == 0)
        {
            Debug.LogError("No GridManager instances found!");
        }

        // ✅ Initialize affectedCubes to track cube states
        int totalInstances = gridManagers.Count > 0 ? gridManagers[0].positions.Length : 0;
        affectedCubes = new NativeArray<bool>(totalInstances, Allocator.Persistent);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) DetectShockwaveOrigin();
        UpdateShockwaves();
    }

    void DetectShockwaveOrigin()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            activeShockwaves.Add(new Shockwave(hit.point, Time.time));
        }
    }

    public void AddShockwave(Vector3 origin)
    {
        activeShockwaves.Add(new Shockwave(origin, Time.time));
    }

    void UpdateShockwaves()
    {
        float currentTime = Time.time;

        for (int i = activeShockwaves.Count - 1; i >= 0; i--)
        {
            Shockwave wave = activeShockwaves[i];
            float elapsedTime = currentTime - wave.startTime;
            float radius = elapsedTime * shockwaveSpeed;

            if (radius > shockwaveMaxRadius)
            {
                activeShockwaves.RemoveAt(i);
                continue;
            }

            foreach (GridManager gridManager in gridManagers)
            {
                ApplyShockwaveEffect(gridManager, wave.origin, radius);
            }
        }

        // ✅ Ensure all previously affected cubes continue shrinking
        foreach (GridManager gridManager in gridManagers)
        {
            ApplyShrinkEffect(gridManager);
        }
    }

    void ApplyShockwaveEffect(GridManager gridManager, Vector3 waveOrigin, float radius)
    {
        if (!gridManager.positions.IsCreated || !gridManager.colors.IsCreated || !gridManager.scales.IsCreated) return;

        ApplyShockwaveJob shockwaveJob = new ApplyShockwaveJob
        {
            waveOrigin = waveOrigin,
            radius = radius,
            heightMultiplier = heightMultiplier,
            animationSpeed = 0.1f,
            shrinkSpeed = 0.05f,
            colors = gridManager.colors,
            positions = gridManager.positions,
            scales = gridManager.scales,
            gradientColors = gridManager.gradientColors,
            gradientResolution = gridManager.gradientColors.Length,
            affectedCubes = affectedCubes // ✅ Track affected cubes
        };

        JobHandle jobHandle = shockwaveJob.Schedule(gridManager.positions.Length, 64);
        jobHandle.Complete();

        gridManager.ApplyRendering();
    }

    void ApplyShrinkEffect(GridManager gridManager)
    {
        if (!gridManager.positions.IsCreated || !gridManager.scales.IsCreated) return;

        ApplyShrinkJob shrinkJob = new ApplyShrinkJob
        {
            scales = gridManager.scales,
            shrinkSpeed = 0.02f,
            affectedCubes = affectedCubes // ✅ Use NativeArray<bool> for tracking
        };

        JobHandle jobHandle = shrinkJob.Schedule(gridManager.positions.Length, 64);
        jobHandle.Complete();

        gridManager.ApplyRendering();
    }

    public class Shockwave
    {
        public Vector3 origin;
        public float startTime;
        public Shockwave(Vector3 origin, float startTime) { this.origin = origin; this.startTime = startTime; }
    }

    struct ApplyShockwaveJob : IJobParallelFor
    {
        public Vector3 waveOrigin;
        public float radius;
        public float heightMultiplier;
        public float animationSpeed;
        public float shrinkSpeed;
        public int gradientResolution;

        [ReadOnly] public NativeArray<float3> positions;
        public NativeArray<float3> scales;
        public NativeArray<Color> colors;
        [ReadOnly] public NativeArray<Color> gradientColors;
        public NativeArray<bool> affectedCubes; // ✅ Track affected cubes safely

        public void Execute(int index)
        {
            float3 pos = positions[index];
            float distance = math.distance(new float3(waveOrigin.x, 0, waveOrigin.z), new float3(pos.x, 0, pos.z));

            float radiusSqr = radius * radius;
            float innerRadiusSqr = (radius - 2.5f) * (radius - 2.5f);
            float3 currentScale = scales[index];
            float targetScaleY = 1f;

            if (distance * distance < radiusSqr && distance * distance > innerRadiusSqr)
            {
                targetScaleY = heightMultiplier;
                affectedCubes[index] = true; // ✅ Mark cube as affected

                // ✅ Correctly normalize distance to range [0, 1]
                float gradientPos = math.saturate((distance - (radius - 2.5f)) / 2.5f);
                int gradientIndex = (int)(gradientPos * (gradientResolution - 1));
                gradientIndex = math.clamp(gradientIndex, 0, gradientResolution - 1);

                colors[index] = gradientColors[gradientIndex];
            }

            float lerpSpeed = targetScaleY > currentScale.y ? animationSpeed : shrinkSpeed;
            float newScaleY = math.lerp(currentScale.y, targetScaleY, lerpSpeed);

            scales[index] = new float3(currentScale.x, newScaleY, currentScale.z);
        }
    }

    struct ApplyShrinkJob : IJobParallelFor
    {
        public NativeArray<float3> scales;
        public float shrinkSpeed;
        public NativeArray<bool> affectedCubes; // ✅ Track affected cubes safely

        public void Execute(int index)
        {
            if (!affectedCubes[index]) return;

            float3 currentScale = scales[index];
            float newScaleY = math.lerp(currentScale.y, 1f, shrinkSpeed);

            if (math.abs(newScaleY - 1f) < 0.01f)
            {
                newScaleY = 1f;
                affectedCubes[index] = false; // ✅ Reset when fully shrunk
            }

            scales[index] = new float3(currentScale.x, newScaleY, currentScale.z);
        }
    }

    void OnDestroy()
    {
        if (affectedCubes.IsCreated) affectedCubes.Dispose();
    }
}



