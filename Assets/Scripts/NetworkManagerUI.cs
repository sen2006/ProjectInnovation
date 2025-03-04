using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField sessionNameField;
    [SerializeField] private string sessionName;

    private void Awake()
    {
        //sessionNameField.ActivateInputField();
        sessionNameField.onValueChanged.AddListener(setNewName);

        hostButton.onClick.AddListener(() => {
            if (string.IsNullOrEmpty(sessionName)) return;
            gameManager.CreateSession(sessionName);
        });
        clientButton.onClick.AddListener(() => {
            if (string.IsNullOrEmpty(sessionName)) return;
            gameManager.JoinSession(sessionName);
        });
    }

    private void Update()
    {
        switch (gameManager.GetLocalPlayerType())
        {
            case GameManager.PlayerType.PC:
                clientButton.gameObject.SetActive(false);
                break;
            case GameManager.PlayerType.Mobile:
                hostButton.gameObject.SetActive(false);
                break;
        }
        if (gameObject.activeSelf && gameManager.connectionState == GameManager.ConnectionState.Connected)
        {
            gameObject.SetActive(false);
        }
    }

    private void setNewName(string name)
    {
        sessionName = name;
    }
}
