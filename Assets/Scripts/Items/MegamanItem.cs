using System.Collections;
using UnityEngine;

public class MegamanItem : MonoBehaviour
{
    public enum ItemType { Health, WeaponEnergy, ExtraLife, ETank, LTank, MTank, WTank, STank, RandomItem }
    public enum ObjectType { Permanent, Temporal, PowerUp }

    public ItemType itemType;
    public ObjectType objectType;

    public bool addToInventory = false;
    [Tooltip("The amount of health, energy, or lives the item grants")]
    public int value = 10;

    public Sprite[] animationSprites; // Array of sprites for the animation
    public float animationSpeed = 0.25f;
    public AudioClip itemSound;
    public bool freezeEverything = true;

    public float temporalItemLifetime = 10f; // Time before a temporal item disappears

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float animationTimer;
    private bool isCollected = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on this GameObject.");
            return;
        }

        if (animationSprites.Length > 0)
        {
            spriteRenderer.sprite = animationSprites[0]; // Start with the first frame
        }

        if (objectType == ObjectType.Temporal)
        {
            StartCoroutine(TemporalItemCountdown());
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
        if (!isCollected && collision.gameObject.CompareTag("Player"))
        {
            ApplyItemEffect(collision.gameObject);
            HandleItemCollection();
        }
    }

    private void ApplyItemEffect(GameObject player)
    {
        Megaman playerObject = player.GetComponent<Megaman>();

        if (playerObject == null)
        {
            Debug.LogError("No Megaman script found on the player.");
            return;
        }

        switch (itemType)
        {
            case ItemType.Health:
                playerObject.RestoreHealth(value, itemSound, freezeEverything);
                break;
            case ItemType.WeaponEnergy:
                // playerObject.RestoreWeaponEnergy(value);
                AudioManager.Instance.Play(itemSound, SoundCategory.SFX);
                break;
            case ItemType.ExtraLife:
                GameManager.Instance.AddExtraLife(value);
                AudioManager.Instance.Play(itemSound, SoundCategory.SFX);
                break;
            case ItemType.ETank:
                AudioManager.Instance.Play(itemSound);
                playerObject.RestoreFullHealth(itemSound);
                break;

                // Add cases for other item types (LTank, MTank, etc.)
        }

        if (addToInventory)
        {
            //InventoryManager.Instance.AddItem(this);
        }
    }

    private void HandleItemCollection()
    {
        isCollected = true;

        switch (objectType)
        {
            case ObjectType.Permanent:
                // May involve marking it as collected in save data
                Destroy(gameObject);
                break;
            case ObjectType.Temporal:
                // Destroy immediately after collection
                Destroy(gameObject);
                break;
            case ObjectType.PowerUp:
                // Store it permanently in inventory
                //InventoryManager.Instance.AddItem(this);
                Destroy(gameObject);
                break;
        }
    }

    private IEnumerator TemporalItemCountdown()
    {
        yield return new WaitForSeconds(temporalItemLifetime);
        if (!isCollected)
        {
            Destroy(gameObject); // Destroy the item if not collected within the lifetime
        }
    }
}
