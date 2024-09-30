using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MagnetBeam : Projectile
{
    [SerializeField] private float maxBeamLength = 30f;
    [SerializeField] private float tileExtendInterval = 0.2f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float flashDuration = 2f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Vector2 offset = new(1f, 0f);
    [SerializeField] private float wallRadius = 0.1f;

    private new SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private float currentBeamLength = 0f;
    private float nextTileTime = 0f;
    private bool isExtending = false;
    public bool hasReachedMaxLength = false; // Track if beam reaches max length
    private bool isFlashing = false;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (isExtending && Time.time >= nextTileTime)
        {
            ExtendBeam();
            nextTileTime = Time.time + tileExtendInterval;
        }

        if (!isExtending && !isFlashing)
        {
            StartFlashing(); // Start flashing after stopping
        }

        PerformRaycast(); // Check for walls
    }

    public void StartExtending()
    {
        isExtending = true;
        hasReachedMaxLength = false; // Reset max length flag
    }

    public void StopExtending()
    {
        isExtending = false;
        hasReachedMaxLength = true; // Mark beam as finished extending
        Invoke(nameof(StartFlashing), lifetime);
    }

    public void SetBeamDirection(Vector2 direction)
    {
        this.direction = direction.normalized; // Using direction from Projectile class
        UpdateBeamFlip();
    }

    private void ExtendBeam()
    {
        if (currentBeamLength + spriteRenderer.sprite.bounds.size.x <= maxBeamLength)
        {
            currentBeamLength += spriteRenderer.sprite.bounds.size.x;
            UpdateBeamLength(currentBeamLength);
        }
        else
        {
            StopExtending();
        }
    }

    private void UpdateBeamLength(float newLength)
    {
        if (spriteRenderer.drawMode == SpriteDrawMode.Tiled)
        {
            spriteRenderer.size = new Vector2(newLength, spriteRenderer.size.y);
        }

        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(newLength, boxCollider.size.y);
            boxCollider.offset = new Vector2(direction.x > 0 ? newLength / 2f : -newLength / 2f, boxCollider.offset.y);
        }
    }

    private void UpdateBeamFlip()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private void StartFlashing()
    {
        if (isFlashing) return;

        isFlashing = true;
        StartCoroutine(FlashAndDestroy());
    }

    private IEnumerator FlashAndDestroy()
    {
        float elapsedTime = 0f;

        while (elapsedTime < flashDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval;
        }

        spriteRenderer.enabled = true;
        Destroy(gameObject);
    }

    private void PerformRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, currentBeamLength, wallLayer);

        if (hit.collider != null)
        {
            StopExtending(); // Stop when hitting a wall
        }
    }

    protected override void ApplyEffects(GameObject target)
    {
        
    }
}
