// Assets/Scripts/Weapons/Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected WeaponData weaponData;
    protected Vector2 direction;
    protected int damage;
    protected float bulletSpeed;
    protected int shootLevel = 0;
    protected bool hasHoming;
    protected float homingStrength;
    protected bool canFreeze;
    protected float freezeDuration;
    protected bool hasSplashDamage;
    protected float splashRadius;
    protected int splashDamage;
    protected bool canSlowTime;
    protected float slowAmount;
    protected float slowDuration;

    public delegate void BulletDestroyed();
    public event BulletDestroyed OnBulletDestroyed;

    private SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual void Initialize(WeaponData data, bool facingRight, int level)
    {
        weaponData = data;
        direction = facingRight ? Vector2.right : Vector2.left;
        damage = data.chargeLevels[level].damage;
        shootLevel = level;
        hasHoming = data.hasHoming;
        homingStrength = data.homingStrength;
        canFreeze = data.canFreeze;
        freezeDuration = data.freezeDuration;
        hasSplashDamage = data.hasSplashDamage;
        splashRadius = data.splashRadius;
        splashDamage = data.splashDamage;
        canSlowTime = data.canSlowTime;
        slowAmount = data.slowAmount;
        slowDuration = data.slowDuration;

        bulletSpeed = data.chargeLevels[level].bulletSpeed;
        FlipSpriteBasedOnDirection();

        // Add components based on weapon data     
    }

    private void FlipSpriteBasedOnDirection()
    {
        if (spriteRenderer != null)
        {
            // Flip the sprite based on the direction (left or right)
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    protected virtual void Update()
    {
        // Basic movement
        rb.velocity = direction * bulletSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ApplyEffects(collision.gameObject);
        Destroy(gameObject);
    }

    protected virtual void ApplyEffects(GameObject hitObject)
    {
        // Apply direct damage
        if (hitObject.CompareTag("Enemy"))
        {
            hitObject.GetComponent<EnemyController>().TakeDamage(damage);
        }

        // Additional effects handled by specific behaviors
    }

    // Destroy the projectile and notify listeners
    protected void DestroyProjectile()
    {
        // Invoke the bullet destruction event
        OnBulletDestroyed?.Invoke();
        Destroy(gameObject); // Destroy the bullet object
    }

    private void OnBecameInvisible()
    {
        // Destroy when bullet goes off-screen
        DestroyProjectile();
    }
}