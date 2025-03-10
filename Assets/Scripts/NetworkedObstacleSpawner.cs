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
    [SerializeField] private float spawnDistanceAhead = 15f; // Distance ahead of player
    [SerializeField] private float movingBlockOffsetX = 2f;  // Offset for moving block
    [SerializeField] private float spawnHeight = 1f; // Adjust spawn height if needed

    private bool canSpawnBarrier = true;
    private bool canSpawnMovingBlock = true;

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

        // Spawn slightly to the side so it's not centered
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
}
