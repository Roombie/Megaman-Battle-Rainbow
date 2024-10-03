using UnityEngine;
using UnityEngine.UI;

public class UIEnergyBar : MonoBehaviour
{
    public Image mask;  // The image mask for energy levels
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
        Debug.Log("Setting energy bar value to: " + value);
        mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize * value);
    }
}
