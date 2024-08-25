using UnityEngine;
using UnityEngine.Localization;

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

    public Sprite GetOnSprite(Locale locale)
    {
        foreach (var set in languageSpriteSets)
        {
            if (set.locale == locale)
            {
                return set.onSprite;
            }
        }
        return null;
    }

    public Sprite GetOffSprite(Locale locale)
    {
        foreach (var set in languageSpriteSets)
        {
            if (set.locale == locale)
            {
                return set.offSprite;
            }
        }
        return null;
    }
}
