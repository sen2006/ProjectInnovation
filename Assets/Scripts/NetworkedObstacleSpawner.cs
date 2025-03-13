using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkedObstacleSpawner : NetworkBehaviour
{
    [Header("Obstacle Prefabs")]
    [SerializeField] private GameObject barrierWallPrefab;
    [SerializeField] private GameObject movingBlockPrefab;

    [Header("References")]
    [SerializeField] private Transform player;  // The moving player reference

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistanceAhead = 15f;
    [SerializeField] private float movingBlockOffsetX = 2f;
    [SerializeField] private float spawnHeight = 1f;

    [Header("Distortion Effects")]
    [SerializeField] private CameraEffects cameraEffects;
    [SerializeField] private float distortionCooldownDuration = 5f;

    private bool canSpawnBarrier = true;
    private bool canSpawnMovingBlock = true;
    private bool canChangeAudioDistortion = true;
    private bool canChangeCameraDistortion = true;

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBarrierWallServerRpc()
    {
        if (!canSpawnBarrier) return;
        canSpawnBarrier = false;

        Vector3 spawnPosition = new Vector3(player.position.x, spawnHeight, player.position.z + spawnDistanceAhead);
        GameObject newBarrier = Instantiate(barrierWallPrefab, spawnPosition, Quaternion.identity);
        newBarrier.GetComponent<NetworkObject>().Spawn();

        StartCoroutine(ResetBarrierCooldown());
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnMovingBlockServerRpc()
    {
        if (!canSpawnMovingBlock) return;
        canSpawnMovingBlock = false;

        Vector3 spawnPosition = new Vector3(player.position.x + movingBlockOffsetX, spawnHeight, player.position.z + spawnDistanceAhead);
        GameObject newBlock = Instantiate(movingBlockPrefab, spawnPosition, Quaternion.identity);
        newBlock.GetComponent<NetworkObject>().Spawn();

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

    // **🔹 Synchronize Power-up Effects**
    [ServerRpc(RequireOwnership = false)]
    public void PickupAudioDistortionPowerupServerRpc()
    {
        UpdateAudioDistortionClientRpc(0f);
        canChangeAudioDistortion = false;
        StartCoroutine(AudioDistortionCooldown());
    }

    [ServerRpc(RequireOwnership = false)]
    public void PickupCameraDistortionPowerupServerRpc()
    {
        UpdateCameraDistortionClientRpc(0f);
        canChangeCameraDistortion = false;
        StartCoroutine(CameraDistortionCooldown());
    }

    [ClientRpc]
    private void UpdateAudioDistortionClientRpc(float distortionValue)
    {
        Debug.Log($"Audio Distortion Reset: {distortionValue}");
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Distortion", distortionValue);
    }

    [ClientRpc]
    private void UpdateCameraDistortionClientRpc(float distortionValue)
    {
        cameraEffects.ApplyDistortion(distortionValue);
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
