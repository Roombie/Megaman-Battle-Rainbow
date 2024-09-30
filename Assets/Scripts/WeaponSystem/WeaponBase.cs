using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData; // Reference to the weapon's data
    protected List<GameObject> activeBullets = new(); // List to track active bullets

    public virtual bool CanShoot()
    {
        // Check if the weapon has enough energy and if bullet limits are respected
        return (weaponData.currentEnergy >= weaponData.energyCost) &&
               (!weaponData.limitBulletsOnScreen || activeBullets.Count < weaponData.maxBulletsOnScreen);
    }

    public virtual void Shoot(Transform shooterTransform, Vector2 bulletOffset, bool facingRight, int currentShootLevel)
    {
        if (!CanShoot())
            return;

        // Get the shoot position based on facing direction and the passed bullet offset
        Vector2 shootPosition = GetShootPosition(shooterTransform, bulletOffset, facingRight);

        // Get the bullet prefab (whether charged or uncharged)
        GameObject bulletPrefab = GetBulletPrefab(currentShootLevel);
        if (bulletPrefab == null)
        {
            Debug.LogError("Projectile prefab is not assigned!");
            return;
        }

        GameObject bulletInstance = Instantiate(bulletPrefab, shootPosition, Quaternion.identity);

        // Initialize the bullet
        Projectile bulletScript = bulletInstance.GetComponent<Projectile>();
        bulletScript.Initialize(weaponData, facingRight, currentShootLevel);

        activeBullets.Add(bulletInstance);

        bulletScript.OnBulletDestroyed += () => activeBullets.Remove(bulletInstance);

        DeductEnergy();
    }

    protected virtual void DeductEnergy()
    {
        weaponData.currentEnergy -= weaponData.energyCost;
    }

    protected virtual Vector2 GetShootPosition(Transform shooterTransform, Vector2 bulletOffset, bool facingRight)
    {
        Vector2 offset = facingRight ? bulletOffset : new Vector2(-bulletOffset.x, bulletOffset.y);
        return (Vector2)shooterTransform.position + offset;
    }

    // This method handles both charge levels and standard weaponPrefab
    protected virtual GameObject GetBulletPrefab(int currentShootLevel)
    {
        // Check if the weapon has charge levels
        if (weaponData.chargeLevels != null && weaponData.chargeLevels.Count > 0)
        {
            // Return the projectile prefab for the current charge level
            return weaponData.chargeLevels[currentShootLevel].projectilePrefab;
        }

        // If no charge levels exist, use the standard weapon prefab
        return weaponData.weaponPrefab;
    }

    // Override this to get the current level for chargeable weapons
    protected virtual int GetCurrentLevel() => 0;
}
