using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class movement : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;

    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float jumpForce = 0;
    [SerializeField] private float dashForce = 0;

    private List<GameObject> currentGroundCollisions = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3 (rb.velocity.x, rb.velocity.y, moveSpeed);
    }

    void Update()
    {
        InputHandler();
    }

    void InputHandler()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentGroundCollisions.Count > 0)
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    void Dash()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            currentGroundCollisions.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (currentGroundCollisions.Contains(collision.gameObject))
        {
            currentGroundCollisions.Remove(collision.gameObject);
        }
    }
}
