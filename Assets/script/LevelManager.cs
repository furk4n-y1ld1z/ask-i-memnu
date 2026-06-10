using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro; 

public class LevelManager : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] float fillDuration = 10f; 
    [SerializeField] Color fillColor = new Color(0.2f, 0.9f, 0.3f, 1f);
    [SerializeField] string menuSceneName = "MainMenu";

    [Header("Gizlilik Ayarlari")]
    [SerializeField] float detectionTimeRequired = 1f; 
    float currentDetectionTime = 0f;

    [Header("Parfum Toplama")]
    [SerializeField] private int totalPerfumes = 3; 
    private int collectedPerfumes = 0;

    [Header("Parfum Bar Dolum Ayari (elle ayarla)")]
    [SerializeField] Vector2 fillOffset = Vector2.zero;   
    [SerializeField] float fillWidth = 0f;                
    [SerializeField] float fillHeight = 0f;               

    enum State { Playing, Paused, Won, Lost }
    State state;
    float fill;

    private float elapsedTime = 0f;
    private TMP_Text timeSurvivedText;
    private TMP_Text totalTimeText;

    bihterzone bihterZone;
    npcvision[] npcVisions;

    Transform pauseScreen;
    Transform gameOverScreen;
    Transform winScreen;

    SpriteRenderer parfumBar;
    Transform parfumFill;
    float parfumBarWidth = 1f;
    float parfumFillHeight = 0.6f;
    float parfumFillLeftX;

    void Start()
    {
        Time.timeScale = 1f;
        fill = 0f; 
        elapsedTime = 0f; 
        state = State.Playing;

        bihterZone = FindFirstObjectByType<bihterzone>();
        npcVisions = FindObjectsByType<npcvision>(FindObjectsSortMode.None);

        int foundPerfumes = GameObject.FindGameObjectsWithTag("Perfume").Length;
        if (foundPerfumes > 0)
        {
            totalPerfumes = foundPerfumes;
        }

        SetupScreens();
        SetupParfumBar();
        UpdateBar();
    }

    void Update()
    {
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        if (state == State.Playing)
        {
            if (esc)
            {
                Pause();
                return;
            }
            Gameplay();
        }
        else if (state == State.Paused)
        {
            if (esc) Resume();
        }
    }

    void Gameplay()
    {
        elapsedTime += Time.deltaTime;

        // 1. SURVIVAL CHECK
        if (IsSeen())
        {
            currentDetectionTime += Time.deltaTime;
            if (currentDetectionTime >= detectionTimeRequired)
            {
                GameOver();
                return;
            }
        }
        else
        {
            currentDetectionTime = 0f; 
        }

        // 2. ZONE CHECK
        bool inZone = bihterZone != null && bihterZone.IsPlayerInside();
        
        // 3. DRAIN PROGRESS
        if (inZone && HasAllPerfumes())
        {
            if (fillDuration > 0f)
            {
                fill -= Time.deltaTime / fillDuration; 
                fill = Mathf.Clamp01(fill);
                UpdateBar();
            }

            if (fill <= 0f)
            {
                LevelClear();
            }
        }
    }

    public void CollectPerfume()
    {
        collectedPerfumes++;
        Debug.Log("Parfum Toplandi! " + collectedPerfumes + " / " + totalPerfumes);

        if (totalPerfumes > 0)
        {
            fill = (float)collectedPerfumes / totalPerfumes;
            UpdateBar();
        }
    }

    public bool HasAllPerfumes()
    {
        if (totalPerfumes == 0) return true; 
        return collectedPerfumes >= totalPerfumes;
    }

    bool IsSeen()
    {
        if (npcVisions == null) return false;

        for (int i = 0; i < npcVisions.Length; i++)
        {
            if (npcVisions[i] != null && npcVisions[i].IsPlayerInView())
                return true;
        }
        return false;
    }

    // ---- Kurulum (kod ile otomatik baglama) ----

    void SetupScreens()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null) return;

        Transform[] all = canvas.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in all)
        {
            if (t.name == "PauseScreen") pauseScreen = t;
            else if (t.name == "GameOverScreen") gameOverScreen = t;
            else if (t.name == "WinScreen") winScreen = t;
            
            else if (t.name == "TimeSurvivedText") timeSurvivedText = t.GetComponent<TMP_Text>();
            else if (t.name == "TotalTimeText") totalTimeText = t.GetComponent<TMP_Text>();
        }

        BindButtons(pauseScreen);
        BindButtons(gameOverScreen);
        BindButtons(winScreen);

        HideScreen(pauseScreen);
        HideScreen(gameOverScreen);
        HideScreen(winScreen);
    }

    // --- FIX: Searches locally inside the active screen to update its specific CountText ---
    void UpdateCountTextDisplay(Transform screen)
    {
        if (screen == null) return;

        // Recursively searches all children, down through your PerfumeCount images
        TMP_Text[] allTexts = screen.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in allTexts)
        {
            if (t.gameObject.name == "CountText")
            {
                t.text = $"{collectedPerfumes} / {totalPerfumes}";
            }
        }
    }

    void BindButtons(Transform screen)
    {
        if (screen == null) return;

        Button[] buttons = screen.GetComponentsInChildren<Button>(true);
        foreach (Button b in buttons)
        {
            b.onClick.RemoveAllListeners();
            switch (b.gameObject.name)
            {
                case "ResumeButton": b.onClick.AddListener(Resume); break;
                case "RetryButton": b.onClick.AddListener(OnRetryButton); break;
                case "QuitButton": b.onClick.AddListener(OnQuitButton); break;
                case "NextLevelButton": b.onClick.AddListener(OnNextLevelButton); break;
                case "CloseButton": b.onClick.AddListener(OnCloseButton); break;
            }
        }
    }

    void SetupParfumBar()
    {
        GameObject bar = GameObject.Find("parfumbar");
        if (bar == null) return;

        parfumBar = bar.GetComponent<SpriteRenderer>();
        if (parfumBar == null) return;

        float autoW = parfumBar.sprite != null ? parfumBar.sprite.bounds.size.x : 1f;
        float autoH = parfumBar.sprite != null ? parfumBar.sprite.bounds.size.y * 0.55f : 0.5f;

        parfumBarWidth = fillWidth > 0f ? fillWidth : autoW;
        parfumFillHeight = fillHeight > 0f ? fillHeight : autoH;
        parfumFillLeftX = -parfumBarWidth * 0.5f + fillOffset.x;

        Transform existing = bar.transform.Find("ParfumFill");
        GameObject fillGO = existing != null ? existing.gameObject : new GameObject("ParfumFill");

        SpriteRenderer sr = fillGO.GetComponent<SpriteRenderer>();
        if (sr == null) sr = fillGO.AddComponent<SpriteRenderer>();
        
        if (sr.sprite == null) sr.sprite = MakeUnitSprite();
        
        sr.color = fillColor;
        sr.material = parfumBar.material; 
        sr.drawMode = SpriteDrawMode.Simple; 
        
        sr.sortingLayerID = parfumBar.sortingLayerID;
        sr.sortingOrder = parfumBar.sortingOrder + 1;

        fillGO.transform.SetParent(bar.transform, false);
        fillGO.transform.localPosition = new Vector3(parfumFillLeftX, fillOffset.y, 0f);
        fillGO.transform.localScale = new Vector3(0f, parfumFillHeight, 1f);
        parfumFill = fillGO.transform;
    }

    Sprite MakeUnitSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0f, 0.5f), 1f);
    }

    void UpdateBar()
    {
        if (parfumFill == null) return;

        parfumFill.localPosition = new Vector3(parfumFillLeftX, fillOffset.y, 0f);
        parfumFill.localScale = new Vector3(parfumBarWidth * fill, parfumFillHeight, 1f);
    }

    // ---- Durum gecisleri ----

    void Pause()
    {
        state = State.Paused;
        Time.timeScale = 0f;
        
        // Target specifically the pause screen components
        UpdateCountTextDisplay(pauseScreen);
        
        ShowScreen(pauseScreen);
    }

    public void Resume()
    {
        HideScreen(pauseScreen);
        Time.timeScale = 1f;
        state = State.Playing;
    }

    void GameOver()
    {
        state = State.Lost;
        Time.timeScale = 0f;
        
        if (timeSurvivedText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime - minutes * 60);
            timeSurvivedText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        
        // Target specifically the lose screen components
        UpdateCountTextDisplay(gameOverScreen);
        
        ShowScreen(gameOverScreen);
    }

    void LevelClear()
    {
        state = State.Won;
        Time.timeScale = 0f;
        
        if (totalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime - minutes * 60);
            totalTimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        
        // Target specifically the win screen components
        UpdateCountTextDisplay(winScreen);
        
        ShowScreen(winScreen);
    }

    // ---- Buton metodlari ----
    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnQuitButton()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
    }

    public void OnNextLevelButton()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else if (!string.IsNullOrEmpty(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
    }

    public void OnCloseButton()
    {
        if (state == State.Paused) Resume();
        else OnRetryButton();
    }

    void ShowScreen(Transform screen)
    {
        if (screen == null) return;
        RectTransform rt = screen as RectTransform;
        if (rt != null) rt.anchoredPosition = Vector2.zero;
        screen.gameObject.SetActive(true);
        screen.SetAsLastSibling();
    }

    void HideScreen(Transform screen)
    {
        if (screen != null) screen.gameObject.SetActive(false);
    }
}