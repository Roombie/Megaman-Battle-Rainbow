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

        // Instantiate the Magnet Beam only if the player is allowed to shoot
        if (CanShoot())
        {
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
    }

    public void UpdateMagnetBeamPosition(Vector2 newPosition)
    {
        if (currentMagnetBeam != null)
        {
            MagnetBeam magnetBeamScript = currentMagnetBeam.GetComponent<MagnetBeam>();
            if (magnetBeamScript != null)
            {
                magnetBeamScript.UpdateBeamPosition(newPosition);
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
            }
        }
    }

    public bool HasReachedMaxLength()
    {
        return currentMagnetBeam != null && currentMagnetBeam.GetComponent<MagnetBeam>().hasReachedMaxLength;
    }
}
