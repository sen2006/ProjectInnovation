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

        /*playerType=PlayerType.PC;
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            playerType = PlayerType.Mobile;
        }*/
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        var id = NetworkManager.Singleton.LocalClientId;
        Debug.Log("LocalClientID:" + id);
        switch (id)
        {
            default:
            case 1: playerType = PlayerType.PC; break;
            case 2: playerType = PlayerType.Mobile; break;
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
        if (connectionState != ConnectionState.Disconnected) return;
        CreateSessionAsync(sessionName);
    }

    public void JoinSession(string sessionName)
    {
        if (connectionState != ConnectionState.Disconnected) return;
        JoinSessionAsync(sessionName);
    }

    private async Task CreateSessionAsync(string sessionName)
    {
        if (connectionState != ConnectionState.Disconnected) return;
        connectionState = ConnectionState.Connecting;

        try
        {
            //try
            //{
                AuthenticationService.Instance.SwitchProfile("player1");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //}
            //catch (Exception e) { Debug.LogException(e); }

            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = 2
            }.WithDistributedAuthorityNetwork();

            //session = await MultiplayerService.Instance.CreateSessionAsync(options);  
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
            try
            {

                AuthenticationService.Instance.SwitchProfile("player2");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e){ Debug.LogException(e); }

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
