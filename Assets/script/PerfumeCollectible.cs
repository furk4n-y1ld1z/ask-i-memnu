using UnityEngine;

public class PerfumeCollectible : MonoBehaviour
{
    private LevelManager levelManager;

    void Start()
    {
        // Find the new LevelManager instead!
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (levelManager != null)
            {
                levelManager.CollectPerfume();
            }
            Destroy(gameObject);
        }
    }
}