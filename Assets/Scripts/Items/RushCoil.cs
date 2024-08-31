using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushCoil : MonoBehaviour
{
    public float jumpForce = 20f;
    public bool isPlayerOnRush;
    public bool hasJumped;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isPlayerOnRush", isPlayerOnRush);
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
            if (playerRb != null && !hasJumped)
            {
                isPlayerOnRush = true;
                playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true; 
            }
        }
    }
}
