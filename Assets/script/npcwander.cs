using UnityEngine;

public class npcwander : MonoBehaviour
{
    [Header("Patrol Route (Your System)")]
    [SerializeField] Transform[] waypoints;             // Drag your path points here
    [SerializeField] float moveSpeed = 2.8f;
    
    [Header("Idle Adjustments (Partner's System)")]
    [SerializeField] float minWaitTime = 0.8f;          // Pause duration at each corner
    [SerializeField] float maxWaitTime = 2.5f;
    
    [Header("Animation Settings")]
    [SerializeField] float animationFps = 10f;
    [SerializeField] bool useMatmazelDirectionOrder = false;
    [SerializeField] Sprite[] directionalFrames = new Sprite[8];

    Vector2 target;
    float waitTimer;
    float frameTimer;
    bool waiting;
    int currentPointIndex = 0;                          // Tracks waypoint progression
    int currentDirection;                               // 0 up, 1 down, 2 left, 3 right
    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Animator animator = GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;

        // FIX: Force target to the first waypoint instantly if they are assigned
        if (waypoints != null && waypoints.Length > 0)
        {
            currentPointIndex = 0;
            target = waypoints[0].position;
        }
        else
        {
            target = transform.position;
        }

        BeginWait();
        SetDirection(3);
        SetAnimationFrame(0);
    }

    void Update()
    {
        Vector2 movement = Vector2.zero;

        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                waiting = false;
                PickNewTarget();
            }
        }
        else
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Vector2 current = transform.position;
            Vector2 toTarget = target - current;
            movement = toTarget.normalized;
            
            Vector2 next = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(next.x, next.y, transform.position.z);

            // Arrived at the current waypoint!
            if (Vector2.Distance(next, target) <= 0.05f)
            {
                AdvanceWaypoint();
                BeginWait(); // Optional: Makes the guard look around at the point
            }
        }

        UpdateDirection(movement);
        UpdateAnimation(movement.sqrMagnitude > 0.0001f);
    }

    void PickNewTarget()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        // Target the next point calculated by AdvanceWaypoint()
        target = waypoints[currentPointIndex].position;
    }

    void AdvanceWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Step up to next point index, loop back to zero if we finish the path
        currentPointIndex = (currentPointIndex + 1) % waypoints.Length;
    }

    void BeginWait()
    {
        waiting = true;
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    void UpdateDirection(Vector2 movement)
    {
        if (movement.sqrMagnitude <= 0.0001f)
            return;

        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            SetDirection(movement.x > 0f ? 3 : 2);
        else
            SetDirection(movement.y > 0f ? 0 : 1);
    }

    void SetDirection(int newDirection)
    {
        if (currentDirection == newDirection)
            return;

        currentDirection = newDirection;
        frameTimer = 0f;
        SetAnimationFrame(0);
    }

    void UpdateAnimation(bool moving)
    {
        if (spriteRenderer == null || directionalFrames == null || directionalFrames.Length < 8)
            return;

        if (!moving)
        {
            SetAnimationFrame(0);
            return;
        }

        frameTimer += Time.deltaTime * animationFps;
        int frameInPair = ((int)frameTimer) % 2;
        SetAnimationFrame(frameInPair);
    }

    void SetAnimationFrame(int frameInPair)
    {
        int startIndex = GetDirectionStartIndex(currentDirection);
        int spriteIndex = Mathf.Clamp(startIndex + frameInPair, 0, directionalFrames.Length - 1);
        Sprite sprite = directionalFrames[spriteIndex];
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }

    int GetDirectionStartIndex(int direction)
    {
        if (useMatmazelDirectionOrder)
        {
            if (direction == 2) return 0; // left
            if (direction == 3) return 2; // right
            if (direction == 1) return 4; // down
            return 6; // up
        }

        if (direction == 0) return 0; // up
        if (direction == 1) return 2; // down
        if (direction == 2) return 4; // left
        return 6; // right
    }

    // CRUCIAL: Kept exactly intact so npcvision.cs knows where to cast its rays!
    public Vector2 FacingDirection
    {
        get
        {
            if (currentDirection == 0) return Vector2.up;
            if (currentDirection == 1) return Vector2.down;
            if (currentDirection == 2) return Vector2.left;
            return Vector2.right;
        }
    }
}