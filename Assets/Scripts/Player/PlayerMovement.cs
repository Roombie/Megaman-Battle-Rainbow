using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Functional Options")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canShoot = true;

    [Header("Health state")]
    public int currentHealth;
    public int maxHealth = 28;
    private bool isTakingDamage;
    private bool isInvincible;
    private bool hitSideRight;

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

    [Header("Shooting")]
    [SerializeField] int bulletDamage = 1;
    [SerializeField] float bulletSpeed = 15f;
    [SerializeField] float shootDelay = 0.2f;
    [SerializeField] Transform bulletShootPosition;
    [SerializeField] GameObject bulletPrefab;
    private bool isShooting;
    private float shootTime;
    private float shootTimeLength;
    private bool shootButtonPressed = false;
    private bool shootButtonRelease;
    private float shootButtonReleaseTimeLength;

    [Header("Ladder Climbing")]
    [SerializeField] private float climbSpeed = 2.5f;
    private bool isClimbing;
    private float transformY;
    private float transformHY;
    private bool isClimbingDown;
    private bool atLaddersEnd;
    private bool hasStartedClimbing;
    private bool startedClimbTransition;
    private bool finishedClimbTransition;
    [HideInInspector] public LadderHandlers ladder; // Don't delete this

    [Header("Ladder Settings")]
    [SerializeField] float climbSpriteHeight = 0.24f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Vector2 groundCheckOffset = new(0f, -0.5f);
    [SerializeField] private float groundCheckWidth = 0.5f;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // start at full health
        currentHealth = maxHealth;
        // start facing right always
        facingRight = true;
    }

    void Update()
    {
        if (canJump)
        {
            CheckJump();
        }

        if (canShoot)
        {
            PlayerShoot();
        }

        if (isTakingDamage)
        {
            animator.Play("Player_Hit");
            return;
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

    #region Health & Damage state
    public void HitSide(bool rightSide)
    {
        // determines the push direction of the hit animation
        hitSideRight = rightSide;
    }

    public void Invincible(bool invincibility)
    {
        isInvincible = invincibility;
    }

    public void TakeDamage(int damage)
    {
        // take damage if not invincible
        if (!isInvincible)
        {
            // take damage amount from health and update the health bar
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
            UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);

            // no more health means defeat, otherwise take damage
            if (currentHealth <= 0)
            {
                Defeat();
            }
            else
            {
                StartDamageAnimation();
            }
        }
    }

    private void Defeat()
    {
        // Logic for defeat (e.g., player death, game over screen)
        Destroy(gameObject);
    }

    private void StartDamageAnimation()
    {
        // once isTakingDamage is true in the Update function we'll play the Hit animation
        // here we go invincible so we don't repeatedly take damage, determine the X push force
        // depending which side we were hit on, and then apply that force
        if (!isTakingDamage)
        {
            isTakingDamage = true;
            isInvincible = true;
            float hitForceX = 0.50f;
            float hitForceY = 1.5f;
            if (hitSideRight) hitForceX = -hitForceX;
            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(hitForceX, hitForceY), ForceMode2D.Impulse);
        }
    }
    
    // It's referenced as an Animation Event
    void StopDamageAnimation()
    {
        // this function is called at the end of the Hit animation
        // and we reset the animation because it doesn't loop otherwise
        // we can end up stuck in it
        isTakingDamage = false;
        isInvincible = false;
        animator.Play("Player_Hit", -1, 0f);
    }
    #endregion

    #region Movement
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
        Debug.Log("Jump");
        isJumping = true;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce * jumpMultiplier);
        lastJumpTime = Time.time;
    }
    #endregion

    #region Shoot
    private void PlayerShoot()
    {
        shootTimeLength = 0;
        shootButtonReleaseTimeLength = 0;

        // shoot key is being pressed and key release flag true
        if (shootButtonPressed && shootButtonRelease)
        {
            isShooting = true;
            shootButtonRelease = false;
            shootTime = Time.time;
            Invoke("ShootBullet", shootDelay);
            Debug.Log("Shoot Bullet"); // Shoot Bullet
        }

        // shoot key isn't being pressed and key release flag is false
        if (!shootButtonPressed && !shootButtonRelease)
        {
            shootButtonReleaseTimeLength = Time.time - shootTime;
            shootButtonRelease = true;
        }

        // while shooting limit its duration
        if (isShooting)
        {
            shootTimeLength = Time.time - shootTime;
            if (shootTimeLength >= 0.25f || shootButtonReleaseTimeLength > 0.15f)
            {
                isShooting = false;
            }
        }
    }

    private void ShootBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletShootPosition.position, Quaternion.identity);
        bullet.name = bulletPrefab.name;
        bullet.GetComponent<Bullet>().SetDamageValue(bulletDamage);
        bullet.GetComponent<Bullet>().SetBulletSpeed(bulletSpeed);
        bullet.GetComponent<Bullet>().SetBulletDirection(facingRight ? Vector2.right : Vector2.left);
        bullet.GetComponent<Bullet>().Shoot();
    }
    #endregion

    #region Climbing
    // reset our ladder climbing variables and 
    // put back the animator speed and rigidbody type
    private void ResetClimbing()
    {
        // reset climbing if we're climbing
        if (isClimbing)
        {
            isClimbing = false;
            atLaddersEnd = false;
            startedClimbTransition = false;
            finishedClimbTransition = false;
            animator.speed = 1;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = Vector2.zero;
        }
    }
    #endregion

    #region Input
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpButtonPressed = true;
            lastJumpTime = Time.time;
        } 
        else if (context.canceled)
        {
            jumpButtonPressed = false;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
           shootButtonPressed = true;
        }
        else if (context.canceled)
        {
            shootButtonPressed = false;
        }
    }
    #endregion

    #region Trigger Events
    private void OnTriggerStay2D(Collider2D other)
    {
      
    }

    private void OnTriggerExit2D(Collider2D other)
    {

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