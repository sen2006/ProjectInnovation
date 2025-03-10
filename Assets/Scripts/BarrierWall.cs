using Unity.Netcode;
using UnityEngine;

public class BarrierWall : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 10f); // Auto-destroy after 10s
    }
}
