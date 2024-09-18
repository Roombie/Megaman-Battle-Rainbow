using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MagnetBeam : MonoBehaviour
{
    [SerializeField] private float maxBeamLength = 30f;        // Maximum length of the beam
    [SerializeField] private float tileExtendInterval = 0.2f;  // How quickly the beam extends
    [SerializeField] private float lifetime = 5f;              // Time before the beam starts flashing
    [SerializeField] private float flashDuration = 2f;         // How long the flashing lasts before destruction
    [SerializeField] private float flashInterval = 0.1f;       // How quickly the beam flashes
    [SerializeField] private LayerMask wallLayer;              // LayerMask to specify the wall layer
    [SerializeField] private Vector2 offset = new (1f, 0f); // Offset for positioning the beam

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private float currentBeamLength = 0f;                     // Current length of the beam
    private float nextTileTime = 0f;                          // Timer to control when to extend the beam
    private bool isExtending = false;                          // Flag to control beam extension
    private Vector2 beamDirection = Vector2.right;             // Default direction is right
    private bool isColliding = false;                          // Flag to stop extending when hitting a wall
    private bool isShooting = false;                           // Flag to check if shooting

    private void Awake()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (isExtending && Time.time >= nextTileTime && !isColliding)
        {
            ExtendBeam();
            nextTileTime = Time.time + tileExtendInterval;
        }

        // Continuously update the position if the beam is extending
        if (isExtending && !isColliding)
        {
            UpdatePosition();
        }

        // Check if the player has stopped shooting and the lifetime hasn't started
        if (!isShooting && !isExtending)
        {
            StartFlashing(); // Start the flashing and destruction process
            isShooting = true; // Reset flag to prevent repeated calls
        }
    }

    public void StartExtending()
    {
        isExtending = true;
        isShooting = true; // Ensure the shooting flag is true when starting extension
    }

    public void StopExtending()
    {
        isExtending = false;
        isShooting = false; // Player stopped shooting, so start the lifetime countdown
        Invoke(nameof(StartFlashing), lifetime);
    }

    public void SetBeamDirection(Vector2 direction)
    {
        beamDirection = direction;
        UpdateBeamFlip();
    }

    public void UpdatePosition(Vector2 newPosition)
    {
        transform.position = newPosition;
    }

    private void ExtendBeam()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("Cannot extend beam because SpriteRenderer is not assigned.");
            return;
        }

        // Calculate the length to extend per interval
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
            // Update the size of the collider
            boxCollider.size = new Vector2(newLength, boxCollider.size.y);

            // Update the offset based on the direction
            if (beamDirection == Vector2.right)
            {
                boxCollider.offset = new Vector2(newLength / 2f, boxCollider.offset.y);
            }
            else if (beamDirection == Vector2.left)
            {
                boxCollider.offset = new Vector2(-newLength / 2f, boxCollider.offset.y);
            }
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

        // Flip the sprite based on the direction
        spriteRenderer.flipX = beamDirection == Vector2.left;
    }

    private void StartFlashing()
    {
        StartCoroutine(FlashAndDestroy());
    }

    private IEnumerator FlashAndDestroy()
    {
        float elapsedTime = 0f;

        // Flash the beam for the duration before destruction
        while (elapsedTime < flashDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled; // Toggle sprite visibility
            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval;
        }

        // Ensure the beam is visible when destroyed
        spriteRenderer.enabled = true;

        // Destroy the game object
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            isColliding = true; // Stop extending when hitting a wall
            StopExtending(); // Stop the extension and start flashing timer
            Debug.Log("Beam collided with a wall and stopped extending.");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            isColliding = false;
        }
    }

    private void UpdatePosition()
    {
        // Update position based on the player's current facing direction and offset
        Vector2 newPosition = (Vector2)transform.position + (beamDirection * offset.x);
        transform.position = newPosition;
    }
}