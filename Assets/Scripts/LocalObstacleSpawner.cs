using System.Collections;
using UnityEngine;

public class LocalObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject barrierWallPrefab;
    [SerializeField] private GameObject movingBlockPrefab;

    [Header("References")]
    [SerializeField] private Transform player;  // The moving player reference
    [SerializeField] private CameraEffects cameraEffects;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistanceAhead = 15f;
    [SerializeField] private float movingBlockOffsetX = 2f;
    [SerializeField] private float spawnHeight = 1f;

    private bool canSpawnBarrier = true;
    private bool canSpawnMovingBlock = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && canSpawnBarrier)
        {
            Debug.Log("Spawning Barrier Wall (Standalone Test)");
            SpawnBarrierWall();
        }

        if (Input.GetKeyDown(KeyCode.M) && canSpawnMovingBlock)
        {
            Debug.Log("Spawning Moving Block (Standalone Test)");
            SpawnMovingBlock();
        }

        // Simulating Audio & Camera Distortion Sliders
        if (Input.GetKey(KeyCode.Alpha1))
        {
            float distortionValue = Mathf.PingPong(Time.time, 1f); // Simulated slider value
            Debug.Log($"Simulated Audio Distortion: {distortionValue}");
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Distortion", distortionValue);
            //TODO: turn distortion off after amount of time
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            float distortionValue = Mathf.PingPong(Time.time, 1f);
            Debug.Log($"Simulated Camera Distortion: {distortionValue}");
            cameraEffects.ApplyDistortion(distortionValue);
        }
    }

    private void SpawnBarrierWall()
    {
        canSpawnBarrier = false;

        Vector3 spawnPosition = new Vector3(player.position.x, spawnHeight, player.position.z + spawnDistanceAhead);
        Instantiate(barrierWallPrefab, spawnPosition, Quaternion.identity);

        StartCoroutine(ResetBarrierCooldown());
    }

    private void SpawnMovingBlock()
    {
        canSpawnMovingBlock = false;

        Vector3 spawnPosition = new Vector3(player.position.x + movingBlockOffsetX, spawnHeight, player.position.z + spawnDistanceAhead);
        Instantiate(movingBlockPrefab, spawnPosition, Quaternion.identity);

        StartCoroutine(ResetMovingBlockCooldown());
    }

    private IEnumerator ResetBarrierCooldown()
    {
        yield return new WaitForSeconds(5f);
        canSpawnBarrier = true;
    }

    private IEnumerator ResetMovingBlockCooldown()
    {
        yield return new WaitForSeconds(6f);
        canSpawnMovingBlock = true;
    }
}
