using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float moveRange = 3f;

    private Vector3 startPos;
    private bool movingRight = true;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float moveAmount = moveSpeed * Time.deltaTime;

        // Move left and right in a ping-pong pattern
        transform.position += movingRight ? Vector3.right * moveAmount : Vector3.left * moveAmount;

        // Reverse direction when reaching move range
        if (Vector3.Distance(transform.position, startPos) >= moveRange)
            movingRight = !movingRight;
    }
}
