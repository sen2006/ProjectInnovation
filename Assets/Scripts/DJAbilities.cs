using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Services.Core;

public class DJ : NetworkBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button bassSlamButton;
    [SerializeField] private Button glitchStormButton;
    [SerializeField] private Button controlLagButton;
    [SerializeField] private Button invertWorldButton;


    [Header("Invert World")]
    [SerializeField] private int invertDurationMS = 0;
    [SerializeField] private CameraManager cameraManager;



    private async void Start()
    {
        await UnityServices.InitializeAsync();

        bassSlamButton.onClick.AddListener(BassSlamRpc);
        glitchStormButton.onClick.AddListener(GlitchStormRpc);
        controlLagButton.onClick.AddListener(ControlLagRpc);
        invertWorldButton.onClick.AddListener(InvertWorldRpc);
    }

    [Rpc(SendTo.Server)]
    private void BassSlamRpc()
    {

    }

    [Rpc(SendTo.Server)]
    private void GlitchStormRpc()
    {

    }

    [Rpc(SendTo.Server)]
    private void ControlLagRpc()
    {

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
}
