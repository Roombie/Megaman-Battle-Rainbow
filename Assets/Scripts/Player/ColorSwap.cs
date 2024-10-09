using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/*
 * Comes from this article with some minor changes by GameDev with Tony
 * And I added a minor change as well. A method called ColorToHex - Roombie
 * https://gamedevelopment.tutsplus.com/tutorials/how-to-use-a-shader-to-dynamically-swap-a-sprites-colors--cms-25129
 */

public class ColorSwap : MonoBehaviour
{
    SpriteRenderer mSpriteRenderer;
    Image mImage;

    Texture2D mColorSwapTex;
    public Color[] mSpriteColors;

    void Awake()
    {
        // sprite renderer of gameobject this script is attached to
        mSpriteRenderer = GetComponent<SpriteRenderer>();
        // if no sprite renderer try for Image
        if (mSpriteRenderer == null)
        {
            mImage = GetComponent<Image>();
        }

        InitColorSwapTex();
    }

    public void InitColorSwapTex()
    {
        Texture2D colorSwapTex = new(256, 1, TextureFormat.RGBA32, false, false);
        colorSwapTex.filterMode = FilterMode.Point;

        for (int i = 0; i < colorSwapTex.width; ++i)
            colorSwapTex.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));

        colorSwapTex.Apply();

        if (mSpriteRenderer != null)
        {
            mSpriteRenderer.material.SetTexture("_SwapTex", colorSwapTex);
        }
        else if (mImage != null)
        {
            mImage.material.SetTexture("_SwapTex", colorSwapTex);
        }

        mSpriteColors = new Color[colorSwapTex.width];
        mColorSwapTex = colorSwapTex;
    }

    public static int IntFromColor(Color color)
    {
        // Clamp the color components to the range of 0 to 1, then scale to 0-255
        int r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255), 0, 255);
        int g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255), 0, 255);
        int b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255), 0, 255);

        // Combine RGB values into a single int representing the hex value
        return (r << 16) | (g << 8) | b; // Equivalent to 0xRRGGBB
    }

    public static Color ColorFromInt(int c, float alpha = 1.0f)
    {
        int r = (c >> 16) & 0x000000FF;
        int g = (c >> 8) & 0x000000FF;
        int b = c & 0x000000FF;

        Color ret = ColorFromIntRGB(r, g, b);
        ret.a = alpha;

        return ret;
    }

    public static Color ColorFromIntRGB(int r, int g, int b)
    {
        return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, 1.0f);
    }

    public static int ColorToHex(Color color)
    {
        // Convert Unity Color (normalized values) to an integer representing the hex color
        int r = Mathf.RoundToInt(color.r * 255f);
        int g = Mathf.RoundToInt(color.g * 255f);
        int b = Mathf.RoundToInt(color.b * 255f);

        return (r << 16) | (g << 8) | b;  // Equivalent to 0xRRGGBB
    }

    public void SwapColors(List<int> indexes, List<Color> colors)
    {
        for (int i = 0; i < indexes.Count; ++i)
        {
            if (indexes[i] >= 0 && indexes[i] < mSpriteColors.Length) // Check bounds
            {
                mSpriteColors[indexes[i]] = colors[i];
                mColorSwapTex.SetPixel(indexes[i], 0, colors[i]);
            }
            else
            {
                Debug.LogWarning($"Index {indexes[i]} is out of bounds for sprite colors.");
            }
        }
        mColorSwapTex.Apply();
    }


    public void SwapColor(int index, Color color)
    {
        mSpriteColors[index] = color;
        mColorSwapTex.SetPixel(index, 0, color);
    }

    public void ApplyColor()
    {
        mColorSwapTex.Apply();
        Debug.Log("Color applied");
        // save the color swap texture to local storage for debugging
        //byte[] data = mColorSwapTex.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/../ColorSwapTexture.png", data);
    }
}