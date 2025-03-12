using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using TMPro;

public class BeatSpawnerBPM : MonoBehaviour
{
    private bool beatSpawnArmed = false;

    public GameObject blockPrefab; // Assign a block prefab
    public Transform spawnPoint; // Set a spawn position
    public Transform playerPos;
    public bool trackMusic = true;

    TextMeshProUGUI mText;

    private int currentBeat = 0;
    private int currentBar = 0;
    private string tempoMarker;

    private void Start()
    {
        //if(currentBeat >= 0) currentBeat = MusicManager.me.timelineInfo.currentBeat;
        mText = blockPrefab.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (trackMusic)
        {
            if (currentBeat != MusicManager.me.timelineInfo.currentBeat)
            {
                beatSpawnArmed = true;
                currentBeat = MusicManager.me.timelineInfo.currentBeat;
                currentBar = MusicManager.me.timelineInfo.currentBar;
                tempoMarker = MusicManager.me.timelineInfo.lastMarker;
            }

            if (beatSpawnArmed && currentBeat >= 0)
            {
                SpawnBlock();
                beatSpawnArmed = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !trackMusic)
        {
            SpawnBlock();
        }
    }

    public void SpawnBlock()
    {
        if (blockPrefab && spawnPoint)
        {
            spawnPoint.position = new Vector3(spawnPoint.position.x, spawnPoint.position.y, playerPos.position.z);
            mText.text = tempoMarker
                + "\n Bar: " + currentBar 
                + "\n Beat: " + currentBeat;
            
            Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}
