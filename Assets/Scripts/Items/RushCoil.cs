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
        boxCollider.isTrigger = hasJumped;
        rb.isKinematic = hasJumped;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null && !hasJumped)
            {
                isPlayerOnRush = true;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true; 
            }
        }
    }
}
