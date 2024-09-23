using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Vector2 bulletDirection;
    [SerializeField] private float destroyDelay;
    [SerializeField] private int shootLevel = 0;

    private float destroyTime;

    public delegate void BulletDestroyed();
    public event BulletDestroyed OnBulletDestroyed;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetInteger("shootLevel", shootLevel);

        // remove this bullet once its time is up
        destroyTime -= Time.deltaTime;
        if (destroyTime < 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetShootLevel(int shootLevel)
    {
        this.shootLevel = shootLevel;
    }

    // Update is called once per frame
    public void SetBulletSpeed(float speed)
    {
        this.bulletSpeed = speed;
    }

    public void SetBulletDirection(Vector2 direction)
    {
        this.bulletDirection = direction;
    }

    public void SetDamageValue(int damage)
    {
        this.damage = damage;
    }

    public void SetDestroyDelay(float delay)
    {
        this.destroyDelay = delay;
    }

    public void Shoot()
    {
        // Flip the sprite based on the bullet's direction.
        // If the bullet is moving left (negative x direction), flip the sprite.
        Debug.Log("Shoot Level: " + shootLevel);
        sprite.flipX = (bulletDirection.x < 0);      // This ensures the bullet appears to face the correct direction based on its movement
        rb.velocity = bulletDirection * bulletSpeed; // The velocity is determined by multiplying the direction of the bullet by its speed.
        destroyTime = destroyDelay;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // check for collision with enemy
        if (other.gameObject.CompareTag("Enemy"))
        {
            // enemy controller will apply the damage our bullet can cause
            EnemyController enemy = other.gameObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(this.damage);
            }
            if (shootLevel != 2)
            {
                // remove the bullet - just not immediately
                Destroy(gameObject, 0.01f);
            }          
        }
    }

    void OnBecameInvisible()
    {
        OnBulletDestroyed?.Invoke(); // Notify listeners that the bullet is destroyed
        Destroy(gameObject);
    }
}
