using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class Item : MonoBehaviour
{
    public enum ItemType { Empty, Health, WeaponEnergy, ExtraLife, ETank, LTank, MTank, WTank, STank, Screw, ScoreBall, RandomItem}
    public enum ObjectType { Permanent, Temporal, PowerUp }

    public ItemType itemType;
    public ObjectType objectType;

    [Tooltip("The amount of health, energy, or lives the item grants")]
    public int value = 10;

    public Sprite[] animationSprites; // Array of sprites for the animation
    public float animationSpeed = 0.25f;
    public AudioClip itemSound;
    public bool freezeEverything = true;

    public float temporalItemLifetime = 10f; // Time before a temporal item disappears
    public float flashDuration = 2f; // Time before disappearance to start flashing
    public float flashInterval = 0.2f; // Interval for flashing (visibility toggling)

    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float animationTimer;
    private bool isCollected = false;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (animationSprites.Length > 0)
        {
            spriteRenderer.sprite = animationSprites[0]; // Start with the first frame
        }

        if (objectType == ObjectType.Temporal)
        {
            // Start countdown and schedule the flash effect
            Invoke(nameof(StartFlashing), temporalItemLifetime - flashDuration);
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

    private void OnTriggerEnter2D(Collider2D collision)
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
            case ItemType.Empty:
                break;
            case ItemType.Health:
                playerObject.RestoreHealth(value, itemSound, freezeEverything);
                break;
            case ItemType.WeaponEnergy:
                playerObject.RestoreWeaponEnergy(value, itemSound);
                break;
            case ItemType.ExtraLife:
                GameManager.Instance.AddExtraLife(value);
                AudioManager.Instance.Play(itemSound);
                break;
            case ItemType.ScoreBall:
                GameManager.Instance.AddScorePoints(value);
                break;
            case ItemType.Screw:
                GameManager.Instance.AddScrew(value);
                break;
            default:
                InventoryManager.Instance.AddItem(itemType, 1);
                break;
        }
    }

    private void HandleItemCollection()
    {
        isCollected = true;

        // Stop flashing if it was started
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            spriteRenderer.enabled = true; // Make sure the sprite is visible when collected
        }

        switch (objectType)
        {
            case ObjectType.Permanent:
                Destroy(gameObject);
                break;
            case ObjectType.Temporal:
                Destroy(gameObject);
                break;
            case ObjectType.PowerUp:
                Destroy(gameObject);
                break;
        }
    }

    private void StartFlashing()
    {
        if (!isCollected)
        {
            flashCoroutine = StartCoroutine(FlashEffect());
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

    private IEnumerator FlashEffect()
    {
        while (!isCollected)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Toggle visibility
            yield return new WaitForSeconds(flashInterval);
        }
    }
}
