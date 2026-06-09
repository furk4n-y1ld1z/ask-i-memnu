using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class playermove : MonoBehaviour
{
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] InputActionAsset inputActions;

    static readonly int IdleHash = Animator.StringToHash("idle");
    static readonly int IleriHash = Animator.StringToHash("walk_ileri");
    static readonly int GeriHash = Animator.StringToHash("walk_geri");
    static readonly int SolHash = Animator.StringToHash("walk_sol");
    static readonly int SagHash = Animator.StringToHash("walk_sag");

    Rigidbody2D rb;
    Animator animator;
    InputAction moveAction;
    Vector2 moveInput;
    int currentDirection = -1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        moveAction = inputActions.FindActionMap("Player").FindAction("Move");
    }

    void OnEnable()
    {
        moveAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    void UpdateAnimation()
    {
        if (animator == null)
            return;

        int direction = 0;
        if (moveInput.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                direction = moveInput.x > 0f ? 4 : 3;
            else
                direction = moveInput.y > 0f ? 1 : 2;
        }

        if (direction == currentDirection)
            return;

        currentDirection = direction;

        int stateHash;
        switch (direction)
        {
            case 1: stateHash = IleriHash; break;
            case 2: stateHash = GeriHash; break;
            case 3: stateHash = SolHash; break;
            case 4: stateHash = SagHash; break;
            default: stateHash = IdleHash; break;
        }

        animator.Play(stateHash);
    }
}
