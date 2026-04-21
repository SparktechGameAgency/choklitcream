using UnityEngine;
using UnityEngine.InputSystem;   // <-- new namespace

public class PlayerMovement : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    // This method is called AUTOMATICALLY by the Input System
    // You must name it exactly: "On" + the Action name
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        // Flip sprite based on direction
        if (moveInput.x != 0)
            sr.flipX = moveInput.x < 0;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}