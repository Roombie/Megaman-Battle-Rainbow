using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetBeamWeapon : WeaponBase
{
    private GameObject currentMagnetBeam;

    public override void Shoot(Transform shooterTransform, Vector2 bulletOffset, bool facingRight, int currentShootLevel, float shootRayLength = 1f)
    {
        // Calculate the shoot position using the parent class method
        Vector2 shootPosition = GetShootPosition(shooterTransform, bulletOffset, facingRight, currentShootLevel, shootRayLength);

        Debug.Log($"CanShoot: {CanShoot()} | CurrentEnergy: {weaponData.currentEnergy} | EnergyCost: {weaponData.energyCost}");

        if (!CanShoot())
            return;

        // Instantiate the Magnet Beam
        currentMagnetBeam = Instantiate(GetBulletPrefab(currentShootLevel), shootPosition, Quaternion.identity);

        // Set the direction of the beam and start extending
        MagnetBeam magnetBeamScript = currentMagnetBeam.GetComponent<MagnetBeam>();
        if (magnetBeamScript != null)
        {
            magnetBeamScript.SetBeamDirection(facingRight ? Vector2.right : Vector2.left);
            magnetBeamScript.StartExtending();
        }

        DeductEnergy();

        // Play weapon sound
        if (weaponData.weaponClip != null)
        {
            AudioManager.Instance.Play(weaponData.weaponClip);
        }

        activeBullets.Add(currentMagnetBeam);
    }

    public void UpdateMagnetBeamPosition(Vector2 newPosition, bool facingRight)
    {
        if (currentMagnetBeam != null)
        {
            MagnetBeam magnetBeamScript = currentMagnetBeam.GetComponent<MagnetBeam>();
            if (magnetBeamScript != null)
            {
                // Update beam position
                magnetBeamScript.UpdateBeamPosition(newPosition);

                // Update beam direction based on the player's current facing direction
                Vector2 newDirection = facingRight ? Vector2.right : Vector2.left;
                magnetBeamScript.SetBeamDirection(newDirection);
            }
        }
    }


    public void StopBeam()
    {
        if (currentMagnetBeam != null)
        {
            MagnetBeam magnetBeamScript = currentMagnetBeam.GetComponent<MagnetBeam>();
            if (magnetBeamScript != null)
            {
                magnetBeamScript.StopExtending();
                activeBullets.Remove(currentMagnetBeam);
                currentMagnetBeam = null;
            }
        }
    }

    public bool HasReachedMaxLength()
    {
        return currentMagnetBeam != null && currentMagnetBeam.GetComponent<MagnetBeam>().hasReachedMaxLength;
    }
}
