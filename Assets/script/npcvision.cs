using UnityEngine;

public class npcvision : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float viewDistance = 5f;
    [SerializeField] float viewAngle = 70f;
    [SerializeField] int meshSegments = 20;
    [SerializeField] Vector3 viewOffset = new Vector3(0f, 0.2f, 0f);
    [SerializeField] Color normalColor = new Color(1f, 0.9f, 0.2f, 0.2f);
    [SerializeField] Color detectedColor = new Color(1f, 0.2f, 0.2f, 0.3f);
    
    // NEW: We need a way to tell the script what a "wall" is!
    [SerializeField] LayerMask obstacleMask;

    npcwander wander;
    Transform visualRoot;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh visionMesh;
    Material visionMaterial;
    Vector2 facingDirection = Vector2.right;

    void Awake()
    {
        wander = GetComponent<npcwander>();
        EnsureVisuals();
    }

    void Start()
    {
        if (player == null)
            player = FindPlayer();
    }

    void Update()
    {
        if (player == null)
            player = FindPlayer();

        UpdateFacingDirection();
        RebuildMesh();

        if (visionMaterial != null)
            visionMaterial.color = IsPlayerInView() ? detectedColor : normalColor;
    }

    void EnsureVisuals()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter == null || meshRenderer == null)
        {
            if (visualRoot == null)
            {
                GameObject child = new GameObject("VisionVisual");
                child.transform.SetParent(transform, false);
                child.transform.localPosition = Vector3.zero;
                visualRoot = child.transform;
            }

            meshFilter = visualRoot.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = visualRoot.gameObject.AddComponent<MeshFilter>();

            meshRenderer = visualRoot.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = visualRoot.gameObject.AddComponent<MeshRenderer>();
        }

        if (meshFilter == null || meshRenderer == null)
            return;

        if (visionMesh == null)
        {
            visionMesh = new Mesh();
            visionMesh.name = "NPCVisionMesh";
        }
        meshFilter.sharedMesh = visionMesh;

        if (visionMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                return;
            visionMaterial = new Material(shader);
        }
        meshRenderer.sharedMaterial = visionMaterial;
        meshRenderer.sortingOrder = 2;
    }

    void UpdateFacingDirection()
    {
        if (wander == null)
            return;

        Vector2 dir = wander.FacingDirection;
        if (dir.sqrMagnitude > 0.01f)
            facingDirection = dir.normalized;
    }

    void RebuildMesh()
    {
        if (visionMesh == null)
            return;

        int segments = Mathf.Max(3, meshSegments);
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];
        Vector2 scaleComp = GetScaleCompensation();

        vertices[0] = new Vector3(viewOffset.x * scaleComp.x, viewOffset.y * scaleComp.y, 0f);
        float halfAngle = viewAngle * 0.5f;
        
        // Get the world position of the eye origin
        Vector2 rayOrigin = (Vector2)transform.position + new Vector2(viewOffset.x, viewOffset.y);

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector2 dir = Rotate(facingDirection, angle);
            
            // NEW: Raycast to find walls!
            float currentDistance = viewDistance;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir, viewDistance, obstacleMask);
            
            if (hit.collider != null)
            {
                // If the ray hits a wall, stop drawing the mesh at that distance
                currentDistance = hit.distance;
            }

            Vector3 point = viewOffset + new Vector3(dir.x, dir.y, 0f) * currentDistance;
            vertices[i + 1] = new Vector3(point.x * scaleComp.x, point.y * scaleComp.y, 0f);
        }

        for (int i = 0; i < segments; i++)
        {
            int tri = i * 3;
            triangles[tri] = 0;
            triangles[tri + 1] = i + 1;
            triangles[tri + 2] = i + 2;
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
    }

    Vector2 GetScaleCompensation()
    {
        Vector3 s = transform.lossyScale;
        float x = Mathf.Abs(s.x) > 0.0001f ? 1f / Mathf.Abs(s.x) : 1f;
        float y = Mathf.Abs(s.y) > 0.0001f ? 1f / Mathf.Abs(s.y) : 1f;
        return new Vector2(x, y);
    }

    public bool IsPlayerInView()
    {
        if (player == null)
            return false;

        Vector2 origin = (Vector2)transform.position + (Vector2)viewOffset;
        Vector2 toPlayer = (Vector2)player.position - origin;
        float distance = toPlayer.magnitude;
        
        if (distance > viewDistance || distance < 0.001f)
            return false;

        float angle = Vector2.Angle(facingDirection, toPlayer.normalized);
        if (angle > viewAngle * 0.5f) 
            return false;
            
        // NEW: Line of Sight Check!
        // Shoot a ray at Behlül. If it hits an obstacle first, he is safe.
        RaycastHit2D hit = Physics2D.Raycast(origin, toPlayer.normalized, distance, obstacleMask);
        if (hit.collider != null)
        {
            return false; 
        }

        return true;
    }

    static Vector2 Rotate(Vector2 vector, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    Transform FindPlayer()
    {
        GameObject go = GameObject.Find("behlül");
        return go != null ? go.transform : null;
    }
}