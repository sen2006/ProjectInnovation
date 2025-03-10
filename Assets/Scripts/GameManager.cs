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
    [SerializeField] NetworkManager networkManager;


    [SerializeField] RpcTest rpcTest;



    public async void Awake()
    {
        await UnityServices.InitializeAsync();
        connectionState = ConnectionState.Disconnected;
        //if (Instance==null) Instance = this;

        //playerType=PlayerType.PC;
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            //playerType = PlayerType.Mobile;
        }

        networkManager.OnClientConnectedCallback += OnJoin;
    }


    public void OnJoin(ulong clientId)
    {
        base.OnNetworkSpawn();
        var id = networkManager.LocalClientId;
        Debug.Log("LocalClientID:" + id);
        switch (id)
        {
            case 1: playerType = PlayerType.PC; break;
            case 2: playerType = PlayerType.Mobile; break;
        }
        Debug.Log("LocalClientType:" + GetLocalPlayerType());

        rpcTest.sendRpcs();
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
        //_=CreateSessionAsync(sessionName);
        _=CreateOrJoinSessionAsync("player1", sessionName);
    }

    public void JoinSession(string sessionName)
    {
        if (connectionState != ConnectionState.Disconnected) return;
        //_=JoinSessionAsync(sessionName);
        _=CreateOrJoinSessionAsync("player2", sessionName);
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
            //try
            //{

                AuthenticationService.Instance.SwitchProfile("player2");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            //}
            //catch (Exception e){ Debug.LogException(e); }

            /*var options = new QuerySessionsOptions()
            {

            };*/

            //QuerySessionsResults result = await MultiplayerService.Instance.QuerySessionsAsync(options);
            //Debug.Log(result.Sessions);
            session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionName);

            connectionState = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            connectionState = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
    }

    private async Task CreateOrJoinSessionAsync(string profileName, string sessionName)
    {
        connectionState = ConnectionState.Connecting;

        try
        {
            try
            {
                AuthenticationService.Instance.SwitchProfile(profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = 2
            }.WithDistributedAuthorityNetwork();

            session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);

            connectionState = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            connectionState = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
    }
}
