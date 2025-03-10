using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class RpcTest : NetworkBehaviour
{
    public void sendRpcs()
    {
        ClientAndHostRpc(0, NetworkObjectId);
        ServerOnlyRpc(0, NetworkObjectId);
    }
    public override void OnNetworkSpawn()
    {
        //ClientAndHostRpc(0, NetworkObjectId);
        //ServerOnlyRpc(0, NetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ClientAndHostRpc(int value, ulong sourceNetworkObjectId)
    {
        if (IsHost) Debug.Log("this is a host");
        Debug.Log("this is a client");
    }

    [Rpc(SendTo.Server)]
    void ServerOnlyRpc(int value, ulong sourceNetworkObjectId)
    {
        Debug.Log("this is a server");
    }
}