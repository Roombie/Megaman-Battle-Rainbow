using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushCoil : MonoBehaviour
{
    public float jumpForce = 20f;
    public float activeDuration = 5f; // The duration for which Rush Coil remains active
    private float timer;
    public bool isPlayerOnRush;
    public bool hasJumped;
    private Animator animator;
    private bool isActive;

    private void Awake()
    {
        animator = GetComponent<Animator>();       
    }

    private void Start()
    {
        isActive = true;
        timer = activeDuration;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isPlayerOnRush", isPlayerOnRush);

        // Decrease the timer if Rush Coil is active
        if (isActive && !hasJumped)
        {
            timer -= Time.deltaTime;

            // If the timer reaches zero, make the Rush Coil uninteractable
            if (timer <= 0f)
            {
                MakeUninteractable();
            }
        }

        if (hasJumped)
        {
            // Change the layer of the Rush Coil to a layer that doesn't interact with the player
            gameObject.layer = LayerMask.NameToLayer("IgnorePlayer");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null && !hasJumped && isActive)
            {
                isPlayerOnRush = true;
                playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true;
                isActive = false; // Deactivate the timer once the player interacts with Rush Coil
            }
        }
    }

    private void MakeUninteractable()
    {
        isActive = false;
        gameObject.layer = LayerMask.NameToLayer("IgnorePlayer");
    }
}
