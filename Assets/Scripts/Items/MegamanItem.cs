using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegamanItem : MonoBehaviour
{
    public enum ItemType { Health, WeaponEnergy, ExtraLife, RandomItem }
    public ItemType itemType;

    public int value = 10; // How much health or energy the item restores?

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyItemEffect();
            Destroy(gameObject); // Destroy the item after collection
        }
    }

    private void ApplyItemEffect()
    {
        switch (itemType)
        {
            case ItemType.Health:
                GameManager.Instance.RestoreHealth(value);             
                break;
            case ItemType.WeaponEnergy:
                break;
            case ItemType.ExtraLife:
                GameManager.Instance.AddExtraLife(value);
                break;
        }
    }
}
