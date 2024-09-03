using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewRushCoil : MonoBehaviour
{
    public float jumpForce = 35f;
    public float activeDuration = 5f;
    public LayerMask groundLayer; // specify the ground layer
    private float timer;
    public bool isPlayerOnRush;
    public bool hasJumped;
    private bool isActive;
    private bool isGrounded;
    private bool isInAir;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        isActive = true;
        timer = activeDuration;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isPlayerOnRush", isPlayerOnRush);

        // Decrease the timer if Rush Coil is active
        if (isActive)
        {
            timer -= Time.deltaTime;

            // If the timer reaches zero, make the Rush Coil uninteractable
            if (timer <= 0f)
            {
                MakeUninteractable();
            }
        }

        // Make the Rush Coil uninteractable once the player lands after jumping
        if (hasJumped && !isInAir && isGrounded)
        {
            MakeUninteractable();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            // Check if the player is landing on the Rush Coil
            if (playerRb != null && playerRb.velocity.y < 0 && !hasJumped && isActive)
            {
                isPlayerOnRush = true;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true;
                isInAir = true; // Player is now in the air after jumping
            }
        }

        // Check if the collision is with the ground layer
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;

            // If the player was in the air, they've now landed
            if (isInAir)
            {
                isInAir = false;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Reset the grounded state when the player leaves the ground
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
        }
    }

    private void MakeUninteractable()
    {
        isActive = false;
        boxCollider.enabled = false;
    }
}