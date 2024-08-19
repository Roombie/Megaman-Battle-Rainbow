using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    int damage = 0;
    bool freezeExplosion;
    float animatorSpeed;

    float destroyTimer;
    float destroyDelay;

    Animator animator;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        // get components
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetDestroyDelay(destroyDelay);
    }

    private void Update()
    {
        if (freezeExplosion) return;

        if (destroyDelay > 0)
        {
            destroyTimer -= Time.deltaTime;
            if (destroyTimer <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetDamageValue (int damage)
    {
        this.damage = damage;
    }

    public void SetDestroyDelay(float delay)
    {
        this.destroyDelay = delay;
        this.destroyTimer = delay;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (this.damage > 0)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Megaman player = other.gameObject.GetComponent<Megaman>();
                player.HitSide(transform.position.x > player.transform.position.x);
                player.TakeDamage(this.damage);
            }
        }
    }

    public void FreezeExplosion(bool freeze)
    {
        // freeze/unfreeze the explosions on screen
        // NOTE: this will be called from the GameManager but could be used in other scripts
        if (freeze)
        {
            freezeExplosion = true;
            animatorSpeed = animator.speed;
            animator.speed = 0;
        }
        else
        {
            freezeExplosion = false;
            animator.speed = animatorSpeed;
        }
    }
}
