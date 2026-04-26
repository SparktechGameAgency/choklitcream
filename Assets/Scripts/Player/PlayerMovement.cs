
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 5f;

    [Header("Mouse Settings")]
    public float mouseStopRadius = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Camera cam;

    private Vector2 wasdInput;
    private Vector2 finalDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;

        // Debug checks
        if (rb == null) Debug.LogError("[Player] Rigidbody2D missing!");
        if (sr == null) Debug.LogError("[Player] SpriteRenderer missing!");
        if (cam == null) Debug.LogError("[Player] Camera.main not found!");
    }

    void OnMove(InputValue value)
    {
        wasdInput = value.Get<Vector2>();
    }

    void Update()
    {
        finalDirection = Vector2.zero;

        // ── Mouse hold ─────────────────────────────────────────
        if (Mouse.current == null)
        {
            Debug.LogWarning("[Player] Mouse.current is null!");
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            if (cam == null)
            {
                // Try finding camera again
                cam = Camera.main;
                Debug.LogWarning("[Player] Camera was null — retrying...");
            }

            if (cam != null)
            {
                // Key fix: set z to camera depth before converting
                Vector3 mouseScreen = Mouse.current.position.ReadValue();
                mouseScreen.z = Mathf.Abs(cam.transform.position.z);

                Vector2 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
                Vector2 toMouse = mouseWorld - (Vector2)transform.position;

                Debug.Log("[Player] Mouse world pos: " + mouseWorld
                        + " | Player pos: " + (Vector2)transform.position
                        + " | Distance: " + toMouse.magnitude
                        + " | Direction: " + toMouse.normalized);

                if (toMouse.magnitude > mouseStopRadius)
                    finalDirection = toMouse.normalized;
                else
                    Debug.Log("[Player] Too close to cursor — not moving");
            }
        }

        // ── WASD overrides mouse ───────────────────────────────
        if (wasdInput.sqrMagnitude > 0.01f)
            finalDirection = wasdInput.normalized;

        // ── Flip sprite ────────────────────────────────────────
        if (finalDirection.x > 0.01f) sr.flipX = false;
        else if (finalDirection.x < -0.01f) sr.flipX = true;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = finalDirection * moveSpeed;

        if (finalDirection != Vector2.zero)
            Debug.Log("[Player] Moving: " + finalDirection
                    + " | Velocity: " + rb.linearVelocity);
    }
}