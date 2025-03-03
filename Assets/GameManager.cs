using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Core;

public class GameManager : NetworkBehaviour
{
    public enum PlayerType
    {
        None,
        PC,
        Mobile
    }

    public enum ConnectionState
    {
        None,
        Disconnected,
        Connecting,
        Connected,
    }

    public PlayerType playerType { get; private set; }
    public ConnectionState connectionState { get; private set; }
    private ISession session;

    public async void Awake()
    {
        await UnityServices.InitializeAsync();
        connectionState = ConnectionState.Disconnected;
        //if (Instance==null) Instance = this;
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        var id = NetworkManager.Singleton.LocalClientId;
        Debug.Log("LocalClientID:" + id);
        switch (id)
        {
            default:
            case 0: playerType = PlayerType.PC; break;
            case 1: playerType = PlayerType.Mobile; break;
        }
        Debug.Log("LocalClientType:" + GetLocalPlayerType());
        
    }

    public PlayerType GetLocalPlayerType() { return playerType; }


    public override void OnDestroy()
    {
        session?.LeaveAsync();
        base.OnDestroy();
    }

    public void CreateSession(string sessionName)
    {
        CreateSessionAsync(sessionName);
    }

    public void JoinSession(string sessionName)
    {
        JoinSessionAsync(sessionName);
    }

    private async Task CreateSessionAsync(string sessionName)
    {
        if (connectionState != ConnectionState.Disconnected) return;
        connectionState = ConnectionState.Connecting;

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SwitchProfile(playerType.ToString());
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = 2
            }.WithDistributedAuthorityNetwork();

            session = await MultiplayerService.Instance.CreateSessionAsync(options);

            connectionState = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            connectionState = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
    }

    private async Task JoinSessionAsync(string sessionName)
    {
        if (connectionState != ConnectionState.Disconnected) return;
        connectionState = ConnectionState.Connecting;

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SwitchProfile(playerType.ToString());
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionName);

            connectionState = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            connectionState = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
    }
}
