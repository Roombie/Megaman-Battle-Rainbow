// Assets/Scripts/Weapons/Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected WeaponData weaponData;
    protected Vector2 direction;
    protected int damage;
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

    public virtual void Initialize(WeaponData data, bool facingRight)
    {
        weaponData = data;
        direction = facingRight ? Vector2.right : Vector2.left;
        damage = data.baseDamage;
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

        // Add components based on weapon data     
    }

    protected virtual void Update()
    {
        // Basic movement
        transform.Translate(20f * Time.deltaTime * direction);
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
}
