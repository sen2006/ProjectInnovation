using UnityEngine;
using Unity.Services.Core;
using System.Threading.Tasks;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class movement : MonoBehaviour
{
    private Rigidbody rb;
    private CapsuleCollider col;
    private Transform tf;

    [SerializeField] private float moveSpeed = 0;
    [SerializeField] private float strafeSpeed = 0;
    [SerializeField] private float strafeAirControl = 0;
    [SerializeField] private float jumpForce = 0;
    [SerializeField] private float wallJumpForce = 0;
    [SerializeField] private int   wallJumpControlGracePeriodMS = 0;
    [SerializeField] private float slideMult = 0;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [SerializeField] Transform cameraTransfrom;

    private float colliderHeight;

    private bool controlLocked = false;

    //TODO: add serializable controls

    async void Start()
    {
        await UnityServices.InitializeAsync();
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
        WallRide();
        Debug.Log(rb.linearVelocity.z);
    }

    void MoveForward()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, moveSpeed);
    }

    void InputHandler()
    {
        Strafe();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        Slide(Input.GetKey(KeyCode.LeftControl));
    }

    void WallRide()
    {
        if (((IsOnWallLeft() && Input.GetKey(KeyCode.A)) || (IsOnWallRight() && Input.GetKey(KeyCode.D))) && !controlLocked)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    void Strafe()
    {
        if (controlLocked) return;

        if (IsGrounded())
        {
            rb.linearVelocity = new Vector3(Input.GetAxis("Horizontal") * strafeSpeed, rb.linearVelocity.y, rb.linearVelocity.z);
        }
        else
        {
            rb.AddForce(new Vector3(Input.GetAxis("Horizontal") * strafeAirControl, 0, 0));
            rb.linearVelocity = new Vector3(Mathf.Clamp(rb.linearVelocity.x, -strafeSpeed, strafeSpeed), rb.linearVelocity.y, rb.linearVelocity.z);
        }
    }
        

    void Jump()
    {
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
        else if (IsOnWallLeft())
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.linearVelocity = new Vector3(wallJumpForce, jumpForce, rb.linearVelocity.z);
            LockStrafe(wallJumpControlGracePeriodMS);
        }
        else if (IsOnWallRight())
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.linearVelocity = new Vector3(-wallJumpForce, jumpForce, rb.linearVelocity.z);
            LockStrafe(wallJumpControlGracePeriodMS);
        }
        
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

    private async void LockStrafe(int ms)
    {
        if(controlLocked) return;

        controlLocked = true;
        await Task.Delay(ms);
        controlLocked = false;
    }
}
