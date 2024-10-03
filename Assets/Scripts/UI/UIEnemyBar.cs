using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnemyBar : MonoBehaviour
{
    public Image mask;
    float originalSize;

    public static UIEnemyBar Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // get the initial height of the mask
        originalSize = mask.rectTransform.rect.height;
    }

    public void SetValue(float value)
    {
        Debug.Log("Setting health bar value to: " + value);
        // adjust the height of the mask to "hide" lost health bars
        mask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalSize * value);
    }
}
