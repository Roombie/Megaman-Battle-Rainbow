using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Functional Options")]
    [Tooltip("Enables or disables moving functionality.")]
    [SerializeField] private bool canMove = true;
    [Tooltip("Enables or disables jumping functionality.")]
    [SerializeField] private bool canJump = true;
    [Tooltip("Enables or disables shooting functionality.")]
    [SerializeField] private bool canShoot = true;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool facingRight = true;
    private Vector2 moveInput;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpMultiplier = 1f;
    [Tooltip("How long I should buffer your jump input for (seconds)")]
    [SerializeField] private float jumpBufferTime = 0.125f;
    [Tooltip("How long you have to jump after leaving a ledge (seconds)")]
    [SerializeField] private float coyoteTime = 0.125f;
    private bool isJumping; // indicates whether the player is CURRENTLY in the PROCESS of JUMPING
    private bool jumpButtonPressed = false; // indicates whether the JUMP button is CURRENTLY PRESSED
    private float lastGroundedTime;
    private float lastJumpTime;

    [Header("Ladder Movement")]
    [SerializeField] private float climbSpeed = 2.5f;
    private bool isClimbing = false; // indicates whether the player is CURRENTLY climbing a ladder
    private bool isCloseToLadder = false; // determine whether character can even start interacting with the ladder at the current position
    private Transform ladder;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Vector2 groundCheckOffset = new(0f, -0.5f);
    [SerializeField] private float groundCheckWidth = 0.5f;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer sprite;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (canJump && !isClimbing)
        {
            CheckJump();
        }

        // Animator integration
        /*animator.SetBool("isGrounded", isGrounded());
        animator.SetBool("isJumping", isJumping);
        animator.SetFloat("speed", Mathf.Abs(moveInput.x));
        animator.SetBool("isClimbing", isClimbing);*/
    }

    void FixedUpdate()
    {
        if (isClimbing)
        {
            LadderClimb();
        }
        else
        {
            Move();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isCloseToLadder = true;
            this.ladder = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isCloseToLadder = false;
            isClimbing = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private bool IsGrounded()
    {
        Vector2 position = (Vector2)transform.position + groundCheckOffset;
        float halfWidth = groundCheckWidth / 2;
        Vector2 leftRayStart = position - new Vector2(halfWidth, 0);
        Vector2 rightRayStart = position + new Vector2(halfWidth, 0);

        bool centerHit = Physics2D.Raycast(position, Vector2.down, groundCheckDistance, groundLayer);
        bool leftHit = Physics2D.Raycast(leftRayStart, Vector2.down, groundCheckDistance, groundLayer);
        bool rightHit = Physics2D.Raycast(rightRayStart, Vector2.down, groundCheckDistance, groundLayer);

        Debug.DrawRay(position, Vector2.down * groundCheckDistance, centerHit ? Color.green : Color.red);
        Debug.DrawRay(leftRayStart, Vector2.down * groundCheckDistance, leftHit ? Color.green : Color.red);
        Debug.DrawRay(rightRayStart, Vector2.down * groundCheckDistance, rightHit ? Color.green : Color.red);

        if (centerHit || leftHit || rightHit)
        {
            lastGroundedTime = Time.time;
            return true;
        }
        return false;
    }

    private void Move()
    {
        if (canMove)
        {
            Vector2 velocity = rb.velocity;
            velocity.x = moveInput.x * speed;
            rb.velocity = velocity;

            if (moveInput.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (moveInput.x < 0 && facingRight)
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void CheckJump()
    {
        if (isJumping && rb.velocity.y <= 0)
        {
            isJumping = false;
        }

        if (jumpButtonPressed)
        {
            if ((Time.time - lastJumpTime <= jumpBufferTime) && (IsGrounded() || (Time.time - lastGroundedTime <= coyoteTime)))
            {
                Jump();
                Debug.Log("Jump");
            }
        }
    }

    private void Jump()
    {
        isJumping = true;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpMultiplier);
        lastJumpTime = Time.time;
    }

    private void LadderClimb()
    {
        Vector2 velocity = rb.velocity;
        velocity.y = moveInput.y * climbSpeed;
        rb.velocity = velocity;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Align to ladder
        if (ladder != null)
        {
            Vector2 position = transform.position;
            position.x = ladder.position.x;
            transform.position = position;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (isCloseToLadder && Mathf.Abs(moveInput.y) > 0f)
        {
            isClimbing = true;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isClimbing)
            {
                isClimbing = false;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
            else
            {
                jumpButtonPressed = true;
                lastJumpTime = Time.time;
            }
        }

        if (context.canceled)
        {
            jumpButtonPressed = false;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (canShoot)
                Debug.Log("Shoot");
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 position = (Vector2)transform.position + groundCheckOffset;
        float halfWidth = groundCheckWidth / 2;
        Vector2 leftRayStart = position - new Vector2(halfWidth, 0);
        Vector2 rightRayStart = position + new Vector2(halfWidth, 0);

        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawLine(position, position + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(leftRayStart, leftRayStart + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(rightRayStart, rightRayStart + Vector2.down * groundCheckDistance);
    }
}
