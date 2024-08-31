using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegamanItem : MonoBehaviour
{
    public enum ItemType { Health, WeaponEnergy, ExtraLife, ETank, LTank, MTank, WTank, STank, RandomItem }
    public ItemType itemType;

    public bool addToInventory = false;
    [Tooltip("The amount of health, energy, or lives the item grants")]
    public int value = 10;

    public Sprite[] animationSprites; // Array of sprites for the animation
    public float animationSpeed = 0.25f;
    public AudioClip itemSound;
    public bool freezeEverything = true;

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float animationTimer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animationSprites.Length > 0)
        {
            spriteRenderer.sprite = animationSprites[0]; // Start with the first frame
        }
    }

    private void Update()
    {
        AnimateSprite();
    }

    private void AnimateSprite()
    {
        if (animationSprites.Length > 1)
        {
            animationTimer += Time.unscaledDeltaTime;
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                currentFrame = (currentFrame + 1) % animationSprites.Length; // Loop through frames
                spriteRenderer.sprite = animationSprites[currentFrame];
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ApplyItemEffect();
            Destroy(gameObject); // Destroy the item after collection
        }
    }

    private void ApplyItemEffect()
    {
        switch (itemType)
        {
            case ItemType.Health:
                GameManager.Instance.RestoreHealth(value, itemSound, freezeEverything);
                break;
            case ItemType.WeaponEnergy:
                break;
            case ItemType.ExtraLife:
                GameManager.Instance.AddExtraLife(value);
                AudioManager.Instance.Play(itemSound, SoundCategory.SFX);
                break;
            case ItemType.ETank:
                AudioManager.Instance.Play(itemSound);
                GameManager.Instance.RestoreFullHealth(itemSound);
                break;
        }
    }
}
