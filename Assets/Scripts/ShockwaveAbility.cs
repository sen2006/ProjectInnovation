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
    private NativeArray<bool> affectedCubes; // Track affected cubes
    private NativeList<int> affectedIndices; // Track affected cube indices

    void Start()
    {
        gridManagers.AddRange(FindObjectsByType<GridManager>(FindObjectsSortMode.None));
        if (gridManagers.Count == 0)
        {
            Debug.LogError("No GridManager instances found!");
        }

        int totalInstances = gridManagers.Count > 0 ? gridManagers[0].positions.Length : 0;
        affectedCubes = new NativeArray<bool>(totalInstances, Allocator.Persistent);
        affectedIndices = new NativeList<int>(Allocator.Persistent);
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
                ApplyShockwaveEffect(gridManager, wave, radius);
            }
        }

        // Ensure all previously affected cubes continue shrinking
        foreach (GridManager gridManager in gridManagers)
        {
            ApplyShrinkEffect(gridManager);
        }
    }

    void ApplyShockwaveEffect(GridManager gridManager, Shockwave wave, float radius)
    {
        if (!gridManager.positions.IsCreated || !gridManager.colors.IsCreated || !gridManager.scales.IsCreated) return;

        affectedIndices.Clear(); // Reset affected indices list before tracking

        // Step 1: Apply Shockwave Job (does NOT modify affectedIndices)
        ApplyShockwaveJob shockwaveJob = new ApplyShockwaveJob
        {
            waveOrigin = wave.origin,
            radius = radius,
            heightMultiplier = heightMultiplier,
            animationSpeed = 0.1f,
            shrinkSpeed = 0.05f,
            colors = gridManager.colors,
            positions = gridManager.positions,
            scales = gridManager.scales,
            gradientColors = gridManager.gradientColors,
            gradientResolution = gridManager.gradientColors.Length,
            affectedCubes = affectedCubes
        };

        JobHandle shockwaveHandle = shockwaveJob.Schedule(gridManager.positions.Length, 64);

        // Step 2: Track affected cubes after the shockwave effect
        TrackAffectedCubesJob trackJob = new TrackAffectedCubesJob
        {
            affectedCubes = affectedCubes,
            affectedIndices = affectedIndices
        };

        JobHandle trackHandle = trackJob.Schedule(shockwaveHandle);
        trackHandle.Complete();

        gridManager.ApplyRendering();
    }

    void ApplyShrinkEffect(GridManager gridManager)
    {
        if (!gridManager.positions.IsCreated || !gridManager.scales.IsCreated) return;

        ApplyShrinkJob shrinkJob = new ApplyShrinkJob
        {
            scales = gridManager.scales,
            shrinkSpeed = 0.02f,
            affectedCubes = affectedCubes,
            affectedIndices = affectedIndices
        };

        JobHandle jobHandle = shrinkJob.Schedule();
        jobHandle.Complete();
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
        public NativeArray<bool> affectedCubes; // ✅ Track affected cubes

        public void Execute(int index)
        {
            float3 pos = positions[index];
            float distance = math.distance(new float3(waveOrigin.x, 0, waveOrigin.z), new float3(pos.x, 0, pos.z));

            float radiusSqr = radius * radius;
            float innerRadiusSqr = (radius - 2.5f) * (radius - 2.5f);
            float3 currentScale = scales[index];
            float targetScaleY = 1f;

            if (distance * distance < radiusSqr)
            {
                targetScaleY = heightMultiplier;
                affectedCubes[index] = true; // ✅ Ensure cube is affected
            }

            // ✅ If in the outer edge, mark it affected for shrinking
            if (distance * distance >= innerRadiusSqr && distance * distance <= radiusSqr)
            {
                affectedCubes[index] = true; // Ensure outer ring is also affected
            }

            float lerpSpeed = targetScaleY > currentScale.y ? animationSpeed : shrinkSpeed;
            float newScaleY = math.lerp(currentScale.y, targetScaleY, lerpSpeed);

            scales[index] = new float3(currentScale.x, newScaleY, currentScale.z);
        }
    }


    struct TrackAffectedCubesJob : IJob
    {
        [ReadOnly] public NativeArray<bool> affectedCubes;
        public NativeList<int> affectedIndices;

        public void Execute()
        {
            affectedIndices.Clear();
            for (int i = 0; i < affectedCubes.Length; i++)
            {
                if (affectedCubes[i])
                {
                    affectedIndices.Add(i);
                }
            }
        }
    }

    struct ApplyShrinkJob : IJob
    {
        public NativeArray<float3> scales;
        public float shrinkSpeed;
        public NativeArray<bool> affectedCubes;
        [ReadOnly] public NativeList<int> affectedIndices;

        public void Execute()
        {
            for (int i = 0; i < affectedIndices.Length; i++)
            {
                int cubeIndex = affectedIndices[i];

                float3 currentScale = scales[cubeIndex];
                float newScaleY = math.lerp(currentScale.y, 1f, shrinkSpeed);

                // ✅ Ensure it fully resets to 1.0 and marks as unaffected
                if (math.abs(newScaleY - 1f) < 0.01f)
                {
                    newScaleY = 1f;
                    affectedCubes[cubeIndex] = false; // ✅ Reset state
                }

                scales[cubeIndex] = new float3(currentScale.x, newScaleY, currentScale.z);
            }
        }
    }


    void OnDestroy()
    {
        if (affectedCubes.IsCreated) affectedCubes.Dispose();
        if (affectedIndices.IsCreated) affectedIndices.Dispose();
    }
}





