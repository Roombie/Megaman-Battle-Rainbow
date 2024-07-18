using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // MEGAMAN
    [Header("Functional Options")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canShoot = true;
    [SerializeField] private bool canSlide = true;

    [Header("Health state")]
    public int currentHealth;
    public int maxHealth = 28;
    [SerializeField] GameObject deathExplosion; // Death explosion
    private bool isTakingDamage;
    private bool isInvincible;
    private bool hitSideRight;

    [Header("Gravity")]
    [SerializeField] private float gravityScale = 1f;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool facingRight = true;
    private Vector2 moveInput;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int maxExtraJumps = 1;
    [SerializeField] private float extraJumpForce = 5f;
    [SerializeField] private float jumpBufferTime = 0.125f;
    [SerializeField] private float coyoteTime = 0.125f;
    private bool isJumping; // Check if we are currently jumping
    private bool jumpButtonPressed = false;
    private bool inAirFromJump;
    private float lastGroundedTime;
    private float lastJumpTime;
    private int extraJumpCount;

    [Header("Shooting")]
    [SerializeField] int bulletDamage = 1;
    [SerializeField] float bulletSpeed = 15f;
    [SerializeField] float shootDelay = 0.2f;
    [SerializeField] Vector2 bulletShootOffset = new(0.5f, 1f);
    [SerializeField] float shootRayLength = 5f;
    [SerializeField] GameObject bulletPrefab;
    private bool isShooting; // Check if we are currently shooting
    private float shootTime;
    private float shootTimeLength;
    private bool shootButtonPressed = false;
    private bool shootButtonRelease;
    private float shootButtonReleaseTimeLength;

    [Header("Sliding")]
    [SerializeField] private float slideSpeed = 6f;
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private Transform slideDustPos;
    [SerializeField] private GameObject slideDustPrefab;
    private bool isSliding; // Check if we are currently sliding
    private float slideTime;
    private float slideTimeLength;

    // Ladder
    [HideInInspector] public LadderHandlers ladder;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Vector2 groundCheckOffset = new(0f, -0.5f);
    [SerializeField] private float groundCheckWidth = 0.5f;

    [Header("Ceiling Check")]
    [SerializeField] private Vector2 ceilingCheckOffset = new Vector2(0f, 0.5f);
    [SerializeField] private float ceilingCheckDistance = 0.1f;

    [Header("Gear")]
    public ParticleSystem gearSmoke;
    public GameObject speedGearTrail;

    [Header("Pause Menu")]
    public bool isPaused = false;
    public enum PlayerStates { Normal, Still, Frozen, Climb, Hurt, Fallen, Paused, Riding }
    public PlayerStates state = PlayerStates.Normal;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // start at full health
        currentHealth = maxHealth;
        // start facing right always
        facingRight = true;
    }

    void Update()
    {
        if (isPaused) return;
        if (canJump) CheckJump();
        if (canShoot) PlayerShoot();

        if (isTakingDamage)
        {
            animator.Play("Megaman_Hit");
            return;
        }

        if (currentHealth <= 5)
        {
            if (!gearSmoke.isPlaying)
            {
                gearSmoke.Play();
            }
        }
        else
        {
            if (gearSmoke.isPlaying)
            {
                gearSmoke.Stop();
            }
        }

        animator.SetBool("isGrounded", IsGrounded());
        animator.SetFloat("horizontal", Mathf.Abs(moveInput.x));
        animator.SetBool("isShooting", isShooting);
        animator.SetBool("isSliding", isSliding && IsGrounded());
    }

    void FixedUpdate()
    {
        Move();
        ApplyGravity();
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

    // used when sliding
    private bool IsColAbove()
    {
        Vector2 position = (Vector2)transform.position + ceilingCheckOffset;
        bool centerHit = Physics2D.Raycast(position, Vector2.up, ceilingCheckDistance, groundLayer);
        return centerHit;
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
        GameObject deathPlayer = Instantiate(deathExplosion);
        deathPlayer.transform.position = transform.position;
        Destroy(gameObject);
        // Logic for defeat (e.g., player death, game over screen)
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
        animator.Play("Megaman_Hit", -1, 0f);
    }
    #endregion

    #region Gravity
    public void ApplyGravity()
    {
        if (!rb.isKinematic && !isPaused)
            rb.velocity += gravityScale * rb.mass * Vector2.down;
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
        // Check if the player is grounded and reset inAirFromJump flag
        if (IsGrounded())
        {
            inAirFromJump = false;
            isJumping = false;
            extraJumpCount = maxExtraJumps; // Reset extra jump count when grounded
        }

        // Check if the player is falling or has reached the peak of the jump
        if (isJumping && rb.velocity.y <= 0)
        {
            isJumping = false;
        }

        // Handle normal jump
        if (jumpButtonPressed && (Time.time - lastJumpTime <= jumpBufferTime)) // Check if the jump button is pressed and if the time since the last jump is within the jump buffer time
        {
            if (IsGrounded() || (Time.time - lastGroundedTime <= coyoteTime && !inAirFromJump)) // Check if the player is grounded or if within the coyote time and not in the air from a previous jump
            {
                Jump(jumpForce);
                Debug.Log("Jump!");
            }
            else if (!IsGrounded() && extraJumpCount > 0 && !isJumping)  // If not grounded and there are extra jumps available and the player is not currently jumping
            {
                Jump(extraJumpForce);
                extraJumpCount--;  // Decrease the count of available extra jumps
                Debug.Log("Extra Jump!");
            }
        }

        // Reduce upward velocity for variable jump height
        if (!jumpButtonPressed && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    private void Jump(float jumpForce)
    {
        isJumping = true;
        inAirFromJump = true;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        lastJumpTime = Time.time;
    }
    #endregion

    #region Shoot
    private void PlayerShoot()
    {
        shootTimeLength = 0;
        shootButtonReleaseTimeLength = 0;

        // shoot key is being pressed and key release flag true
        if (shootButtonPressed && shootButtonRelease && !isSliding)
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
        // Calculate the direction based on facingRight
        Vector2 shootDirection = facingRight ? Vector2.right : Vector2.left;
        // Calculate the starting point of the raycast using the offset
        Vector2 shootStartPosition = (Vector2)transform.position + new Vector2(facingRight ? bulletShootOffset.x : -bulletShootOffset.x, bulletShootOffset.y);
        // Always use the maximum ray length for the shoot position
        Vector2 shootPosition = shootStartPosition + shootDirection * shootRayLength;

        GameObject bullet = Instantiate(bulletPrefab, shootPosition, Quaternion.identity);
        bullet.name = bulletPrefab.name;
        bullet.GetComponent<Bullet>().SetDamageValue(bulletDamage);
        bullet.GetComponent<Bullet>().SetBulletSpeed(bulletSpeed);
        bullet.GetComponent<Bullet>().SetBulletDirection(facingRight ? Vector2.right : Vector2.left);
        bullet.GetComponent<Bullet>().Shoot();
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

    #region Gizmos
    private void OnDrawGizmos()
    {
        // Determine ground check width and offset for each ground raycast
        Vector2 position = (Vector2)transform.position + groundCheckOffset;
        float halfWidth = groundCheckWidth / 2;
        Vector2 leftRayStart = position - new Vector2(halfWidth, 0);
        Vector2 rightRayStart = position + new Vector2(halfWidth, 0);

        // Draw the ground raycast
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawLine(position, position + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(leftRayStart, leftRayStart + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(rightRayStart, rightRayStart + Vector2.down * groundCheckDistance);

        // Draw the shoot raycast
        Vector2 shootDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 shootStartPosition = (Vector2)transform.position + new Vector2(facingRight ? bulletShootOffset.x : -bulletShootOffset.x, bulletShootOffset.y);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(shootStartPosition, shootStartPosition + shootDirection * shootRayLength);

        // Ceiling check visualization when sliding
        Vector2 onTopCollision = (Vector2)transform.position + ceilingCheckOffset;
        Gizmos.color = IsColAbove() ? Color.green : Color.red;
        Gizmos.DrawLine(onTopCollision, onTopCollision + Vector2.up * ceilingCheckDistance);
    }
    #endregion
}