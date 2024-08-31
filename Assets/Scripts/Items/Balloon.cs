using System.Collections;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    public float riseSpeed = 1.0f;
    public float sinkAmount = 0.1f;
    public float lifeTime = 5.0f;
    public float flashDuration = 1.5f;
    public GameObject explosionPrefab;
    public float flashDelay = 0.0833f;
    public float explosionLifetime = 1.0f;
    public float sinkDuration = 0.2f; // Duration for sinking movement

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float timer;
    private bool isFlashing = false;
    private bool isSteppedOn = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        timer = lifeTime;
    }

    void Update()
    {
        animator.SetBool("IsPlayerOnBalloon", isSteppedOn);
        timer -= Time.deltaTime;

        if (timer <= flashDuration && !isFlashing)
        {
            StartCoroutine(FlashBeforeDestroy());
        }

        if (timer <= 0)
        {
            Explode();
        }

        if (!isSteppedOn)
        {
            rb.velocity = new Vector2(0, riseSpeed); // Balloon rises only if the player is not on it
        }
    }

    private IEnumerator FlashBeforeDestroy()
    {
        isFlashing = true;
        for (int i = 0; i < 10; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(flashDelay);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(flashDelay);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.velocity.y < 0) // Check if the player is falling
            {
                Debug.Log("Player landed on the balloon!");
                isSteppedOn = true;
                StartCoroutine(SinkThenRise());
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isSteppedOn = false;
        }
    }

    private IEnumerator SinkThenRise()
    {
        Vector3 originalPosition = transform.position;
        Vector3 sinkPosition = new Vector3(transform.position.x, transform.position.y - sinkAmount, transform.position.z);

        float elapsedTime = 0f;

        while (elapsedTime < sinkDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, sinkPosition, elapsedTime / sinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset back to original rise speed after sinking
        rb.velocity = new Vector2(0, riseSpeed);
    }

    void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity)
            .GetComponent<Explosion>()?.SetDestroyDelay(explosionLifetime);
        Destroy(gameObject);
    }
}