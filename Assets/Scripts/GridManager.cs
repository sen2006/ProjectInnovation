using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public int width = 30, height = 30;
    public float spacing = 1.5f, waveSpeed = 2.0f, waveAmplitude = 1.5f;
    public Gradient colorGradient;

    private Matrix4x4[] instanceMatrices;
    private NativeArray<float> waveOffsets;
    public NativeArray<float3> positions;
    public NativeArray<float3> scales;
    public NativeArray<Color> colors;
    public NativeArray<Color> gradientColors;
    private MaterialPropertyBlock propBlock;
    private List<Vector4> colorVectors;

    void Start()
    {
        if (material != null)
            material.enableInstancing = true;

        PrecomputeGradient();
        InitializeGrid();
        AddFloorCollider();
    }

    void Update()
    {
        float time = Time.time * waveSpeed;

        ApplyWaveEffectJob waveJob = new ApplyWaveEffectJob
        {
            time = time,
            waveAmplitude = waveAmplitude,
            waveOffsets = waveOffsets,
            positions = positions,
            colors = colors,
            gradientColors = gradientColors,
            gradientResolution = gradientColors.Length
        };

        JobHandle jobHandle = waveJob.Schedule(positions.Length, 64);
        jobHandle.Complete();

        ApplyRendering();
    }

    void InitializeGrid()
    {
        int totalInstances = width * height;
        instanceMatrices = new Matrix4x4[totalInstances];
        waveOffsets = new NativeArray<float>(totalInstances, Allocator.Persistent);
        positions = new NativeArray<float3>(totalInstances, Allocator.Persistent);
        scales = new NativeArray<float3>(totalInstances, Allocator.Persistent); // ✅ Initialize scales
        colors = new NativeArray<Color>(totalInstances, Allocator.Persistent);
        colorVectors = new List<Vector4>(totalInstances);
        propBlock = new MaterialPropertyBlock();

        Vector3 basePosition = transform.position;
        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                positions[index] = new float3(x * spacing, 0, z * spacing) + (float3)basePosition;
                scales[index] = new float3(1, 1, 1); // ✅ Default scale (1,1,1)
                waveOffsets[index] = UnityEngine.Random.Range(0f, Mathf.PI * 2);
                colors[index] = Color.white;
                index++;
            }
        }
    }

    public void ApplyRendering()
    {
        colorVectors.Clear();
        for (int i = 0; i < positions.Length; i++)
        {
            instanceMatrices[i] = Matrix4x4.TRS(positions[i], Quaternion.identity, Vector3.one);

            // ✅ Convert Color to Vector4 and store per-instance color
            colorVectors.Add(new Vector4(colors[i].r, colors[i].g, colors[i].b, colors[i].a));
        }

        for (int i = 0; i < positions.Length; i++)
        {
            instanceMatrices[i] = Matrix4x4.TRS(
                positions[i],
                Quaternion.identity,
                scales[i]  // Use new scale data
            );
        }

        propBlock.SetVectorArray("_BaseColor", colorVectors);
        Graphics.DrawMeshInstanced(mesh, 0, material, instanceMatrices, instanceMatrices.Length, propBlock);
    }

    void AddFloorCollider()
    {
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3((width - 1) * spacing / 2, 0, (height - 1) * spacing / 2);
        box.size = new Vector3(width * spacing, 1f, height * spacing);
    }

    // ✅ Precompute Gradient (with Linear Color Correction)
    void PrecomputeGradient()
    {
        int gradientResolution = 256;
        gradientColors = new NativeArray<Color>(gradientResolution, Allocator.Persistent);
        for (int i = 0; i < gradientResolution; i++)
        {
            float t = i / (float)(gradientResolution - 1);
            gradientColors[i] = colorGradient.Evaluate(t).linear; // ✅ Ensures proper gradient mapping
        }
    }

    [BurstCompile]
    struct ApplyWaveEffectJob : IJobParallelFor
    {
        public float time;
        public float waveAmplitude;
        public int gradientResolution;

        [ReadOnly] public NativeArray<float> waveOffsets;
        public NativeArray<float3> positions;
        public NativeArray<Color> colors;
        [ReadOnly] public NativeArray<Color> gradientColors;

        public void Execute(int index)
        {
            float waveHeight = Mathf.Sin(time + waveOffsets[index]) * waveAmplitude;
            float3 pos = positions[index];
            pos.y = waveHeight;
            positions[index] = pos;

            float normalizedHeight = Mathf.InverseLerp(-waveAmplitude, waveAmplitude, waveHeight);
            int colorIndex = Mathf.Clamp(Mathf.RoundToInt(normalizedHeight * (gradientResolution - 1)), 0, gradientResolution - 1);
            colors[index] = gradientColors[colorIndex];
        }
    }

    void OnDestroy()
    {
        if (waveOffsets.IsCreated) waveOffsets.Dispose();
        if (positions.IsCreated) positions.Dispose();
        if (colors.IsCreated) colors.Dispose();
        if (gradientColors.IsCreated) gradientColors.Dispose();
    }
}
