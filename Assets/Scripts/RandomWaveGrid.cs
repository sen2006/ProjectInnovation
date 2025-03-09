using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomWaveGrid : MonoBehaviour
{
    public GameObject prefab;
    public int width = 30, height = 30;
    public float spacing = 1.5f, waveSpeed = 2.0f, waveAmplitude = 1.5f;
    public float shockwaveSpeed = 8.0f, shockwaveMaxRadius = 10.0f, heightMultiplier = 3.0f;
    public float shockwaveDuration = 0.5f;
    public Gradient colorGradient, shockwaveGradient;

    private Transform[,] grid;
    private float[,] waveOffsets;
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private List<Shockwave> activeShockwaves = new List<Shockwave>();

    void Start()
    {
        CreateGrid();
    }

    void Update()
    {
        ApplyWaveEffect();
        UpdateShockwaves();
        if (Input.GetMouseButtonDown(0)) DetectShockwaveOrigin();
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
                Transform objTransform = obj.transform;
                grid[x, z] = objTransform;
                waveOffsets[x, z] = Random.Range(0f, Mathf.PI * 2);

                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null) rend.material = new Material(rend.material);

                // Remove BoxCollider if present
                Destroy(obj.GetComponent<BoxCollider>());

                originalScales[objTransform] = objTransform.localScale;
            }
        }
        AddFloorCollider();
    }

    void AddFloorCollider()
    {
        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3((width - 1) * spacing / 2, 4f-0.5f, (height - 1) * spacing / 2);
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
            float radiusSqr = (elapsedTime * shockwaveSpeed) * (elapsedTime * shockwaveSpeed);

            if (radiusSqr > shockwaveMaxRadius * shockwaveMaxRadius)
            {
                activeShockwaves.RemoveAt(i);
                continue;
            }

            foreach (Transform obj in grid)
            {
                float distanceSqr = (obj.position - wave.origin).sqrMagnitude;
                if (distanceSqr < radiusSqr && distanceSqr > (radiusSqr - 2.25f)) // Using squared values
                {
                    StartCoroutine(ShockwaveEffect(obj, Mathf.Sqrt(distanceSqr) / shockwaveMaxRadius));
                }
            }
        }
    }

    IEnumerator ShockwaveEffect(Transform obj, float normalizedDistance)
    {
        Vector3 originalScale = originalScales[obj];
        Renderer rend = obj.GetComponent<Renderer>();
        Color originalColor = rend.material.color;
        Color waveColor = shockwaveGradient.Evaluate(normalizedDistance);
        float halfDuration = shockwaveDuration / 2;
        float startTime = Time.time;
        float targetHeight = originalScale.y * heightMultiplier;

        while (Time.time - startTime < halfDuration)
        {
            float t = (Time.time - startTime) / halfDuration;
            obj.localScale = new Vector3(originalScale.x, Mathf.Lerp(originalScale.y, targetHeight, t), originalScale.z);
            rend.material.color = Color.Lerp(originalColor, waveColor, t);
            yield return null;
        }

        startTime = Time.time;
        while (Time.time - startTime < halfDuration)
        {
            float t = (Time.time - startTime) / halfDuration;
            obj.localScale = new Vector3(originalScale.x, Mathf.Lerp(targetHeight, originalScale.y, t), originalScale.z);
            rend.material.color = Color.Lerp(waveColor, originalColor, t);
            yield return null;
        }

        obj.localScale = originalScale;
        rend.material.color = originalColor;
    }

    private class Shockwave
    {
        public Vector3 origin;
        public float startTime;
        public Shockwave(Vector3 origin, float startTime) { this.origin = origin; this.startTime = startTime; }
    }
}

