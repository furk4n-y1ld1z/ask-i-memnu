using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator; // Add a reference to the Animator component

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Cache the Animator on start
    }

    void Update()
    {
        // 1. Get input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. Normalize movement
        if (movement.sqrMagnitude > 1)
        {
            movement.Normalize();
        }

        // 3. ONLY update the Animator if we are actually pressing a key
        if (movement != Vector2.zero)
        {
            animator.SetFloat("HorizontalInput", movement.x);
            animator.SetFloat("VerticalInput", movement.y);
        }
    }

    void FixedUpdate()
    {
        // Apply movement
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}