using Unity.Netcode;
using UnityEngine;

public class DebugObstacleSpawner : MonoBehaviour
{
    private DJAbilities obstacleSpawner;

    private void Start()
    {
        obstacleSpawner = FindObjectOfType<DJAbilities>();
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return; // Ensure only the Host can test

        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Spawning Barrier Wall (Test Mode)");
            obstacleSpawner.SpawnBarrierWallServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Spawning Moving Block (Test Mode)");
            //obstacleSpawner.SpawnMovingBlockServerRpc();
        }
    }
}
