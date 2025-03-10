using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public GameObject prefab;
    public int width = 30, height = 30;
    public float spacing = 1.5f, waveSpeed = 2.0f, waveAmplitude = 1.5f;
    public Gradient colorGradient;

    private Transform[,] grid;
    private float[,] waveOffsets;

    public Transform[,] Grid => grid; // Public getter for ShockwaveAbility

    void Start()
    {
        CreateGrid();
    }

    void Update()
    {
        ApplyWaveEffect();
    }

    void CreateGrid()
    {
        grid = new Transform[width, height];
        waveOffsets = new float[width, height];

        Vector3 basePosition = transform.position;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = basePosition + new Vector3(x * spacing, 0, z * spacing);
                GameObject obj = Instantiate(prefab, position, Quaternion.identity, transform);
                grid[x, z] = obj.transform;
                waveOffsets[x, z] = Random.Range(0f, Mathf.PI * 2);

                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null) rend.material = new Material(rend.material);

                Destroy(obj.GetComponent<BoxCollider>()); // Remove BoxCollider if present
            }
        }
        AddFloorCollider();
    }

    void AddFloorCollider()
    {
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3((width - 1) * spacing / 2, 3.5f, (height - 1) * spacing / 2);
        box.size = new Vector3(width * spacing, 1f, height * spacing);
    }

    void ApplyWaveEffect()
    {
        float time = Time.time * waveSpeed;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Transform obj = grid[x, z];
                float waveHeight = Mathf.Sin(time + waveOffsets[x, z]) * waveAmplitude;
                obj.position = new Vector3(obj.position.x, waveHeight, obj.position.z);

                float normalizedHeight = Mathf.InverseLerp(-waveAmplitude, waveAmplitude, waveHeight);
                obj.GetComponent<Renderer>().material.color = colorGradient.Evaluate(normalizedHeight);
            }
        }
    }
}

