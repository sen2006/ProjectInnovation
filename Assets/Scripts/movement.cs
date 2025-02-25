using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class movement : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider col;
    private Transform tf;

    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float jumpForce = 0;
    [SerializeField] private float dashForce = 0;
    [SerializeField] private float slideTime = 0;

    private float slideTimer = 0;
    private bool isSliding = false;

    private List<GameObject> currentGroundCollisions = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        tf = GetComponent<Transform>();
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
        else if (Input.GetKeyDown(KeyCode.LeftControl) || isSliding)
        {
            Slide();
        }
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    void Dash()
    {

    }

    void Slide()
    {
        if (!isSliding)
        {
            slideTimer = slideTime;
            tf.RotateAround(new Vector3(tf.position.x, tf.position.y - col.height/2 + col.radius, tf.position.z), Vector3.right, -90);
            isSliding = true;
        }
        else if (isSliding)
        {
            if (slideTimer > 0)
            {
                slideTimer -= Time.deltaTime;
            }
            else if (slideTimer <= 0)
            {
                tf.RotateAround(new Vector3(tf.position.x, tf.position.y, tf.position.z + col.height / 2 - col.radius), Vector3.right, 90);
                isSliding = false;
            }
        }
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
