using System.Collections;
using UnityEngine;

public class MegaBuster : Projectile
{
    private float destroyTime = 5f;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize(WeaponData data, bool facingRight, int level)
    {
        base.Initialize(data, facingRight, level);

        // Use bullet speed from WeaponData instead of getting it from the prefab
        bulletSpeed = data.chargeLevels[level].bulletSpeed;

        // Set initial velocity based on direction and bullet speed
        rb.velocity = direction * bulletSpeed;
    }

    protected override void Update()
    {
        // Reduce destroy time
        destroyTime -= Time.deltaTime;
        if (destroyTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            ApplyEffects(other.gameObject);
            Destroy(gameObject); // Destroy on collision
        }
    }
}
