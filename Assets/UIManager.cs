using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject PcUI;
    [SerializeField] GameObject MobileUI;
    [SerializeField] GameManager gameManager;
    [SerializeField] bool enableAutoUIToggling = true;

    private void Awake()
    {
        Debug.Assert(PcUI != null, "No PC UI assigned");
        Debug.Assert(MobileUI != null, "No Mobile UI assigned");
        Debug.Assert(gameManager != null, "No GameManager assigned");
    }

    private void Update()
    {
        AutoToggleUI();
    }

    private void AutoToggleUI()
    {
        if (!enableAutoUIToggling) return;
        if (gameManager.connectionState != GameManager.ConnectionState.Connected)
        {
            PcUI.SetActive(false);
            MobileUI.SetActive(false);
            return;
        }

        switch (gameManager.playerType)
        {
            case GameManager.PlayerType.PC:
                PcUI.SetActive(true);
                MobileUI.SetActive(false);
                break;
            case GameManager.PlayerType.Mobile:
                PcUI.SetActive(false);
                MobileUI.SetActive(true);
                break;
            default:
                PcUI.SetActive(false);
                MobileUI.SetActive(false);
                break;
        }
    }
}
