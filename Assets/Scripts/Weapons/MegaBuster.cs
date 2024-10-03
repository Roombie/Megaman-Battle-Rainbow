using System.Collections;
using UnityEngine;

public class MegaBuster : Projectile
{
    private float destroyTime = 5f;  
    private int damage;               // Damage value from ChargeLevel
    private ChargeLevel currentChargeLevel; // Store current charge level data
    private int currentLevel;          // Variable to hold the current charge level index

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize(WeaponData data, bool facingRight, int level)
    {
        base.Initialize(data, facingRight, level);

        // Store the current charge level index
        currentLevel = level;

        // Retrieve and store data from the specific charge level
        currentChargeLevel = data.chargeLevels[currentLevel];

        // Set bullet speed and damage
        bulletSpeed = currentChargeLevel.bulletSpeed;
        damage = currentChargeLevel.damage;
        AudioManager.Instance.Play(currentChargeLevel.weaponClip);

        // Set initial velocity based on direction and bullet speed
        rb.velocity = bulletSpeed * direction;
    }

    private void Update()
    {
        // Handle projectile's lifetime and destruction
        destroyTime -= Time.deltaTime;
        if (destroyTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Override the base OnTriggerEnter2D method
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            // Apply damage to the enemy
            ApplyEffects(other.gameObject);

            // Check if the current level is greater than 0
            if (currentLevel <= 1) // Check the current charge level
            {
                Destroy(gameObject);  // Destroy the bullet on collision
            }
        }
    }

    // Apply effects to the main target (like damage)
    protected override void ApplyEffects(GameObject target)
    {
        EnemyController enemy = target.GetComponent<EnemyController>();
        if (enemy != null)
        {
            // Apply base damage
            enemy.TakeDamage(damage);
        }
    }

    // Handle projectile becoming invisible (off-screen)
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
