using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShockwaveAbility : MonoBehaviour
{
    public float shockwaveSpeed = 8.0f, shockwaveMaxRadius = 10.0f, heightMultiplier = 3.0f;
    public float shockwaveDuration = 0.5f;
    public Gradient shockwaveGradient;

    private List<GridManager> gridManagers = new List<GridManager>(); // Track all GridManagers
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();
    private List<Shockwave> activeShockwaves = new List<Shockwave>();

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame(); // Wait for GridManager to initialize

        gridManagers.AddRange(FindObjectsByType<GridManager>(FindObjectsSortMode.None));

        if (gridManagers.Count == 0)
        {
            Debug.LogError("No GridManager instances found! Make sure they exist in the scene.");
            yield break;
        }

        CacheOriginalScales();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) DetectShockwaveOrigin();
        UpdateShockwaves();
    }

    void CacheOriginalScales()
    {
        //foreach (GridManager gridManager in gridManagers)
        //{
        //    foreach (Transform obj in gridManager.Grid)
        //    {
        //        originalScales[obj] = obj.localScale;
        //    }
        //}
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

            foreach (GridManager gridManager in gridManagers)
            {
                //foreach (Transform obj in gridManager.Grid)
                //{
                 //   float distanceSqr = (obj.position - wave.origin).sqrMagnitude;
                 //   if (distanceSqr < radiusSqr && distanceSqr > (radiusSqr - 2.25f)) // Using squared values
                 //   {
                 //       StartCoroutine(ShockwaveEffect(obj, Mathf.Sqrt(distanceSqr) / shockwaveMaxRadius));
                 //   }
                //}
            }
        }
    }

    IEnumerator ShockwaveEffect(Transform obj, float normalizedDistance)
    {
        if (!originalScales.ContainsKey(obj))
        {
            Debug.LogWarning($"Missing key in originalScales dictionary: {obj.name}");
            yield break; // Exit the coroutine if key is missing
        }

        Vector3 originalScale = originalScales[obj]; // Safe access now
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
