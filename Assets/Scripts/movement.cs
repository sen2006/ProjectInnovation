using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class movement : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider col;
    private Transform tf;

    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float jumpForce = 0;
    [SerializeField] private float dashForce = 0;
    [SerializeField] private float slideMult = 0;

    private float colliderHeight;

    //TODO: add serializable controls

    private List<GameObject> currentGroundCollisions = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        tf = GetComponent<Transform>();

        colliderHeight = col.height;
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
        
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Dash();
        }

        Slide(Input.GetKey(KeyCode.LeftControl));
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    void Dash()
    {

    }

    void Slide(bool shouldSlide)
    {
        if (shouldSlide && currentGroundCollisions.Count > 0)
        {
            col.height = colliderHeight * slideMult;
            col.center = new Vector3(0, -col.height / 2, 0);
        }
        else
        {
            col.height = colliderHeight;
            col.center = Vector3.zero;
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
