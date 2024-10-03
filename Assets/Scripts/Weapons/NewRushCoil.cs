using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class NewRushCoil : MonoBehaviour
{
    public float liftAmount = 5f;
    public float liftDuration = 0.2f;
    public float fallSpeed = 5f;
    public float activeDuration = 9f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    private float timer;
    public bool isPlayerOnRush;
    public bool hasJumped;
    private bool isActive;
    private bool isGrounded;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
