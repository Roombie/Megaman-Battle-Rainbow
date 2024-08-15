using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    int damage = 0;

    public void SetDamageValue (int damage)
    {
        this.damage = damage;
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
}
