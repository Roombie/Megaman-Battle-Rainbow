using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MagnetBeam : MonoBehaviour
{
    [SerializeField] private float maxBeamLength = 30f;
    [SerializeField] private float tileExtendInterval = 0.2f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float flashDuration = 2f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Vector2 offset = new(1f, 0f);
    [SerializeField] private float wallRadius = 0.1f;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Megaman megaman;
    private float currentBeamLength = 0f;
    private float nextTileTime = 0f;
    private bool isExtending = false;
    public bool hasReachedMaxLength = false; // Track if beam reaches max length
    private Vector2 beamDirection = Vector2.right;
    private bool isFlashing = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        megaman = FindObjectOfType<Megaman>();
    }

    private void Update()
    {
        if (megaman != null && megaman.freezeInput)
        {
            // Stop extending the beam if input is frozen
            StopExtending();
            return;  // Exit Update early if input is frozen
        }

        if (isExtending && Time.time >= nextTileTime)
        {
            ExtendBeam();
            nextTileTime = Time.time + tileExtendInterval;

            // Only update position while extending
            UpdateBeamPosition((Vector2)transform.position);
        }

        if (!isExtending && !isFlashing)
        {
            StartFlashing(); // Start the flashing and destruction process
        }

        PerformRaycast(); // Check for walls
    }

    public void StartExtending()
    {
        isExtending = true;
        hasReachedMaxLength = false; // Reset the max length flag
        Debug.Log("The beam is extending now");
    }

    public void StopExtending()
    {
        if (!isExtending) return;

        isExtending = false;
        hasReachedMaxLength = true; // Set the flag when the beam stops
        Debug.Log("Beam has stopped extending.");
        Invoke(nameof(StartFlashing), lifetime);
    }

    public void SetBeamDirection(Vector2 direction)
    {
        beamDirection = direction.normalized;
        UpdateBeamFlip();
    }

    public void UpdateBeamPosition(Vector2 newPosition)
    {
        if (isExtending) // Only update position if the beam is extending
        {
            transform.position = newPosition;
        }
    }

    private void ExtendBeam()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("Cannot extend beam because SpriteRenderer is not assigned.");
            return;
        }

        float extendAmount = spriteRenderer.sprite.bounds.size.x;

        // Ensure we don't exceed the maximum beam length
        if (currentBeamLength + extendAmount <= maxBeamLength)
        {
            currentBeamLength += extendAmount;
            UpdateBeamLength(currentBeamLength);
        }
        else
        {
            Debug.Log("Max beam length reached.");
            StopExtending(); // Stop extending when max length is reached
        }
    }

    private void UpdateBeamLength(float newLength)
    {
        if (spriteRenderer.drawMode == SpriteDrawMode.Tiled)
        {
            Vector2 newSize = new(newLength, spriteRenderer.size.y);
            spriteRenderer.size = newSize;
        }
        else
        {
            Debug.LogWarning("SpriteRenderer draw mode is not set to Tiled.");
        }

        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(newLength, boxCollider.size.y);

            // Adjust collider based on direction
            boxCollider.offset = new Vector2(
                beamDirection == Vector2.right ? newLength / 2f : -newLength / 2f,
                boxCollider.offset.y
            );
        }
        else
        {
            Debug.LogError("BoxCollider2D is not assigned. Cannot update beam collider size.");
        }
    }

    private void UpdateBeamFlip()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("Cannot flip beam because SpriteRenderer is not assigned.");
            return;
        }

        if (isExtending)
        {
            spriteRenderer.flipX = beamDirection == Vector2.left;
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
        Vector2 raycastStart = (Vector2)transform.position;
        Vector2 raycastEnd = raycastStart + (beamDirection * currentBeamLength);

        RaycastHit2D hit = Physics2D.Raycast(raycastStart, beamDirection, currentBeamLength, wallLayer);

        if (hit.collider != null)
        {
            Debug.Log("Raycast hit: " + hit.collider.name);
            StopExtending(); // Stop if a wall is detected
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 raycastStart = (Vector2)transform.position + offset;
        Vector2 raycastEnd = raycastStart + (beamDirection * currentBeamLength);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(raycastStart, raycastEnd);

        RaycastHit2D hit = Physics2D.Raycast(raycastStart, beamDirection, currentBeamLength, wallLayer);
        if (hit.collider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.point, wallRadius);
        }
    }
}
