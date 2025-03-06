using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class BeatSpawnerBPM : MonoBehaviour
{
    public string eventPath = "event:/YourMusicEvent"; // Replace with your FMOD event path
    public GameObject blockPrefab; // Assign a block prefab
    public Transform spawnPoint; // Set a spawn position
    public Transform playerPos;
    public float bpm = 120f; // Set the BPM of the song

    private EventInstance musicInstance;
    private float beatInterval;
    private float nextBeatTime;

    void Start()
    {
        // Calculate beat interval (seconds per beat)
        beatInterval = 60f / bpm;
        nextBeatTime = Time.time;

        // Start the music
        musicInstance = RuntimeManager.CreateInstance(eventPath);
        musicInstance.start();
    }

    void Update()
    {
        if (Time.time >= nextBeatTime)
        {
            SpawnBlock();
            nextBeatTime += beatInterval; // Schedule next beat
        }
    }

    void SpawnBlock()
    {
        if (blockPrefab && spawnPoint)
        {
            spawnPoint.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, playerPos.position.z);

            Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    void OnDestroy()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicInstance.release();
    }
}
