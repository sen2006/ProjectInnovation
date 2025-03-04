using Unity.Netcode;
using UnityEngine;

public class NetworkStates : NetworkBehaviour
{
    public NetworkVariable<Vector3> playerPosition {  get; private set; } = new NetworkVariable<Vector3>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);




}
