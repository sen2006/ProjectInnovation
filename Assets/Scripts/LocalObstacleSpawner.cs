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

    [Header("Distortion Cooldowns")]
    [SerializeField] private float distortionCooldownDuration = 5f; // Cooldown before DJ can change
    private bool canChangeAudioDistortion = true;
    private bool canChangeCameraDistortion = true;

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

        // Simulated DJ Controls (Only work if cooldown has ended)
        if (Input.GetKey(KeyCode.Alpha1) && canChangeAudioDistortion)
        {
            float distortionValue = Mathf.PingPong(Time.time, 1f);
            Debug.Log($"Simulated Audio Distortion: {distortionValue}");
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Distortion", distortionValue);
        }

        if (Input.GetKey(KeyCode.Alpha2) && canChangeCameraDistortion)
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

    // **🔹 New Power-up Handling**
    public void PickupAudioDistortionPowerup()
    {
        Debug.Log("Picked Up Audio Distortion Powerup - Resetting Distortion & Starting Cooldown.");
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Distortion", 0f); // Reset audio distortion
        canChangeAudioDistortion = false;
        StartCoroutine(AudioDistortionCooldown());
    }

    public void PickupCameraDistortionPowerup()
    {
        Debug.Log("Picked Up Camera Distortion Powerup - Resetting Distortion & Starting Cooldown.");
        cameraEffects.ApplyDistortion(0f); // Reset camera distortion
        canChangeCameraDistortion = false;
        StartCoroutine(CameraDistortionCooldown());
    }

    private IEnumerator AudioDistortionCooldown()
    {
        yield return new WaitForSeconds(distortionCooldownDuration);
        canChangeAudioDistortion = true;
        Debug.Log("DJ can change Audio Distortion again.");
    }

    private IEnumerator CameraDistortionCooldown()
    {
        yield return new WaitForSeconds(distortionCooldownDuration);
        canChangeCameraDistortion = true;
        Debug.Log("DJ can change Camera Distortion again.");
    }
}
