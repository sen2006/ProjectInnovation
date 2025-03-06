using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomWaveGrid : MonoBehaviour
{
    public GameObject prefab;
    public int width = 30;
    public int height = 30;
    public float spacing = 1.5f;
    public float waveSpeed = 2.0f;
    public float waveAmplitude = 1.5f;
    public float shockwaveSpeed = 8.0f;
    public float shockwaveMaxRadius = 10.0f;
    public float heightMultiplier = 3.0f; // How tall the cubes get in the shockwave
    public float shockwaveDuration = 0.5f;
    public Gradient colorGradient;
    public Gradient shockwaveGradient;

    private Transform[,] grid;
    private float[,] waveOffsets;
    private Dictionary<Transform, float> originalHeights = new Dictionary<Transform, float>();
    private List<Shockwave> activeShockwaves = new List<Shockwave>();

    void Start()
    {
        CreateGrid();
    }

    void Update()
    {
        ApplyWaveEffect();
        UpdateShockwaves();
        CheckMagnetInput();
    }

    void CreateGrid()
    {
        grid = new Transform[width, height];
        waveOffsets = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = transform.position + new Vector3(x * spacing, 0, z * spacing);
                GameObject obj = Instantiate(prefab, position, Quaternion.identity, transform);
                grid[x, z] = obj.transform;

                waveOffsets[x, z] = Random.Range(0.0f, Mathf.PI * 2);

                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = new Material(rend.material);
                }

                // Remove BoxCollider from cubes (if they have one)
                if (obj.GetComponent<BoxCollider>() != null)
                {
                    Destroy(obj.GetComponent<BoxCollider>());
                }

                originalHeights[obj.transform] = obj.transform.localScale.y;
            }
        }

        AddFloorCollider();
    }

    void AddFloorCollider()
    {
        BoxCollider box = gameObject.AddComponent<BoxCollider>(); // Attach to this object
        box.center = new Vector3((width - 1) * spacing / 2, -0.1f, (height - 1) * spacing / 2);
        box.size = new Vector3(width * spacing, 1f, height * spacing); // Set correct size
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
                ApplyColor(obj, normalizedHeight);
            }
        }
    }

    void ApplyColor(Transform obj, float normalizedHeight)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = colorGradient.Evaluate(normalizedHeight);
        }
    }

    void CheckMagnetInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                StartShockwave(hit.point);
            }
        }
    }

    void StartShockwave(Vector3 position)
    {
        activeShockwaves.Add(new Shockwave(position, Time.time));
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

            foreach (Transform obj in grid)
            {
                float distance = Vector3.Distance(obj.position, wave.origin);
                if (distance < radius && distance > radius - 1.5f) // Affect objects inside wave boundary
                {
                    StartCoroutine(ShockwaveEffect(obj, distance / shockwaveMaxRadius));
                }
            }
        }
    }

    IEnumerator ShockwaveEffect(Transform obj, float normalizedDistance)
    {
        float originalHeight = originalHeights[obj]; // Restore correct height
        Vector3 originalScale = obj.localScale;
        Renderer rend = obj.GetComponent<Renderer>();
        Color originalColor = rend.material.color;
        Color waveColor = shockwaveGradient.Evaluate(normalizedDistance); // Rainbow effect

        float startTime = Time.time;

        while (Time.time - startTime < shockwaveDuration / 2)
        {
            float t = (Time.time - startTime) / (shockwaveDuration / 2);
            float newHeight = Mathf.Lerp(originalHeight, originalHeight * heightMultiplier, t);
            obj.localScale = new Vector3(originalScale.x, newHeight, originalScale.z);
            rend.material.color = Color.Lerp(originalColor, waveColor, t);
            yield return null;
        }

        startTime = Time.time;

        while (Time.time - startTime < shockwaveDuration / 2)
        {
            float t = (Time.time - startTime) / (shockwaveDuration / 2);
            float newHeight = Mathf.Lerp(originalHeight * heightMultiplier, originalHeight, t);
            obj.localScale = new Vector3(originalScale.x, newHeight, originalScale.z);
            rend.material.color = Color.Lerp(waveColor, originalColor, t);
            yield return null;
        }

        obj.localScale = new Vector3(originalScale.x, originalHeight, originalScale.z); // Ensure reset
        rend.material.color = originalColor;
    }

    private class Shockwave
    {
        public Vector3 origin;
        public float startTime;

        public Shockwave(Vector3 origin, float startTime)
        {
            this.origin = origin;
            this.startTime = startTime;
        }
    }
}

