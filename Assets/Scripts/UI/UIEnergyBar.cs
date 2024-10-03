using UnityEngine;
using UnityEngine.UI;

public class UIEnergyBar : MonoBehaviour
{
    public Image mask;  // The image mask for energy levels
    public Image weaponImage;  // The image to represent the current weapon's energy bar
    float originalSize;

    public static UIEnergyBar Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalSize = mask.rectTransform.rect.height;
        if (weaponImage == null)
        {
            Debug.LogError("Weapon Image is not assigned in the Inspector!");
        }
    }

    public void SetValue(float value)
    {
        Debug.Log("Setting energy bar value to: " + value);
        mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize * value);
    }

    // Assign the sprite from WeaponData to the energy bar's Image
    public void SetWeaponBarSprite(Sprite weaponSprite)
    {
        if (weaponImage == null)
        {
            Debug.LogError("Weapon Image reference is missing!");
            return;
        }

        if (weaponSprite != null)
        {
            weaponImage.sprite = weaponSprite;
            Debug.Log($"Weapon bar sprite set to {weaponSprite.name}");
        }
        else
        {
            Debug.LogError("Weapon sprite is null.");
        }
    }

    public void SetVisibility(bool isVisible)
    {
        weaponImage.gameObject.SetActive(isVisible);
        Debug.Log($"Energy bar visibility set to {isVisible}");
    }
}
