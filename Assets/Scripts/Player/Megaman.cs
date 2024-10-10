using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class Megaman : MonoBehaviour
{
    // MEGAMAN
    [Header("Functional Options")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canShoot = true;
    [SerializeField] private bool canSlide = true;
    [SerializeField] private bool canClimb = true;
    [HideInInspector]
    public bool freezeInput = false;
    private bool freezePlayer = false;
    private bool freezeEverything = false;

    [Header("Health state")]
    public int currentHealth;
    public int maxHealth = 28;
    [SerializeField] GameObject deathExplosion; // Death explosion
    [SerializeField] private float delayBeforeDeath = 0.5f;
    private bool isTakingDamage;
    private bool isInvincible;
    private bool hitSideRight;

    [Header("Gravity")]
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float waterGravityScale = 0.5f;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private bool facingRight = true;
    public bool IsFacingRight => facingRight; // This is to be referenced on WeaponBase script
    [SerializeField] private bool useStepDelay = true;
    [SerializeField] private float stepDelay = 0.1f;  // Time for the 1-pixel step
    [SerializeField] private float stepDistance = 2f; // Amount of the 1-pixel step
    private Vector2 moveInput;
    private bool isMoving = false;
    private bool hasStepped = false;
    private float stepTimer = 0f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 18f;
    [SerializeField] private int maxExtraJumps = 1;
    [SerializeField] private float extraJumpForce = 12f;
    [SerializeField] private float jumpBufferTime = 0.125f;
    [SerializeField] private float coyoteTime = 0.125f;
    private bool isJumping; // Check if we are currently jumping
    private bool isFalling = false;
    private bool jumpButtonPressed = false;
    private bool inAirFromJump;
    private float lastGroundedTime;
    private float lastJumpTime;
    private int extraJumpCount;

    [Header("Shooting")]
    [SerializeField] private Vector2 bulletShootOffset = new(0.5f, 1f);
    [SerializeField] private float shootRayLength = 5f;
    [SerializeField] private bool chargerEnabled = true;
    [SerializeField] private bool interruptChargeOnDamage = true;
    private int currentShootLevel = 0;  // Current shoot level (index in the shootLevel list)
    private float chargeTime = 0f;      // Takes the current charge time
    private bool hasPlayedChargeSound = false;
    private readonly List<GameObject> activeBullets = new();  // Track active bullets
    private bool isShooting; // Check if we are currently shooting
    private float shootTime;
    private float shootTimeLength;
    private bool shootButtonPressed = false;
    private bool shootButtonRelease;
    private float shootButtonReleaseTimeLength;

    [System.Serializable]
    public struct WeaponsStruct
    {
        public WeaponTypes weaponType;
        public WeaponData weaponData;  // Use WeaponData SO for FLEXIBLE weapon properties
    }
    public WeaponsStruct[] weaponsData;
    public WeaponTypes playerWeapon = WeaponTypes.MegaBuster;
    private WeaponBase currentWeapon;

    public WeaponsStruct[] GetWeapons()
    {
        return weaponsData;
    }

    public WeaponTypes GetCurrentWeaponType()
    {
        return playerWeapon;
    }

    public GameObject weaponSwitchIcon;
    [SerializeField] private float iconDisplayTime = 1.5f; // Time to display the weapon switch icon
    private float iconTimer = 0f; // Timer for tracking icon display duration

    private enum SwapIndex
    {
        Primary = 64,   // Red 64 for the helmet, gloves, boots, etc (SwapIndex.Primary) | 64: Grey value
        Secondary = 128 // Red 128 for his shirt, pants, etc (SwapIndex.Secondary) | 128:
    }

    [Header("Sliding")]
    [SerializeField] private float slideSpeed = 6f;
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private Transform slideDustPos;
    [SerializeField] private GameObject slideDustPrefab;
    [SerializeField] private ParticleSystem slideParticles;
    [SerializeField] private Vector2 slideBoxOffset;
    [SerializeField] private Vector2 slideBoxSize;
    private bool isSliding; // Check if we are currently sliding
    private float slideTime;
    private float slideTimeLength;
    private bool slideButtonPressed = false;
    private bool slideButtonRelease = true;
    private Vector2 defaultBoxOffset;
    private Vector2 defaultBoxSize;

    [Header("Climbing")]
    [SerializeField] private float climbSpeed = 3.5f;
    private bool isCloseToLadder = false; // Indicates if the player is close to a ladder
    private bool isClimbing; // Check if we are currently climbing
    private bool isOnPlatformLadder;
    private bool atLadderTop;
    private bool atLaddersEnd; // Indicates if the player has reached the end of the ladder.
    [HideInInspector] public LadderHandlers ladder; // Ladder

    [Header("Under Water")]
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform bubblePos;
    [SerializeField] private BubbleType bubbleType = BubbleType.NextBubble;
    [SerializeField] private float spawnInterval = 1.25f;
    private float timer;                        // Timer based
    private GameObject currentBubble = null;    // Next bubble

    [System.Serializable]
    public enum BubbleType
    {
        NextBubble,    // For bubbles that spawn after the other disappears
        TimerBased    // For bubbles that spawn based on a timer
    }

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Vector2 groundCheckOffset = new(0f, -0.5f);
    [SerializeField] private float groundCheckWidth = 0.5f;

    [Header("Top Collision Check")]
    [SerializeField] private Vector2 topCheckOffset = new(0f, 0.5f);
    [SerializeField] private Vector2 topCheckSize = new(1f, 0.2f);

    [Header("Front Collision Check")]
    [SerializeField] private Vector2 frontCheckOffset = new(0.5f, 0f);
    [SerializeField] private Vector2 frontCheckSize = new(0.2f, 1f);

    public enum PlayerStates { Normal, Still, Frozen, Climb, Hurt, Fallen, Paused, Riding }
    public PlayerStates state = PlayerStates.Normal;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip chargingMegaBuster;
    [SerializeField] private AudioClip land;
    [SerializeField] private AudioClip damage;

    private Rigidbody2D rb;
    private RigidbodyConstraints2D rb2dConstraints;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private ColorSwap colorSwap;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // start at full health
        currentHealth = maxHealth;
        // start facing right always
        facingRight = true;
        // store box collider's size and offset
        defaultBoxOffset = new(boxCollider.offset.x, boxCollider.offset.y);
        defaultBoxSize = new(boxCollider.size.x, boxCollider.size.y);

        colorSwap = GetComponent<ColorSwap>();
        SetWeapon(playerWeapon);
        weaponSwitchIcon.SetActive(false);
    }

    void Update()
    {
        if (!freezeInput)
        {
            if (canMove) Move();
            if (canJump) CheckJump();
            if (canShoot) UseWeapon();
            if (canSlide) PerformSlide();
            if (canClimb) HandleClimbing();
        }

        // Check if the icon is active and update the timer
        if (weaponSwitchIcon.activeSelf && weaponSwitchIcon != null)
        {
            // Set the local scale based on the player's facing direction
            Vector3 iconScale = weaponSwitchIcon.transform.localScale;
            iconScale.x = facingRight ? 1 : -1; // Flip the icon based on the player's facing direction (this is so it looks like its facing the same direction no matter if the player flips
            weaponSwitchIcon.transform.localScale = iconScale;

            iconTimer += Time.deltaTime; // Increment the timer
            if (iconTimer >= iconDisplayTime)
            {
                weaponSwitchIcon.SetActive(false); // Deactivate the icon after the time is up
            }
        }

        UpdateAnimations();
        HandleBubbleState();
        HandleSlideParticles();
    }

    void FixedUpdate()
    {
        ApplyGravity();
    }

    #region Particle Handling
    void HandleSlideParticles()
    {
        if (slideParticles != null) // if you add slide particles
        {
            if (currentHealth <= 5 && currentHealth > 0) // when player's current health is equals or less than 5 but greater than 0
            {
                if (!slideParticles.isPlaying)
                {
                    slideParticles.Play(); // play the particles
                }
            }
            else // if it's not
            {
                if (slideParticles.isPlaying)
                {
                    slideParticles.Stop(); // stop the particles
                }
            }
        }
    }
    #endregion

    #region Animation Updates
    private void UpdateAnimations()
    {
        animator.SetBool("isTakingDamage", isTakingDamage);
        if (isTakingDamage)
        {
            animator.SetTrigger("hit");
            return;
        }

        bool isClimbingToTop = ladder != null && isClimbing && Mathf.Abs(transform.position.y - ladder.posTopHandlerY) < 0.5f;

        animator.SetBool("isStepping", useStepDelay && isMoving && !hasStepped);
        animator.SetBool("useStep", useStepDelay);
        animator.SetBool("isGrounded", IsGrounded());
        animator.SetFloat("horizontal", Mathf.Abs(moveInput.x));
        animator.SetBool("isShooting", isShooting);
        animator.SetBool("isSliding", isSliding);
        animator.SetBool("isClimbing", isClimbing);
        animator.SetBool("isClimbingToTop", isClimbingToTop);
    }
    #endregion

    #region Collision Detection (ground, above collision, front collision)

    // ground
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
        Vector2 position = (Vector2)transform.position + topCheckOffset;
        bool colAbove = Physics2D.OverlapBox(position, topCheckSize, 0f, groundLayer) != null;
        return colAbove;
    }

    private bool IsFrontCollision()
    {
        // Calculate the position and size for the front check
        Vector2 position = (Vector2)transform.position + (facingRight ? frontCheckOffset : new Vector2(-frontCheckOffset.x, frontCheckOffset.y));
        bool frontHit = Physics2D.OverlapBox(position, frontCheckSize, 0f, groundLayer) != null;
        return frontHit;
    }
    #endregion

    #region Stop Input & Freeze Player
    public void FreezeInput(bool freeze)
    {
        freezeInput = freeze;
    }

    public void FreezePlayer(bool shouldFreeze)
    {
        freezePlayer = shouldFreeze;

        if (shouldFreeze)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // freeze player so gravity isn't applied
            rb2dConstraints = rb.constraints; // save current constraints
            animator.speed = 0; // pause player's animation
            rb.constraints = RigidbodyConstraints2D.FreezeAll; // freeze all rigidbody movement
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic; // gravity is now applied to the player
            animator.speed = 1; // resume player's animation
            rb.constraints = rb2dConstraints; // restore current constraints
        }
    }
    #endregion

    #region Health Management
    public void RestoreFullHealth(AudioClip itemSound)
    {
        if (currentHealth < maxHealth)
        {
            StartCoroutine(IncrementHealth(maxHealth - currentHealth, itemSound));
        }
    }

    public void RestoreHealth(int amount, AudioClip itemSound, bool freezeEverything = true)
    {
        if (currentHealth != maxHealth)
            StartCoroutine(IncrementHealth(amount, itemSound, freezeEverything));
    }

    private IEnumerator IncrementHealth(int amount, AudioClip itemSound, bool freezeEverything = true)
    {
        int healthToRestore = Mathf.Clamp(amount, 0, maxHealth - currentHealth);
        if (freezeEverything) GameManager.Instance.FreezeEverything(true);
        while (healthToRestore > 0)
        {
            AudioManager.Instance.Play(itemSound, SoundCategory.SFX, 1, 1, true);
            currentHealth++;  // This increments player's health
            UIHealthBar.Instance.SetValue(currentHealth / (float)maxHealth);  // And then, update health bar UI
            Debug.Log("Current health: " + currentHealth);
            healthToRestore--;

            yield return new WaitForSeconds(0.05f);
        }
        AudioManager.Instance.Stop(itemSound);
        GameManager.Instance.FreezeEverything(false);
    }
    #endregion

    #region Weapon Energy Management
    public void RestoreFullWeaponEnergy(AudioClip itemSound)
    {
        if (currentWeapon.weaponData.currentEnergy < currentWeapon.weaponData.maxEnergy)
        {
            StartCoroutine(IncrementWeaponEnergy(currentWeapon.weaponData.maxEnergy - currentWeapon.weaponData.currentEnergy, itemSound));
        }
    }

    public void RestoreWeaponEnergy(int amount, AudioClip itemSound, bool freezeEverything = true)
    {
        if (currentWeapon.weaponData.currentEnergy < currentWeapon.weaponData.maxEnergy)
        {
            StartCoroutine(IncrementWeaponEnergy(amount, itemSound, freezeEverything));
        }
    }

    private IEnumerator IncrementWeaponEnergy(int amount, AudioClip itemSound, bool freezeEverything = true)
    {
        int energyToRestore = Mathf.Clamp(amount, 0, currentWeapon.weaponData.maxEnergy - currentWeapon.weaponData.currentEnergy);

        // Optionally freeze everything while restoring energy
        if (freezeEverything) GameManager.Instance.FreezeEverything(true);

        while (energyToRestore > 0)
        {
            // Play item sound during the restoration process
            AudioManager.Instance.Play(itemSound, SoundCategory.SFX, 1, 1, true);

            // Increment the current weapon's energy
            currentWeapon.weaponData.currentEnergy++;

            // Update the weapon energy bar UI
            UIEnergyBar.Instance.SetValue(currentWeapon.weaponData.currentEnergy / (float)currentWeapon.weaponData.maxEnergy);

            Debug.Log($"Current energy: {currentWeapon.weaponData.currentEnergy}");

            energyToRestore--;

            // Wait for a small interval before restoring the next point of energy
            yield return new WaitForSeconds(0.05f);
        }

        // Stop the sound once the restoration process is complete
        AudioManager.Instance.Stop(itemSound);

        // Unfreeze everything after the restoration process
        GameManager.Instance.FreezeEverything(false);
    }
    #endregion

    #region Damage state
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
            if (damage > 0)
            {
                // take damage amount from health and update the health bar
                currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
                UIHealthBar.Instance.SetValue(currentHealth / (float)maxHealth);

                if (playerWeapon == WeaponTypes.MegaBuster)
                {
                    // Update energy bar with health percentage
                    float healthPercentage = (float)currentHealth / maxHealth;
                    UIEnergyBar.Instance.SetValue(healthPercentage);
                }

                // no more health means defeat, otherwise take damage
                if (currentHealth <= 0)
                {
                    Defeat();
                }
                else
                {
                    StartDamageAnimation();
                    Debug.Log("Taking Damage");
                }
            }         
        }
    }

    private void StartDamageAnimation()
    {
        // once isTakingDamage is true in the Update function we'll play the Hit animation
        // here we go invincible so we don't repeatedly take damage, determine the X push force
        // depending which side we were hit on, and then apply that force
        if (!isTakingDamage)
        {
            isSliding = false;
            if (interruptChargeOnDamage) InterruptChargeShoot();
            AudioManager.Instance.Play(damage);
            isTakingDamage = true;
            Invincible(true);
            FreezeInput(true);
            EndClimbing();
            float hitForceX = 0.50f;
            float hitForceY = 1.5f;
            if (hitSideRight) hitForceX = -hitForceX;
            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(hitForceX, hitForceY), ForceMode2D.Impulse);
        }
    }

    void StopDamageAnimation()
    {
        // It's referenced as an Animation Event, this function is called at the end of the Hit animation
        isTakingDamage = false;
        FreezeInput(false);
        StartCoroutine(FlashAfterDamage());
    }

    private IEnumerator FlashAfterDamage()
    {
        // hit animation is 12 samples, keep flashing consistent with 1/12 secs
        float flashDelay = 0.0833f;
        for (int i = 0; i < 10; i++)
        {
            // Toggle transparency
            // Calling the sprite renderer's material transparency and change it to 0 and 1 for 
            spriteRenderer.material.SetFloat("_Transparency", 0f);
            yield return new WaitForSeconds(flashDelay);
            spriteRenderer.material.SetFloat("_Transparency", 1f);
            yield return new WaitForSeconds(flashDelay);
        }
        Invincible(false);
    }
    #endregion

    #region Death
    private void Defeat()
    {
        StartCoroutine(StartDeathAnimation());
    }

    private IEnumerator StartDeathAnimation()
    {
        FreezeInput(true);
        FreezePlayer(true);
        yield return new WaitForSeconds(delayBeforeDeath);
        GameObject deathPlayer = Instantiate(deathExplosion);
        deathPlayer.transform.position = transform.position;
        Destroy(gameObject);
        GameManager.Instance.LoseLife();
    }
    #endregion

    #region Gravity
    public void ApplyGravity()
    {
        if (!rb.isKinematic)
        {
            float gravity = IsInWater() ? gravityScale * waterGravityScale : gravityScale;
            rb.velocity += gravity * rb.mass * Vector2.down;
        }
    }
    #endregion

    #region Weapons
    private void InitializeWeapon(WeaponTypes weaponType)
    {
        // Find the selected weapon data
        WeaponsStruct selectedWeapon = weaponsData.First(w => w.weaponType == weaponType);

        // Check if the selected weapon prefab is not null
        if (selectedWeapon.weaponData.weaponPrefab != null)
        {
            // Assign the weapon prefab to the current weapon
            currentWeapon = selectedWeapon.weaponData.weaponPrefab.GetComponent<WeaponBase>();

            // Ensure the weapon data is also assigned
            currentWeapon.weaponData = selectedWeapon.weaponData;
        }
        else
        {
            Debug.LogError("Weapon prefab is not assigned for " + weaponType);
        }
    }

    void SetWeapon(WeaponTypes weaponType)
    {
        /* ColorSwap and Shader to change MegaMan's color scheme (Explained by Gamedev with Tony)
         * 
         * his spritesheets have been altered to greyscale for his outfit
         * Red 64 for the helmet, gloves, boots, etc ( SwapIndex.Primary )
         * Red 128 for his shirt, pants, etc ( SwapIndex.Secondary )
         * 
         * couple ways to code this but I settled on #2
         * 
         * #1 using Lists
         * 
         * var colorIndex = new List<int>();
         * var playerColors = new List<Color>();
         * colorIndex.Add((int)SwapIndex.Primary);
         * colorIndex.Add((int)SwapIndex.Secondary);
         * playerColors.Add(ColorSwap.ColorFromIntRGB(64, 64, 64));
         * playerColors.Add(ColorSwap.ColorFromIntRGB(128, 128, 128));
         * colorSwap.SwapColors(colorIndex, playerColors);
         * 
         * #2 using SwapColor as needed then ApplyColor
         * 
         * colorSwap.SwapColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(0x0073F7));
         * colorSwap.SwapColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(0x00FFFF));
         * colorSwap.ApplyColor();
         * 
         * Also, we'll change the color of our weapon energy bar
         * and adjust the energy value as given in the playerWeaponsStruct
         * 
         */
        // Check if the weaponsData is populated
        if (weaponsData == null || weaponsData.Length == 0)
        {
            Debug.LogError("Weapons data is not initialized or empty.");
            return;
        }

        // Find the selected weapon from the weaponsData array
        WeaponsStruct selectedWeapon = weaponsData.FirstOrDefault(w => w.weaponType == weaponType);

        // Check if the selected weapon is valid
        if (selectedWeapon.weaponData == null)
        {
            Debug.LogError($"Selected weapon {weaponType} is not valid or doesn't have associated data.");
            return;
        }

        // Initialize the weapon using the selected weapon type
        InitializeWeapon(weaponType);

        // Update the player's weapon type
        playerWeapon = weaponType;

        // Update the weapon energy bar UI and weapon sprite
        UIEnergyBar.Instance.SetValue(currentWeapon.weaponData.currentEnergy / (float)currentWeapon.weaponData.maxEnergy);
        UIEnergyBar.Instance.SetEnergyBar(currentWeapon.weaponData.weaponBarSprite);

        if (weaponType == WeaponTypes.MegaBuster)
        {
            currentWeapon.weaponData.currentEnergy = currentHealth;
            UIEnergyBar.Instance.SetValue((float)currentHealth / maxHealth);
            UIEnergyBar.Instance.SetVisibility(false);
        }
        else
        {
            UIEnergyBar.Instance.SetVisibility(true);
        }

        // Reset charge level and time
        currentShootLevel = 0;
        chargeTime = 0f;

        // Convert Color to integer representation for ColorFromInt usage
        int primaryColorInt = ColorSwap.ColorToHex(selectedWeapon.weaponData.primaryColor);
        int secondaryColorInt = ColorSwap.ColorToHex(selectedWeapon.weaponData.secondaryColor);

        // Swap colors using the integer representation
        // Change player's colors based on the current weapon
        colorSwap.SwapColor((int)SwapIndex.Primary, ColorSwap.ColorFromInt(primaryColorInt));
        colorSwap.SwapColor((int)SwapIndex.Secondary, ColorSwap.ColorFromInt(secondaryColorInt));
        colorSwap.ApplyColor();

        Debug.Log($"Primary color: {primaryColorInt}, Secondary color: {secondaryColorInt}");

        // Debug log to confirm the weapon change
        // Debug.Log($"Weapon switched to: {playerWeapon}");
    }

    public void SwitchWeapon(WeaponTypes weaponType)
    {
        // we can call this function to switch the player to the chosen weapon
        // this is used when player is on the weapons menu
        EndClimbing();
        SetWeapon(weaponType);
    }

    private void SwitchToNextWeapon()
    {
        int nextWeaponIndex = (int)playerWeapon + 1;
        if (nextWeaponIndex >= weaponsData.Length) nextWeaponIndex = 0;

        SetWeapon((WeaponTypes)nextWeaponIndex);
        ShowWeaponSwitchIcon();
        // Debug.Log($"Switched to Next Weapon: {nextWeaponIndex}"); // Debug log to verify

        currentShootLevel = 0;
        chargeTime = 0f;
    }

    private void SwitchToPreviousWeapon()
    {
        int previousWeaponIndex = (int)playerWeapon - 1;
        if (previousWeaponIndex < 0) previousWeaponIndex = weaponsData.Length - 1;

        SetWeapon((WeaponTypes)previousWeaponIndex);
        ShowWeaponSwitchIcon();
        // Debug.Log($"Switched to Previous Weapon: {previousWeaponIndex}"); // Debug log to verify

        currentShootLevel = 0;
        chargeTime = 0f;
    }

    private void ShowWeaponSwitchIcon()
    {
        // Set the weapon icon to the current weapon's icon
        weaponSwitchIcon.GetComponent<SpriteRenderer>().sprite = currentWeapon.weaponData.weaponIcon;
        weaponSwitchIcon.SetActive(true); // Activate the icon

        // Reset the timer
        iconTimer = 0f; // Reset the timer whenever the icon is shown
    }

    private void UseWeapon()
    {
        switch (playerWeapon)
        {
            case WeaponTypes.MegaBuster:
                MegaBuster();
                break;

            case WeaponTypes.MagnetBeam:
                MagnetBeam();
                break;
        }
    }
    #endregion

    #region Movement
    private void Move()
    {
        if (isSliding || isClimbing) return; // Player will use the slide or climbing if is true

        // Check if there is horizontal input
        if (moveInput.x != 0)
        {
            if (!isMoving) // If you're not moving
            {
                // Start moving with a step delay
                isMoving = true;
                hasStepped = false;
                stepTimer = stepDelay; // Start the timer for the step delay
            }
            else
            {
                // Check if direction has changed while falling
                bool directionChangedWhileFalling = (Mathf.Sign(moveInput.x) != Mathf.Sign(rb.velocity.x)) && !IsGrounded();

                if (useStepDelay && !hasStepped && IsGrounded() && !directionChangedWhileFalling)
                {
                    // Apply step
                    rb.velocity = new Vector2(moveInput.x * stepDistance, rb.velocity.y);
                    stepTimer -= Time.deltaTime; // Decrease the step timer

                    if (stepTimer <= 0)
                    {
                        // Start with step distance
                        hasStepped = true; // the step has been completed
                        stepTimer = stepDelay; // Reset timer for next step
                    }
                }
                else
                {
                    // Apply normal speed if direction has changed while falling or step is not applicable
                    rb.velocity = new Vector2(moveInput.x * speed, rb.velocity.y);
                    hasStepped = true; // Ensure stepping is not applied in these cases
                }
            }
        }
        else
        {
            // Stop movement when no input
            isMoving = false;
            hasStepped = false;
            rb.velocity = new Vector2(0, rb.velocity.y); // Stop movement when no input
        }

        if (moveInput.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && facingRight)
        {
            Flip();
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

    #region Jumping
    private void CheckJump()
    {
        if (isClimbing) return;  // Exit early if climbing

        bool isGrounded = IsGrounded();  // Cache the result of IsGrounded() for performance

        // Reset jump states when grounded
        if (isGrounded)
        {
            inAirFromJump = false;
            isJumping = false;
            extraJumpCount = maxExtraJumps;  // Reset extra jumps
        }
        else if (rb.velocity.y < 0)
        {
            isFalling = true;
        }

        // Play landing sound when grounded after falling
        if (isGrounded && isFalling)
        {
            AudioManager.Instance.Play(land);
            isFalling = false;
        }

        // Cancel jumping state when player is falling or reaches jump peak
        if (isJumping && rb.velocity.y <= 0)
        {
            isJumping = false;
        }

        // Prevent jumping if sliding and an object is above
        if (isSliding && IsColAbove())
        {
            jumpButtonPressed = false;
            return; // Exit early since jumping isn't allowed
        }

        // Slide logic
        if (CanSlideWithDownJump() && isGrounded && !isSliding)
        {
            PerformSlide();
            jumpButtonPressed = false;  // Ensure no jump after sliding
            return;
        }

        // Handle jumping
        if (jumpButtonPressed && Time.time - lastJumpTime <= jumpBufferTime)
        {
            // Normal jump or coyote time jump
            if (isGrounded || (Time.time - lastGroundedTime <= coyoteTime && !inAirFromJump))
            {
                Jump(jumpForce);
            }
            // Extra jump logic
            else if (!isGrounded && extraJumpCount > 0 && !isJumping)
            {
                Jump(extraJumpForce);
                extraJumpCount--;
            }
        }

        // Variable jump height (reduce upward velocity if jump button is released mid-air)
        if (!jumpButtonPressed && rb.velocity.y > 0 && inAirFromJump)
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

    #region Shooting
    private void InterruptChargeShoot()
    {
        if (chargeTime > 0)
        {
            chargeTime = 0f;
            currentShootLevel = 0;
            isShooting = false;
            hasPlayedChargeSound = false;
            AudioManager.Instance.Stop(chargingMegaBuster);
            Debug.Log("Charge interrupted due to damage.");
        }
    }

    #region MegaBuster
    private void MegaBuster()
    {
        shootTimeLength = 0;
        shootButtonReleaseTimeLength = 0;

        // Charge shoot level based on button press duration
        if (shootButtonPressed && !shootButtonRelease && chargerEnabled)
        {
            chargeTime += Time.deltaTime;

            // Determine the current shoot level
            currentShootLevel = 0;
            for (int i = 0; i < currentWeapon.weaponData.chargeLevels.Count; i++)
            {
                if (chargeTime >= currentWeapon.weaponData.chargeLevels[i].timeRequired)
                {
                    currentShootLevel = i;
                }
            }

            // Play charge sound if needed
            if (currentShootLevel > 0 && !hasPlayedChargeSound)
            {
                AudioManager.Instance.Play(chargingMegaBuster);
                hasPlayedChargeSound = true;
            }
        }

        // Handle shooting when button is pressed and released
        if (shootButtonPressed && shootButtonRelease && !isSliding)
        {
            if ((!currentWeapon.weaponData.limitBulletsOnScreen || activeBullets.Count < currentWeapon.weaponData.maxBulletsOnScreen)
                && currentWeapon.weaponData.currentEnergy >= currentWeapon.weaponData.energyCost)
            {
                isShooting = true;
                shootButtonRelease = false;
                shootTime = Time.time;

                // Deduct energy for the shot
                currentWeapon.weaponData.currentEnergy -= currentWeapon.weaponData.energyCost;

                // Delay shot based on weapon data
                Invoke(nameof(ShootMegaBuster), currentWeapon.weaponData.shootDelay);
            }
        }

        // Handle releasing the button for charged shots
        if (!shootButtonPressed && shootButtonRelease)
        {
            // Debug.Log("Button released, checking for charged shot");
            shootButtonReleaseTimeLength = Time.time - shootTime;

            if (currentShootLevel > 0)
            {
                if (isSliding)
                {
                    Debug.Log("Player is sliding, delaying shot");
                    return; // Delay shooting if sliding
                }
                else
                {
                    isShooting = true;
                    shootTime = Time.time;
                    AudioManager.Instance.Stop(chargingMegaBuster);
                    ShootMegaBuster(); // Call the new method to handle shooting
                }
            }

            // Reset charge time and button states
            chargeTime = 0f;
            currentShootLevel = 0;
            shootButtonRelease = false;
            hasPlayedChargeSound = false;
        }

        // shoot key isn't being pressed and key release flag is false
        if (!shootButtonPressed && !shootButtonRelease)
        {
            shootButtonReleaseTimeLength = Time.time - shootTime;
            shootButtonRelease = true;
        }

        // Limit shooting duration
        if (isShooting)
        {
            shootTimeLength = Time.time - shootTime;
            if (shootTimeLength >= 0.25f || shootButtonReleaseTimeLength > 0.15f)
            {
                isShooting = false;
            }
        }
    }

    private void ShootMegaBuster()
    {
        // Call the Shoot method of the current weapon, which handles the position calculation
        currentWeapon.Shoot(transform, bulletShootOffset, facingRight, currentShootLevel, shootRayLength);

        // Reset the current shoot level and charge time after shooting
        currentShootLevel = 0;
        chargeTime = 0f;
    }
    #endregion

    #region MagnetBeam
    private void MagnetBeam()
    {
        MagnetBeamWeapon magnetBeamWeapon = currentWeapon as MagnetBeamWeapon;

        // Stop the beam if button is released
        if (!shootButtonPressed && shootButtonRelease)
        {
            if (magnetBeamWeapon != null)
            {
                Debug.Log("Stopping the beam");
                magnetBeamWeapon.StopBeam();
                isShooting = false; // Reset shooting state
            }
        }

        // Handle beam creation when the shoot button is pressed and released
        if (shootButtonPressed && shootButtonRelease && !isShooting && !isSliding && !isInvincible && magnetBeamWeapon != null)
        {
            Debug.Log("Creating beam");
            // Call the Shoot method to instantiate the beam
            magnetBeamWeapon.Shoot(transform, bulletShootOffset, facingRight, currentShootLevel, shootRayLength);
            if (magnetBeamWeapon.CanShoot())
            {
                isShooting = true; // Set shooting to true
            }
            shootButtonRelease = false; // Reset shoot button release
        }

        // Update the beam position while the player is shooting
        if (magnetBeamWeapon != null && isShooting && !isInvincible)
        {
            Vector2 newBeamPosition = (Vector2)transform.position + new Vector2(facingRight ? bulletShootOffset.x : -bulletShootOffset.x, bulletShootOffset.y);
            magnetBeamWeapon.UpdateMagnetBeamPosition(newBeamPosition, facingRight);
        }

        // Check if the beam has reached its max length
        if (magnetBeamWeapon != null && magnetBeamWeapon.HasReachedMaxLength() || isInvincible)
        {
            Debug.Log("Beam reached its limit");
            magnetBeamWeapon.StopBeam(); // Stop the beam when it reaches max length
            isShooting = false; // Stop shooting
        }

        // Reset the shoot button release flag if needed
        if (!shootButtonPressed && !shootButtonRelease)
        {
            shootButtonRelease = true; // Reset the shoot button release flag
        }
    }
    #endregion

    #endregion

    #region Sliding
    private bool CanSlideWithDownJump()
    {
        if (GlobalVariables.canSlideWithDownJump)
        {
            // Ensure the player is pressing the down and jump buttons
            return moveInput.y < 0 && jumpButtonPressed;
        }
        return false;
    }

    private void StartSliding()
    {
        // Start sliding if the button is pressed (or down + jump) when grounded and not already sliding
        if (((slideButtonPressed && slideButtonRelease) || CanSlideWithDownJump()) && IsGrounded() && !isSliding && !IsFrontCollision())
        {
            //Debug.Log("Start Slide!");
            isSliding = true;
            slideTime = Time.time;
            slideTimeLength = 0;

            GameObject slideDust = Instantiate(slideDustPrefab);
            slideDust.name = slideDustPrefab.name;
            slideDust.transform.position = slideDustPos.transform.position;

            if (!facingRight)
            {
                slideDust.transform.Rotate(0f, 180f, 0f);
            }

            slideButtonPressed = false;
            slideButtonRelease = false;
        }
    }

    private void PerformSlide()
    {
        if (state == PlayerStates.Climb) return;

        // Adjust collider based on sliding state
        boxCollider.offset = isSliding ? slideBoxOffset : defaultBoxOffset;
        boxCollider.size = isSliding ? slideBoxSize : defaultBoxSize;

        if (!isSliding)
        {
            StartSliding();
            return;
        }

        // Track slide duration
        bool isTouchingTop = IsColAbove();
        bool isTouchingFront = IsFrontCollision();
        slideTimeLength = Time.time - slideTime;

        // Check if we need to exit slide
        bool exitSlide = ShouldExitSlide(isTouchingTop, isTouchingFront);

        // Stop sliding if conditions are met
        if (exitSlide || slideTimeLength >= slideDuration && !isTouchingTop || !IsGrounded())
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            isSliding = false;
            slideButtonRelease = true;
        }
        else
        {
            // Apply sliding force
            rb.velocity = new Vector2(slideSpeed * (facingRight ? 1f : -1f), rb.velocity.y);
        }
    }

    private bool ShouldExitSlide(bool isTouchingTop, bool isTouchingFront)
    {
        // Check if player attempts to change direction while sliding
        if ((moveInput.x < 0 && facingRight) || (moveInput.x > 0 && !facingRight))
        {
            if (isTouchingTop)
            {
                Flip(); // Flip if there's no colliding object above
            }
            else
            {
                return true; // Exit slide if there's something blocking the direction change
            }
        }

        // Exit slide if jump is pressed with no obstacle above
        if (jumpButtonPressed && !isTouchingTop)
        {
            return true;
        }

        // Exit slide if there's a front collision after some time
        if (isTouchingFront && !isTouchingTop && slideTimeLength >= 0.1f)
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Climbing
    private void HandleClimbing()
    {
        bool nearLadder = ladder != null && ladder.isNearLadder;
        isCloseToLadder = nearLadder;

        if (!isCloseToLadder)
        {
            if (isClimbing) // Not close to ladder, reset climbing
            {
                EndClimbing();
            }
            return; // Exit if not close to the ladder
        }

        isOnPlatformLadder = IsPlayerOnPlatformLadder();
        atLadderTop = IsAtLadderTop();
        atLaddersEnd = IsAtLadderBottom();
       
        // Determine if we should start climbing
        bool shouldStartClimbing =
            // Start climbing if the player is grounded, on the platform ladder, moving down, and not shooting
            (IsGrounded() && isOnPlatformLadder && moveInput.y < 0 && !isShooting) ||
            // Start climbing if the player is grounded, not at the bottom of the ladder, not on the platform ladder, moving up, and not shooting
            (IsGrounded() && !atLaddersEnd && !isOnPlatformLadder && moveInput.y > 0 && !isShooting) ||
            // Start climbing when not grounded and pressing up or down without shooting|
            (!IsGrounded() && moveInput.y != 0 && !isShooting);

        // Determine if we should stop climbing
        bool shouldStopClimbing =
            // Stop climbing if not moving or shooting
            (moveInput.y == 0 || isShooting);

        if (isCloseToLadder)
        {
            if (shouldStartClimbing)
            {
                StartClimbing();
            }
            else if (shouldStopClimbing && isClimbing)
            {
                PauseClimbing();
            }
        }       

        if (isClimbing) // when you're already climbing 
        {
            // Flip sprite based on horizontal input when you're shooting
            if (isShooting)
            {
                if (moveInput.x > 0 && !facingRight)
                {
                    Flip();
                }
                else if (moveInput.x < 0 && facingRight)
                {
                    Flip();
                }
            }

            // Prevent climbing down if you're ground before reaching the bottom of the ladder and if you're at the platform ladder and ladder top
            if (IsGrounded() && !atLaddersEnd && !atLadderTop && !isOnPlatformLadder && moveInput.y < 0)
            {
                EndClimbing();
            }

            // Jump off the ladder when you press the jump button and not moving vertically
            if (moveInput.y == 0 && jumpButtonPressed)
            {
                EndClimbing();
            }
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
        rb.bodyType = RigidbodyType2D.Kinematic;
        animator.speed = 1;
        transform.position = new Vector3(ladder.transform.position.x, transform.position.y, transform.position.z); // Align the player with the ladder's X position
        rb.velocity = new Vector2(0, moveInput.y * climbSpeed);
    }

    private void PauseClimbing()
    {
        // Stop vertical movement and animation
        rb.velocity = Vector2.zero;
        animator.speed = 0;
    }

    private void EndClimbing()
    {
        if (isClimbing)
        {
            jumpButtonPressed = false; // Avoid immediate jump after climbing
            isClimbing = false;
            animator.speed = 1;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = Vector2.zero;
        }
    }

    private bool IsAtLadderTop()
    {
        if (ladder == null || !isClimbing) return false;
        return transform.position.y + 0.24f > ladder.posTopHandlerY;
    }

    private bool IsAtLadderBottom()
    {
        if (ladder == null || !isClimbing) return false;
        return transform.position.y + 1.5f < ladder.posBottomHandlerY;
    }

    private bool IsPlayerOnPlatformLadder()
    {
        if (ladder == null) return false;
        return transform.position.y > ladder.posPlatformY;
    }
    #endregion

    #region Underwater behavior
    private bool IsInWater()
    {
        return Physics2D.OverlapBox((Vector2)transform.position + boxCollider.offset, boxCollider.size, 0f, waterLayer) != null;
    }

    private void HandleBubbleState()
    {
        switch (bubbleType)
        {
            case BubbleType.NextBubble:
                SpawnBubble();
                break;
            case BubbleType.TimerBased:
                TimerBasedSpawnBubble();
                break;
        }
    }

    private void SpawnBubble()
    {
        if (IsInWater())
        {
            if (bubblePrefab != null)
            {
                // Define the player's collider bounds
                Bounds playerBounds = boxCollider.bounds;

                // Check if the player's collider is fully within the water layer
                bool isFullyInWater = IsColliderFullyInWater(playerBounds);

                if (isFullyInWater && currentBubble == null)
                {
                    currentBubble = Instantiate(bubblePrefab, bubblePos.position, Quaternion.identity);
                }
            }
        }       
    }

    private void TimerBasedSpawnBubble()
    {
        if (IsInWater())
        {
            if (bubblePrefab != null)
            {
                // Define the player's collider bounds
                Bounds playerBounds = boxCollider.bounds;
                // Check if the player's collider is fully within the water layer
                bool isFullyInWater = IsColliderFullyInWater(playerBounds);

                if (isFullyInWater)
                {
                    timer = 0f;
                    timer += Time.deltaTime;
                    if (timer >= spawnInterval)
                    {
                        Instantiate(bubblePrefab, bubblePos.position, Quaternion.identity);
                        timer = 0f; // Reset timer
                    }
                }
            }
        }
    }

    private bool IsColliderFullyInWater(Bounds colliderBounds)
    {
        // Get the corners of the collider bounds
        Vector2[] points = new Vector2[]
        {
        new Vector2(colliderBounds.min.x, colliderBounds.min.y),
        new Vector2(colliderBounds.min.x, colliderBounds.max.y),
        new Vector2(colliderBounds.max.x, colliderBounds.min.y),
        new Vector2(colliderBounds.max.x, colliderBounds.max.y)
        };

        // Check if all the points are within the water layer
        foreach (Vector2 point in points)
        {
            if (!IsPointInWater(point))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsPointInWater(Vector2 point)
    {
        // Check if the point is within the water layer
        // Make sure waterLayer is a LayerMask and adjust as needed
        return Physics2D.OverlapPoint(point, waterLayer) != null;
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

    public void OnSlide(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            slideButtonPressed = true;
        }
        else if (context.canceled)
        {
            slideButtonPressed = false;
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started) 
        {
            freezeEverything = !freezeEverything;
            GameManager.Instance.FreezeEverything(freezeEverything);
            GameManager.Instance.ToggleWeaponsMenu();
        }       
    }

    public void OnNextWeaponSwitch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SwitchToNextWeapon();
        }
    }

    public void OnPreviousWeaponSwitch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SwitchToPreviousWeapon();
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

        // Top check position and size
        Vector2 topCheckPos = (Vector2)transform.position + topCheckOffset;
        Gizmos.color = IsColAbove() ? Color.green : Color.red;
        Gizmos.DrawWireCube(topCheckPos, topCheckSize);

        // Front collision check position and size
        Vector2 frontCheckPos = (Vector2)transform.position + (facingRight ? frontCheckOffset : new Vector2(-frontCheckOffset.x, frontCheckOffset.y));
        Gizmos.color = IsFrontCollision() ? Color.green : Color.red;
        Gizmos.DrawWireCube(frontCheckPos, frontCheckSize);

        // Slide box collider visualization
        Gizmos.color = Color.yellow;
        Vector2 slideBoxPosition = (Vector2)transform.position + slideBoxOffset;
        Gizmos.DrawWireCube(slideBoxPosition, slideBoxSize);

        // ladder
        if (ladder != null)
        {
            // Draw Gizmos for the top and bottom of the ladder
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(ladder.transform.position.x, ladder.posTopHandlerY, transform.position.z), 0.2f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(new Vector3(ladder.transform.position.x, ladder.posBottomHandlerY, transform.position.z), 0.2f);

            // Optionally, draw a line to indicate the ladder’s full height
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(ladder.transform.position.x, ladder.posTopHandlerY, transform.position.z),
                            new Vector3(ladder.transform.position.x, ladder.posBottomHandlerY, transform.position.z));
        }
    }
    #endregion
}