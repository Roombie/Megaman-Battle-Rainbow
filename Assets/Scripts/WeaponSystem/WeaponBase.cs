// Assets/Scripts/Weapons/BaseWeapon.cs
using System.Collections;
using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    public WeaponData weaponData;
    protected Megaman playerController;
    protected float chargeTime; // Time spent holding the shoot button
    protected int currentChargeLevel; // Current charge level index
    protected bool isCharging; // Whether the weapon is charging

    protected virtual void Awake()
    {
        playerController = GetComponent<Megaman>();
    }

    public abstract void Fire();
    public abstract void Stop();


    protected void StartCharging()
    {
        chargeTime = 0f;
        isCharging = true;
    }

    protected void StopCharging()
    {
        isCharging = false;
        chargeTime = 0f;
        currentChargeLevel = 0;
    }

    protected int GetChargeLevel()
    {
        if (weaponData.chargeLevels == null || weaponData.chargeLevels.Count == 0)
        {
            return 0; // If there are no charge levels, just return the base charge level.
        }

        // Determine the current charge level based on how long the player has held the fire button.
        for (int i = weaponData.chargeLevels.Count - 1; i >= 0; i--)
        {
            if (chargeTime >= weaponData.chargeLevels[i].timeRequired)
            {
                return i; // Return the highest level for which the time requirement has been met.
            }
        }

        return 0;
    }

    public virtual void Shoot()
    {
        if (weaponData.currentEnergy <= 0) return;

        // Instantiate Projectile
        GameObject projectile = Instantiate(weaponData.weaponPrefab, GetShootPosition(), GetShootRotation());
        Projectile projScript = projectile.GetComponent<Projectile>();
        projScript.Initialize(weaponData, playerController.IsFacingRight);

        // Play Shoot Sound
        if (weaponData.weaponClip != null)
        {
            AudioManager.Instance.Play(weaponData.weaponClip);
        }

        // Reduce Ammo
        weaponData.currentEnergy--;

        // Implement Shoot Delay
        StartCoroutine(ShootDelay());
    }

    protected virtual Vector2 GetShootPosition()
    {
        // Calculate shoot position based on player's position and facing direction
        Vector2 offset = playerController.IsFacingRight ? (weaponData.currentEnergy > 0 ? Vector2.right : Vector2.left) : (weaponData.currentEnergy > 0 ? Vector2.left : Vector2.right);
        return (Vector2)transform.position + offset;
    }

    protected virtual Quaternion GetShootRotation()
    {
        // Adjust rotation based on facing direction
        return playerController.IsFacingRight ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
    }

    protected IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(weaponData.shootDelay);

    }

    public virtual void Reload()
    {
        weaponData.currentEnergy = weaponData.maxEnergy;
    }
}
