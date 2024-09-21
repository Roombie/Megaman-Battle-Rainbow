using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyController : MonoBehaviour
{
    bool isInvincible;
    float animatorSpeed;
    Vector2 freezeVelocity;
    RigidbodyConstraints2D rb2dConstraints;

    public bool freezeEnemy;
    public int currentHealth;
    public int maxHealth = 1;
    public int contactDamage = 1;
    public int explosionDamage = 0;
    public float explosionDelayBeforeDestroy = 2f;
    public AudioClip damageClip;

    GameObject explodeEffect;
    [SerializeField] GameObject explosionEffectPrefab;

    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider2D;
    Rigidbody2D rb;
    Animator animator;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // start at full health
        currentHealth = maxHealth;
    }

    public void Invincible(bool invincibility)
    {
        isInvincible = invincibility;
    }

    public void FreezeEnemy(bool freeze)
    {
        if (freeze)
        {
            freezeEnemy = true;
            animatorSpeed = animator.speed;
            rb2dConstraints = rb.constraints;
            freezeVelocity = rb.velocity;
            animator.speed = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            freezeEnemy = false;
            animator.speed = animatorSpeed;
            rb.constraints = rb2dConstraints;
            rb.velocity = freezeVelocity;
        }
    }

    public void TakeDamage(int damage)
    {
        // take damage if not invincible
        if (!isInvincible)
        {
            AudioManager.Instance.Play(damageClip);
            // take damage amount from health and call defeat if no health
            currentHealth -= damage;
            Mathf.Clamp(currentHealth, 0, maxHealth);
            if (currentHealth <= 0)
            {
                Defeat();
            }
        }
    }

    void StartDefeatAnimation()
    {
        explodeEffect = Instantiate(explosionEffectPrefab);
        explodeEffect.name = explosionEffectPrefab.name;
        explodeEffect.transform.position = spriteRenderer.bounds.center;
        explodeEffect.GetComponent<Explosion>().SetDamageValue(this.explosionDamage);
        Destroy(explodeEffect, explosionDelayBeforeDestroy);
    }

    void StopDefeatAnimation()
    {
        Destroy(gameObject);
    }

    void Defeat()
    {
        StartDefeatAnimation();
        // remove this enemy *poof*
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Megaman player = other.gameObject.GetComponent<Megaman>();
            if (player != null)
            {
                // colliding with player inflicts damage and takes contact damage away from health
                player.HitSide(transform.position.x > player.transform.position.x);
                player.TakeDamage(this.contactDamage);
            }
        }
    }
}
