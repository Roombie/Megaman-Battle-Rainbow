// Assets/Scripts/Weapons/Projectile.cs
// Handles the behavior and properties of individual projectiles (like bullets or beams)
// that are spawned when a weapon is fired.
// THIS APPLIES TO HELP CODING THE PROJECTILES FOR THE WEAPONS
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Vector2 direction;
    protected float bulletSpeed;
    protected SpriteRenderer spriteRenderer;

    public delegate void BulletDestroyed();
    public event BulletDestroyed OnBulletDestroyed;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void Initialize(WeaponData data, bool facingRight, int level)
    {
        // Set bullet speed and direction based on the facing direction
        bulletSpeed = data.chargeLevels[level].bulletSpeed;
        direction = facingRight ? Vector2.right : Vector2.left;

        // Set velocity
        rb.velocity = bulletSpeed * direction;

        AudioManager.Instance.Play(data.weaponClip);
        // Flip sprite based on direction
        FlipSpriteBasedOnDirection();
    }

    private void FlipSpriteBasedOnDirection()
    {
        if (spriteRenderer != null)
        {
            // Flip the sprite based on the direction (left or right)
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            ApplyEffects(other.gameObject); // Apply damage, effects, etc.
        }
    }

    protected virtual void OnDestroy()
    {
        OnBulletDestroyed?.Invoke();
    }

    // Called when the projectile goes off-screen
    private void OnBecameInvisible()
    {
        // Destroy the projectile when it becomes invisible
        Destroy(gameObject);
    }

    protected abstract void ApplyEffects(GameObject target);
}
