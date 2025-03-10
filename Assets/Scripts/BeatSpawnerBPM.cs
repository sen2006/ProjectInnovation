using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class BeatSpawnerBPM : MonoBehaviour
{
    private bool beatSpawnArmed = false;

    public GameObject blockPrefab; // Assign a block prefab
    public Transform spawnPoint; // Set a spawn position
    public Transform playerPos;

    private int currentBeat = 0;

    private void Start()
    {
        //if(currentBeat >= 0) currentBeat = MusicManager.me.timelineInfo.currentBeat;
    }

    private void Update()
    {
        if (currentBeat != MusicManager.me.timelineInfo.currentBeat)
        {
            beatSpawnArmed = true;
            currentBeat = MusicManager.me.timelineInfo.currentBeat;
        }

        if (beatSpawnArmed && currentBeat >= 0)
        {
            SpawnBlock();
            beatSpawnArmed = false;
        }
    }

    public void SpawnBlock()
    {
        if (blockPrefab && spawnPoint)
        {
            spawnPoint.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, playerPos.position.z);

            Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}
