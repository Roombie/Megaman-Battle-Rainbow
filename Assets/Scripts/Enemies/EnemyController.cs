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

    public void TakeDamage(int damage)
    {
        // take damage if not invincible
        if (!isInvincible)
        {
            audioSource.PlayOneShot(damageClip);
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
        // check for collision with player
        if (other.gameObject.CompareTag("Player"))
        {
            // colliding with player inflicts damage and takes contact damage away from health
            Megaman player = other.gameObject.GetComponent<Megaman>();
            player.HitSide(transform.position.x > player.transform.position.x);
            player.TakeDamage(this.contactDamage);
        }
    }
}
