using UnityEngine;

public class bihterzone : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float radius = 2.5f;
    [SerializeField] int meshSegments = 36;
    [SerializeField] Vector3 zoneOffset = new Vector3(0f, 0.1f, 0f);
    [SerializeField] Color outsideColor = new Color(0.2f, 0.95f, 0.25f, 0.25f);
    [SerializeField] Color insideColor = new Color(0.05f, 0.45f, 0.1f, 0.45f);

    Transform visualRoot;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh zoneMesh;
    Material zoneMaterial;

    void Awake()
    {
        EnsureVisuals();
        RebuildMesh();
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

        if (zoneMaterial != null)
            zoneMaterial.color = IsPlayerInside() ? insideColor : outsideColor;
    }

    void EnsureVisuals()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshFilter == null || meshRenderer == null)
        {
            if (visualRoot == null)
            {
                GameObject child = new GameObject("ZoneVisual");
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

        if (zoneMesh == null)
        {
            zoneMesh = new Mesh();
            zoneMesh.name = "BihterZoneMesh";
        }
        meshFilter.sharedMesh = zoneMesh;

        if (zoneMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                return;
            zoneMaterial = new Material(shader);
        }
        meshRenderer.sharedMaterial = zoneMaterial;
        meshRenderer.sortingOrder = 1;
    }

    void RebuildMesh()
    {
        if (zoneMesh == null)
            return;

        int segments = Mathf.Max(8, meshSegments);
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];
        Vector2 scaleComp = GetScaleCompensation();

        vertices[0] = new Vector3(zoneOffset.x * scaleComp.x, zoneOffset.y * scaleComp.y, 0f);
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = t * Mathf.PI * 2f;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector3 point = zoneOffset + new Vector3(dir.x, dir.y, 0f) * radius;
            vertices[i + 1] = new Vector3(point.x * scaleComp.x, point.y * scaleComp.y, 0f);
        }

        for (int i = 0; i < segments; i++)
        {
            int tri = i * 3;
            triangles[tri] = 0;
            triangles[tri + 1] = i + 1;
            triangles[tri + 2] = i + 2;
        }

        zoneMesh.Clear();
        zoneMesh.vertices = vertices;
        zoneMesh.triangles = triangles;
    }

    Vector2 GetScaleCompensation()
    {
        Vector3 s = transform.lossyScale;
        float x = Mathf.Abs(s.x) > 0.0001f ? 1f / Mathf.Abs(s.x) : 1f;
        float y = Mathf.Abs(s.y) > 0.0001f ? 1f / Mathf.Abs(s.y) : 1f;
        return new Vector2(x, y);
    }

    public bool IsPlayerInside()
    {
        if (player == null)
            return false;

        Vector2 center = (Vector2)transform.position + (Vector2)zoneOffset;
        float distance = Vector2.Distance(center, player.position);
        return distance <= radius;
    }

    Transform FindPlayer()
    {
        GameObject go = GameObject.Find("behlül");
        return go != null ? go.transform : null;
    }
}
