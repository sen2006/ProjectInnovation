using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class movement : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider col;
    private Transform tf;

    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float strafeSpeed = 0;
    [SerializeField] private float jumpForce = 0;
    [SerializeField] private float slideMult = 0;

    [SerializeField] private LayerMask groundLayer;

    [SerializeField] Transform cameraTransfrom;

    private float colliderHeight;

    //TODO: add serializable controls

    void Start()
    {
        Debug.Assert(cameraTransfrom != null, "cameraTransform is not assigned");

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        tf = GetComponent<Transform>();

        colliderHeight = col.height;
    }

    void Update()
    {
        MoveForward();
        InputHandler();
    }

    void MoveForward()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, moveSpeed);
    }

    void InputHandler()
    {
        Strafe();

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            Jump();
        }

        Slide(Input.GetKey(KeyCode.LeftControl));
    }

    void Strafe()
    {
        rb.linearVelocity = new Vector3(Input.GetAxis("Horizontal") * strafeSpeed, rb.linearVelocity.y, rb.linearVelocity.z);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    void Slide(bool shouldSlide)
    {
        if (shouldSlide && IsGrounded())
        {
            col.height = colliderHeight * slideMult;
            col.center = new Vector3(0, -col.height / 2, 0);
        }
        else
        {
            col.height = colliderHeight;
            col.center = Vector3.zero;
        }

        cameraTransfrom.localPosition = col.center + new Vector3(0, col.height / 2 - col.radius, 0);
    }

    private bool IsGrounded()
    {
        Vector3 pos = tf.position;
        return Physics.CapsuleCast(new Vector3(pos.x, pos.y - transform.localScale.y + col.radius, pos.z),
            new Vector3(pos.x, pos.y + col.height / 2 - col.radius, pos.z), col.radius - Physics.defaultContactOffset,
            Vector3.down, .1f, groundLayer);
    }
}
