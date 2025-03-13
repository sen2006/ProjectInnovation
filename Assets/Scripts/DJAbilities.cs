using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Services.Core;
using System.Collections;

public class DJAbilities : NetworkBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button bassSlamButton;
    [SerializeField] private Button glitchStormButton;
    [SerializeField] private Button controlLagButton;
    [SerializeField] private Button invertWorldButton;
    [SerializeField] private Button spawnWallObjectButton;
    [SerializeField] private Button movingBlockButton;
    //[SerializeField] private Button Button;
    //[SerializeField] private Button Button;

    [Header("Bass Slam")]
    [SerializeField] private ShockwaveAbility shockwaveAbility;

    [Header("Glitch Storm")]
    [SerializeField] private int stormDurationMS = 0;

    [Header("Control Lag")]
    [SerializeField] private int controlLagDurationMS = 0;
    [SerializeField] private float controlLagDelay = 0;
    [SerializeField] private Movement movement;


    [Header("Invert World")]
    [SerializeField] private int invertDurationMS = 0;
    [SerializeField] private CameraManager cameraManager;

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



    private async void Start()
    {
        await UnityServices.InitializeAsync();

        bassSlamButton.onClick.AddListener(BassSlamRpc);
        glitchStormButton.onClick.AddListener(GlitchStormRpc);
        controlLagButton.onClick.AddListener(ControlLagRpc);
        invertWorldButton.onClick.AddListener(InvertWorldRpc);
        spawnWallObjectButton.onClick.AddListener(SpawnBarrierWallServerRpc);
        movingBlockButton.onClick.AddListener(SpawnMovingBlockRpc);
    }

    [Rpc(SendTo.Server)]
    private void BassSlamRpc()
    {
        //shockwaveAbility.AddShockwave(new ShockwaveAbility.Shockwave(new Vector3(1, 1, 1), Time.time));
    }

    [Rpc(SendTo.Server)]
    private void GlitchStormRpc()
    {
        GlitchStormAsync(stormDurationMS);
    }

    private async void GlitchStormAsync(int ms)
    {

        await Task.Delay(ms);

    }

    [Rpc(SendTo.Server)]
    private void ControlLagRpc()
    {
        ControlLagAsync(controlLagDurationMS);
    }

    private async void ControlLagAsync(int ms)
    {
        movement.SetDelay(controlLagDelay);
        await Task.Delay(ms);
        movement.SetDelay(0);
    }

    [Rpc(SendTo.Server)]
    private void InvertWorldRpc()
    {
        InvertWorldAsync(invertDurationMS);
    }

    private async void InvertWorldAsync(int ms)
    {
        cameraManager.setFlipped(true);
        await Task.Delay(ms);
        cameraManager.setFlipped(false);
    }

    // ---- 

    [Rpc(SendTo.Server)]
    public void SpawnBarrierWallServerRpc()
    {
        if (!canSpawnBarrier) return;
        canSpawnBarrier = false;

        Vector3 spawnPosition = new Vector3(player.position.x, spawnHeight, player.position.z + spawnDistanceAhead);
        GameObject newBarrier = Instantiate(barrierWallPrefab, spawnPosition, Quaternion.identity);
        newBarrier.GetComponent<NetworkObject>().Spawn();

        StartCoroutine(ResetBarrierCooldown());
    }

    [Rpc(SendTo.Server)]
    public void SpawnMovingBlockRpc()
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
    [Rpc(SendTo.Server)]
    public void PickupAudioDistortionPowerupRpc()
    {
        UpdateAudioDistortionClientRpc(0f);
        canChangeAudioDistortion = false;
        StartCoroutine(AudioDistortionCooldown());
    }

    [Rpc(SendTo.Server)]
    public void PickupCameraDistortionPowerupRpc()
    {
        UpdateCameraDistortionClientRpc(0f);
        canChangeCameraDistortion = false;
        StartCoroutine(CameraDistortionCooldown());
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateAudioDistortionClientRpc(float distortionValue)
    {
        Debug.Log($"Audio Distortion Reset: {distortionValue}");
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Distortion", distortionValue);
    }

    [Rpc(SendTo.ClientsAndHost)]
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

