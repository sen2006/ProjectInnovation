using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System.Threading.Tasks;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class Movement : MonoBehaviour
{
    // Components
    private Rigidbody rb;
    private CapsuleCollider col;
    private Transform tf;

    // Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float strafeSpeed = 8f;
    [SerializeField] private float strafeAirControl = 6f;

    // Jump Settings
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 20f; // Faster Jumping

    // Sliding Settings
    [Header("Sliding Settings")]
    [SerializeField] private float slideMult = 0.5f;

    // Gravity & Physics
    [Header("Gravity & Physics")]
    [SerializeField] private float gravityForce = 15f; // Adjusted Gravity

    // Timing Mechanics
    [Header("Timing Mechanics")]
    [SerializeField] private float coyoteTime = 0.1f; // Coyote Time Duration
    [SerializeField] private float jumpBufferTime = 0.1f; // Jump Buffer Duration

    // Wall Running
    [Header("Wall Running Settings")]
    [SerializeField] private float wallRunGravity = 0.5f;
    [SerializeField] private float wallRunSpeed = 10f;
    [SerializeField] private float jumpOffForce = 15f;
    [SerializeField] private float wallDetectionRange = 1.2f;
    [SerializeField] private float wallRunCooldown = 0.5f;
    [SerializeField] private float jumpPushForce = 10f;

    // Camera Effects
    [Header("Camera Effects")]
    [SerializeField] private float cameraTiltAmount = 15f;
    [SerializeField] private float cameraTiltSpeed = 5f;

    // Layers
    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    // References
    [Header("References")]
    [SerializeField] Transform cameraTransform;

    // Internal State
    private float colliderHeight;
    private bool controlLocked = false;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isWallRunning = false;
    private bool isWallLeft, isWallRight;
    private bool canWallRun = true;

    // Control Delay Time
    public float delayTime = 0;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        Debug.Assert(cameraTransform != null, "cameraTransform is not assigned");

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        tf = GetComponent<Transform>();

        colliderHeight = col.height;
        rb.useGravity = false; // We handle gravity manually
    }

    void Update()
    {
        ApplyGravity();
        MoveForward();
        InputHandler();
        CheckForWalls();
        HandleWallRunning();
        HandleWallJump();

        // Update Grounded Status
        isGrounded = IsGrounded();

        // Handle Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Handle Jump Buffer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    void MoveForward()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, moveSpeed);
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            rb.linearVelocity += Vector3.down * gravityForce * Time.deltaTime;
        }
    }


    void InputHandler()
    {
        Strafe();

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
             Jump();
            jumpBufferCounter = 0; // Reset buffer after jump
        }

        Slide(Input.GetKey(KeyCode.LeftControl));
    }

    void CheckForWalls()
    {
        if (!canWallRun) return;

        isWallLeft = IsOnWallLeft();
        isWallRight = IsOnWallRight();
    }

    void HandleWallRunning()
    {
        if ((isWallLeft || isWallRight) && !IsGrounded() && canWallRun)
        {
            StartWallRun();
        }
        else
        {
            StopWallRun();
        }
    }

    void StartWallRun()
    {
        isWallRunning = true;
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallRunGravity, wallRunSpeed);

        float targetTilt = isWallLeft ? -cameraTiltAmount : cameraTiltAmount;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetTilt);
        cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, targetRotation, Time.deltaTime * cameraTiltSpeed);
    }

    void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;

        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
        cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, targetRotation, Time.deltaTime * cameraTiltSpeed);
    }

    void HandleWallJump()
    {
        if (isWallRunning && Input.GetKeyDown(KeyCode.Space))
        {
            // Apply a stronger sideways push, with a small vertical boost
            Vector3 jumpDirection = (isWallLeft ? transform.right : -transform.right) * jumpPushForce + Vector3.up * jumpOffForce * 0.5f;
            rb.linearVelocity = jumpDirection;

            StopWallRun();
            StartCoroutine(WallRunCooldown());
        }
    }

    IEnumerator WallRunCooldown()
    {
        canWallRun = false;
        yield return new WaitForSeconds(wallRunCooldown);
        canWallRun = true;
    }

    void Strafe()
    {
        if (controlLocked) return;

        float horizontalInput = Input.GetAxis("Horizontal");

        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(horizontalInput * strafeSpeed, rb.linearVelocity.y, rb.linearVelocity.z);
        }
        else
        {
            rb.AddForce(new Vector3(horizontalInput * strafeAirControl, 0, 0), ForceMode.Acceleration);
            rb.linearVelocity = new Vector3(Mathf.Clamp(rb.linearVelocity.x, -strafeSpeed, strafeSpeed), rb.linearVelocity.y, rb.linearVelocity.z);

            // ** Improved Air Control: Smooth Stop When Input is Released **
            if (Mathf.Approximately(horizontalInput, 0))
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.85f, rb.linearVelocity.y, rb.linearVelocity.z);
            }
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        coyoteTimeCounter = 0; // Reset coyote time after jump
    }

    void Slide(bool shouldSlide)
    {
        if (shouldSlide && isGrounded)
        {
            col.height = colliderHeight * slideMult;
            col.center = new Vector3(0, -col.height / 2, 0);
            cameraTransform.GetComponent<CameraEffects>().ApplySlideEffect(0.7f, true); // Move down & tilt BACKWARD

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, moveSpeed * 1.5f); // Temporary speed boost
        }
        else
        {
            col.height = colliderHeight;
            col.center = Vector3.zero;
            cameraTransform.GetComponent<CameraEffects>().ApplySlideEffect(0f, false); // Reset camera position & tilt
        }
    }




    private bool IsGrounded()
    {
        Vector3 pos = tf.position;
        return Physics.CapsuleCast(new Vector3(pos.x, pos.y - transform.localScale.y + col.radius, pos.z),
            new Vector3(pos.x, pos.y + col.height / 2 - col.radius, pos.z), col.radius - Physics.defaultContactOffset,
            Vector3.down, .1f, groundLayer);
    }

    private bool IsOnWallLeft()
    {
        Vector3 pos = tf.position;
        return Physics.CapsuleCast(new Vector3(pos.x, pos.y - transform.localScale.y + col.radius, pos.z),
            new Vector3(pos.x, pos.y + col.height / 2 - col.radius, pos.z), col.radius - Physics.defaultContactOffset,
            Vector3.left, .1f, wallLayer);
    }

    private bool IsOnWallRight()
    {
        Vector3 pos = tf.position;
        return Physics.CapsuleCast(new Vector3(pos.x, pos.y - transform.localScale.y + col.radius, pos.z),
            new Vector3(pos.x, pos.y + col.height / 2 - col.radius, pos.z), col.radius - Physics.defaultContactOffset,
            Vector3.right, .1f, wallLayer);
    }
}