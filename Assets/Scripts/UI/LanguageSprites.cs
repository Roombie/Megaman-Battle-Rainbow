using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LanguageSprites", menuName = "Localization/LanguageSprites")]
public class LanguageSprites : ScriptableObject
{
    [System.Serializable]
    public struct LanguageSpriteSet
    {
        public Locale locale;
        public Sprite onSprite;
        public Sprite offSprite;
    }

    public LanguageSpriteSet[] languageSpriteSets;

    private Dictionary<Locale, LanguageSpriteSet> spriteSetDict;

    private void OnEnable()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        spriteSetDict = new Dictionary<Locale, LanguageSpriteSet>();
        foreach (var set in languageSpriteSets)
        {
            if (!spriteSetDict.ContainsKey(set.locale))
            {
                spriteSetDict.Add(set.locale, set);
            }
        }
    }

    public Sprite GetSprite(Locale locale, bool isOn)
    {
        if (spriteSetDict.TryGetValue(locale, out var spriteSet))
        {
            return isOn ? spriteSet.onSprite : spriteSet.offSprite;
        }
        Debug.LogWarning($"Sprite for locale {locale} not found.");
        return null;
    }
}
