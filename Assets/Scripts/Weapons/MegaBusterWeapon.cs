using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaBusterWeapon : WeaponBase
{
    // The Shoot method will handle bullet instantiation
    protected override GameObject GetBulletPrefab(int currentShootLevel)
    {
        if (weaponData.chargeLevels.Count > 0)
        {
            return weaponData.chargeLevels[currentShootLevel].projectilePrefab;
        }

        return weaponData.weaponPrefab;  // Use the weapon prefab if no charge level exists
    }
}
