using UnityEngine;

public class BubbleRise : MonoBehaviour
{
    public float riseSpeed = 2.5f;
    private bool isInWater = false; // Start with the bubble not being in water
    private Collider2D waterCollider; // Reference to the water's collider
    private Camera mainCamera; // Reference to the main camera

    private void Start()
    {
        // Get the main camera reference
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (isInWater)
        {
            // Move the bubble upwards
            transform.Translate(riseSpeed * Time.deltaTime * Vector2.up);

            // Check if the bubble has reached the top of the water's collider
            if (waterCollider != null && transform.position.y >= waterCollider.bounds.max.y)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Destroy the bubble if it is out of the camera bounds
        if (!IsWithinCameraBounds())
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the bubble enters the water
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            isInWater = true;
            waterCollider = other; // Save the reference to the water's collider
        }
        else if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            // Destroy the bubble if it touches something that isn't the Player layer
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the bubble exits the water
        if (other == waterCollider)
        {
            isInWater = false;
            waterCollider = null; // Clear the reference to the water's collider
        }
    }

    private bool IsWithinCameraBounds()
    {
        // Get the camera's viewport bounds in world coordinates
        Vector3 cameraBottomLeft = mainCamera.ViewportToWorldPoint(Vector3.zero);
        Vector3 cameraTopRight = mainCamera.ViewportToWorldPoint(Vector3.one);

        // Check if the bubble is within the camera's bounds
        return transform.position.x >= cameraBottomLeft.x && transform.position.x <= cameraTopRight.x &&
               transform.position.y >= cameraBottomLeft.y && transform.position.y <= cameraTopRight.y;
    }
}
