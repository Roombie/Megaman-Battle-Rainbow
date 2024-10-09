using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData weaponData; // Reference to the weapon's data
    protected List<GameObject> activeBullets = new(); // List to track active bullets

    public virtual bool CanShoot()
    {
        // Check the energy and active bullet count
        // Debug.Log($"Current Energy: {weaponData.currentEnergy}, Energy Cost: {weaponData.energyCost}");
        // Debug.Log($"Limit Bullets On Screen: {weaponData.limitBulletsOnScreen}, Active Bullets: {activeBullets.Count}, Max Bullets: {weaponData.maxBulletsOnScreen}");

        // Check if the weapon has enough energy and if bullet limits are respected
        return (weaponData.currentEnergy >= weaponData.energyCost) &&
               (!weaponData.limitBulletsOnScreen || activeBullets.Count < weaponData.maxBulletsOnScreen);
    }

    public virtual void Shoot(Transform shooterTransform, Vector2 bulletOffset, bool facingRight, int currentShootLevel, float shootRayLength = 1f)
    {
        if (!CanShoot())
            return;

        // Get the shoot position based on the passed bullet offset and facing direction
        Vector2 shootPosition = GetShootPosition(shooterTransform, bulletOffset, facingRight, currentShootLevel, shootRayLength);

        // Instantiate the bullet
        GameObject bulletPrefab = GetBulletPrefab(currentShootLevel);
        GameObject bulletInstance = Instantiate(bulletPrefab, shootPosition, Quaternion.identity);

        // Initialize the bullet
        Projectile bulletScript = bulletInstance.GetComponent<Projectile>();
        bulletScript.Initialize(weaponData, facingRight, currentShootLevel);

        // Add bullet to active bullets list to track for limits
        activeBullets.Add(bulletInstance);

        // Remove bullet from active list on destruction
        bulletScript.OnBulletDestroyed += () => activeBullets.Remove(bulletInstance);

        // Deduct energy after shooting
        DeductEnergy();
    }

    protected virtual void DeductEnergy()
    {
        // Deduct the weapon's energy based on the energy cost
        weaponData.currentEnergy -= weaponData.energyCost;
        // Ensure the energy does not go below zero
        weaponData.currentEnergy = Mathf.Max(weaponData.currentEnergy, 0);

        // Update the energy bar UI to reflect the new energy value
        UIEnergyBar.Instance.SetValue(weaponData.currentEnergy / (float)weaponData.maxEnergy);
    }

    protected virtual Vector2 GetShootPosition(Transform shooterTransform, Vector2 bulletOffset, bool facingRight, int currentShootLevel, float shootRayLength = 1f)
    {
        Vector2 offset = facingRight ? bulletOffset : new Vector2(-bulletOffset.x, bulletOffset.y);
        Vector2 shootPosition = (Vector2)shooterTransform.position + offset;

        // Add the shootRayLength to the shootPosition
        return shootPosition + (facingRight ? Vector2.right : Vector2.left) * shootRayLength;
    }


    // This method handles both charge levels and standard weaponPrefab
    protected virtual GameObject GetBulletPrefab(int currentShootLevel)
    {
        if (weaponData.chargeLevels.Count > 0)
        {
            return weaponData.chargeLevels[currentShootLevel].projectilePrefab;
        }

        return weaponData.weaponPrefab;  // Use the weapon prefab if no charge level exists
    }

    // Override this to get the current level for chargeable weapons
    protected virtual int GetCurrentLevel() => 0;
}
