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

    void Start()
    {
        gridManagers.AddRange(FindObjectsByType<GridManager>(FindObjectsSortMode.None));
        if (gridManagers.Count == 0)
        {
            Debug.LogError("No GridManager instances found!");
        }
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
    }

    void ApplyShockwaveEffect(GridManager gridManager, Vector3 waveOrigin, float radius)
    {
        if (!gridManager.positions.IsCreated || !gridManager.colors.IsCreated) return;

        ApplyShockwaveJob shockwaveJob = new ApplyShockwaveJob
        {
            waveOrigin = waveOrigin,
            radius = radius,
            heightMultiplier = heightMultiplier,
            colors = gridManager.colors,
            positions = gridManager.positions,
            gradientColors = gridManager.gradientColors,
            gradientResolution = gridManager.gradientColors.Length
        };

        JobHandle jobHandle = shockwaveJob.Schedule(gridManager.positions.Length, 64);
        jobHandle.Complete();

        gridManager.ApplyRendering();
    }

    private class Shockwave
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
        public int gradientResolution;

        public NativeArray<float3> positions;
        public NativeArray<Color> colors;
        [ReadOnly] public NativeArray<Color> gradientColors;

        public void Execute(int index)
        {
            float3 pos = positions[index];
            float distance = math.distance(new float3(waveOrigin.x, 0, waveOrigin.z), new float3(pos.x, 0, pos.z));

            if (distance < radius && distance > (radius - 2.5f))
            {
                float effectStrength = math.saturate(1 - (distance / radius));
                pos.y += effectStrength * heightMultiplier;
                positions[index] = pos;

                int colorIndex = math.clamp((int)(effectStrength * (gradientResolution - 1)), 0, gradientResolution - 1);
                colors[index] = gradientColors[colorIndex];
            }
        }
    }
}
