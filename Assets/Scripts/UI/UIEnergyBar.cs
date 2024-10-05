using UnityEngine;
using UnityEngine.UI;

public class UIEnergyBar : MonoBehaviour
{
    public Image mask;  // The image mask for energy levels
    public Image weaponImage;
    float originalSize;

    public static UIEnergyBar Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        originalSize = mask.rectTransform.rect.height;
    }

    public void SetValue(float value)
    {
        // Debug.Log("Setting energy bar value to: " + value);
        mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize * value);
    }

    public void SetEnergyBar(Sprite weaponBarImage)
    {
        if (weaponImage != null)
        {
            // Set the weaponImage's sprite to the passed weapon sprite
            weaponImage.sprite = weaponBarImage;
            // Debug.Log("Weapon image updated successfully!");
        }
        else
        {
            Debug.LogError("Weapon image is not assigned or missing!");
        }
    }

    public void SetVisibility(bool isVisible)
    {
        gameObject.GetComponent<CanvasGroup>().alpha = isVisible ? 1f : 0f;     
        // Debug.Log($"Energy bar visibility set to {isVisible}");
    }
}
