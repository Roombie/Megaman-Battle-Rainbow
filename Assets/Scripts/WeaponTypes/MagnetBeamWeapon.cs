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
            // Prevent more than 5 beams being instantiated at the same time
            if (GameObject.FindGameObjectsWithTag("PlatformBeam").Length < 5)
            {
                // Instantiate the Magnet Beam
                currentMagnetBeam = Instantiate(GetBulletPrefab(currentShootLevel), shootPosition, Quaternion.identity);

                // Set the direction of the beam and start extending
                MagnetBeam magnetBeamScript = currentMagnetBeam.GetComponent<MagnetBeam>();
                magnetBeamScript.SetBeamDirection(facingRight ? Vector2.right : Vector2.left);
                magnetBeamScript.StartExtending();

                // Deduct energy after shooting
                DeductEnergy();

                // Play weapon sound
                if (weaponData.weaponClip != null)
                {
                    AudioManager.Instance.Play(weaponData.weaponClip);
                }

                // Add the beam to the active bullets list to keep track
                activeBullets.Add(currentMagnetBeam);
            }
        }
    }

    // Stop extending the beam and remove it
    public void StopBeam()
    {
        if (currentMagnetBeam != null)
        {
            currentMagnetBeam.GetComponent<MagnetBeam>().StopExtending();
            activeBullets.Remove(currentMagnetBeam);
            Destroy(currentMagnetBeam);
        }
    }
}
