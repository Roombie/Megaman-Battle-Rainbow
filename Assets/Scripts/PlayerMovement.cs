using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Functional Options")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canShoot = true;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool facingRight = true;
    private Vector2 moveInput;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float jumpMultiplier = 1f;
    [SerializeField] private float jumpBufferTime = 0.125f;
    [SerializeField] private float coyoteTime = 0.125f;
    private bool isJumping;
    private bool jumpButtonPressed = false;
    private float lastGroundedTime;
    private float lastJumpTime;

    [Header("Ladder Climbing")]
    [SerializeField] private float climbSpeed = 2.5f;
    [SerializeField] private float climbSpriteHeight = 0.24f;
    private bool isClimbing = false;
    private bool atLaddersEnd;
    private bool hasStartedClimbing;
    private bool startedClimbTransition;
    private bool finishedClimbTransition;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
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
        if (canJump)
        {
            CheckJump();
        }

        if (isClimbing)
        {
            Climb();
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    #region Collision detection
    private bool IsGrounded()
    {
        Vector2 position = (Vector2)transform.position + groundCheckOffset;
        float halfWidth = groundCheckWidth / 2;
        Vector2 leftRayStart = position - new Vector2(halfWidth, 0);
        Vector2 rightRayStart = position + new Vector2(halfWidth, 0);

        bool centerHit = Physics2D.Raycast(position, Vector2.down, groundCheckDistance, groundLayer);
        bool leftHit = Physics2D.Raycast(leftRayStart, Vector2.down, groundCheckDistance, groundLayer);
        bool rightHit = Physics2D.Raycast(rightRayStart, Vector2.down, groundCheckDistance, groundLayer);

        if (centerHit || leftHit || rightHit)
        {
            lastGroundedTime = Time.time;
            return true;
        }
        return false;
    }
    #endregion

    #region Movement
    private void Move()
    {
        if (canMove)
        {
            Vector2 velocity = rb.velocity;

            if (!isClimbing)
            {
                velocity.x = moveInput.x * speed;
            }
            else
            {
                velocity.y = moveInput.y * climbSpeed;
            }

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
    #endregion

    #region Jump
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
            }
        }

        if (!jumpButtonPressed && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    private void Jump()
    {
        isJumping = true;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpMultiplier);
        lastJumpTime = Time.time;
    }
    #endregion

    #region Ladder climb
    private void Climb()
    {
        float verticalInput = moveInput.y;

        if (verticalInput != 0f)
        {
            if (!startedClimbTransition && !finishedClimbTransition)
            {
                if (verticalInput > 0 && !atLaddersEnd)
                {
                    ClimbTransition(true);
                }
                else if (verticalInput < 0)
                {
                    if (atLaddersEnd)
                    {
                        ClimbTransition(false);
                    }
                    else
                    {
                        isClimbing = false;
                        rb.bodyType = RigidbodyType2D.Dynamic;
                    }
                }
            }
        }
    }

    private void ClimbTransition(bool movingUp)
    {
        StartCoroutine(ClimbTransitionCo(movingUp));
    }

    private IEnumerator ClimbTransitionCo(bool movingUp)
    {
        FreezeInput(true);
        finishedClimbTransition = false;

        Vector3 newPos = Vector3.zero;

        if (movingUp)
        {
            newPos = new Vector3(ladder.posX, transform.position.y + climbSpriteHeight, 0);
        }
        else
        {
            transform.position = new Vector3(ladder.posX, ladder.posBottomHandlerY - climbSpriteHeight, 0);
            newPos = new Vector3(ladder.posX, ladder.posBottomHandlerY, 0);
        }

        while (transform.position != newPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, newPos, climbSpeed * Time.deltaTime);
            animator.speed = 1;
            animator.Play("Player_Climb");
            yield return null;
        }

        if (!movingUp)
        {
            isClimbing = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        finishedClimbTransition = true;
        FreezeInput(false);
    }

    private void FreezeInput(bool freeze)
    {
        if (freeze)
        {
            moveInput = Vector2.zero;
            jumpButtonPressed = false;
        }
    }
    #endregion

    #region Input
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (isClimbing)
        {
            moveInput.y = context.ReadValue<float>();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpButtonPressed = true;
            lastJumpTime = Time.time;
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
    #endregion

    #region Trigger Events
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ladder") && !isClimbing)
        {
            ladder = other.GetComponent<LadderHandlers>();
            ladder.isNearLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            ladder.isNearLadder = false;
            ladder = null;
        }
    }
    #endregion

    #region Gizmos
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
    #endregion
}
