using Unity.Netcode;
using UnityEngine;

/// <summary>
/// this is an explanation script to help you with networking
/// </summary>


// first make sure to replace the "MonoBehavior" with "NetworkBehavior"
public class NetworkingExplanation : NetworkBehaviour
{
    int localValue;
    NetworkVariable<int> networkValue;

    // Rpc's will always run Only on where they are sent to (for all possible options Type "SendTo." and look through the sugestions or ctrl click SendTo)

    // Rpc functions must always end with "Rpc" in the name

    // Rpc functions can both be public and private but can not be Async
    [Rpc(SendTo.Server)]
    void FunctionRpc()
    {
        // any code here will only be run on the server

        // any variables here will only be changed on the server
        localValue = 2;
        // when getting this value on any othe location than the server will result in 0 or the value localy set

        // when you want a value that is synced over the network use NetworkVariables
        networkValue.Value = 2;
        // when getting this on any location it WILL result in 2
   
        // NetworkVariables can not hold Objects like GameObjects for that use the peramiters of Rpc's
    }

    // i like using Awake over start. it is slightly more optimal
    private void Awake()
    {
        // in case you want to call a function when a NetworkVariable is changed you can add it to the "OnValueChanged"
        networkValue.OnValueChanged += OnNetworkValueChanged;
    }

    private void OnNetworkValueChanged(int oldVal, int newVal)
    {
        // here you can do stuff when the value changes
    }
}
