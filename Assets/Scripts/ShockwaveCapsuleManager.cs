using UnityEngine;

public class ShockwaveCapsuleManager
{
    private GameObject capsulePrefab;
    private GameObject player;

    public ShockwaveCapsuleManager(GameObject capsulePrefab, GameObject player)
    {
        this.capsulePrefab = capsulePrefab;
        this.player = player;
    }

    public GameObject CreateTravelingCapsule(Vector3 startPos)
    {
        if (capsulePrefab == null)
        {
            Debug.LogError("Capsule Prefab is not assigned!");
            return null;
        }

        GameObject capsule = Object.Instantiate(capsulePrefab, startPos, Quaternion.identity);
        CapsuleCollider collider = capsule.AddComponent<CapsuleCollider>();
        collider.height = 2.0f;
        collider.radius = 0.5f;
        collider.direction = 1; // Y-Axis

        Rigidbody rb = capsule.AddComponent<Rigidbody>();
        rb.isKinematic = true; // Make it move manually

        return capsule;
    }

    public void UpdateCapsulePosition(GameObject capsule, Vector3 origin, float radius)
    {
        if (capsule == null || player == null) return;

        Vector3 directionToPlayer = (player.transform.position - origin).normalized;
        Vector3 newCapsulePos = origin + directionToPlayer * radius;

        newCapsulePos.y = Mathf.Clamp(newCapsulePos.y, 1.0f, 3.0f); // Limit height

        capsule.transform.position = newCapsulePos;
        capsule.transform.LookAt(player.transform.position); // Make it face the player
    }

    public void DestroyCapsule(GameObject capsule)
    {
        if (capsule != null)
        {
            Object.Destroy(capsule);
        }
    }
}
