using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] float fillDuration = 10f;                                  // parfum bari tamamen dolma suresi (saniye)
    [SerializeField] Color fillColor = new Color(0.2f, 0.9f, 0.3f, 1f);
    [SerializeField] string menuSceneName = "MainMenu";

    [Header("Parfum Bar Dolum Ayari (elle ayarla)")]
    [SerializeField] Vector2 fillOffset = Vector2.zero;   // dolumu sag/sol-yukari/asagi kaydir (parfumbar local birimi)
    [SerializeField] float fillWidth = 0f;                // dolu haldeki genislik. 0 = sprite genisligini otomatik kullan
    [SerializeField] float fillHeight = 0f;               // dolum yuksekligi. 0 = sprite yuksekligini otomatik kullan

    enum State { Playing, Paused, Won, Lost }
    State state;
    float fill;

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
        state = State.Playing;

        bihterZone = FindFirstObjectByType<bihterzone>();
        npcVisions = FindObjectsByType<npcvision>(FindObjectsSortMode.None);

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
            if (esc)
                Resume();
        }
    }

    void Gameplay()
    {
        bool inZone = bihterZone != null && bihterZone.IsPlayerInside();
        if (!inZone)
            return;

        // Bihter dairesi icindeyken bir NPC gorus konisi gorurse game over
        if (IsSeen())
        {
            GameOver();
            return;
        }

        if (fillDuration > 0f)
            fill = Mathf.Clamp01(fill + Time.deltaTime / fillDuration);

        UpdateBar();

        if (fill >= 1f)
            LevelClear();
    }

    bool IsSeen()
    {
        if (npcVisions == null)
            return false;

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
        if (canvas == null)
            return;

        Transform[] all = canvas.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in all)
        {
            if (t.name == "PauseScreen") pauseScreen = t;
            else if (t.name == "GameOverScreen") gameOverScreen = t;
            else if (t.name == "WinScreen") winScreen = t;
        }

        BindButtons(pauseScreen);
        BindButtons(gameOverScreen);
        BindButtons(winScreen);

        HideScreen(pauseScreen);
        HideScreen(gameOverScreen);
        HideScreen(winScreen);
    }

    void BindButtons(Transform screen)
    {
        if (screen == null)
            return;

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
        if (bar == null)
            return;

        parfumBar = bar.GetComponent<SpriteRenderer>();
        if (parfumBar == null)
            return;

        float autoW = parfumBar.sprite != null ? parfumBar.sprite.bounds.size.x : 1f;
        float autoH = parfumBar.sprite != null ? parfumBar.sprite.bounds.size.y * 0.55f : 0.5f;

        parfumBarWidth = fillWidth > 0f ? fillWidth : autoW;
        parfumFillHeight = fillHeight > 0f ? fillHeight : autoH;
        parfumFillLeftX = -parfumBarWidth * 0.5f + fillOffset.x;

        // Kendi fill objeni elle koymak istersen parfumbar altina "ParfumFill" adinda bir obje ekle; script onu kullanir.
        Transform existing = bar.transform.Find("ParfumFill");
        GameObject fillGO = existing != null ? existing.gameObject : new GameObject("ParfumFill");

        SpriteRenderer sr = fillGO.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = fillGO.AddComponent<SpriteRenderer>();
        if (sr.sprite == null)
            sr.sprite = MakeUnitSprite();
        sr.color = fillColor;
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
        if (parfumFill == null)
            return;

        parfumFill.localPosition = new Vector3(parfumFillLeftX, fillOffset.y, 0f);
        parfumFill.localScale = new Vector3(parfumBarWidth * fill, parfumFillHeight, 1f);
    }

    // ---- Durum gecisleri ----

    void Pause()
    {
        state = State.Paused;
        Time.timeScale = 0f;
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
        ShowScreen(gameOverScreen);
    }

    void LevelClear()
    {
        state = State.Won;
        Time.timeScale = 0f;
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
        if (state == State.Paused)
            Resume();
        else
            OnRetryButton();
    }

    // ---- Yardimcilar ----

    void ShowScreen(Transform screen)
    {
        if (screen == null)
            return;

        RectTransform rt = screen as RectTransform;
        if (rt != null)
            rt.anchoredPosition = Vector2.zero;

        screen.gameObject.SetActive(true);
        screen.SetAsLastSibling();
    }

    void HideScreen(Transform screen)
    {
        if (screen != null)
            screen.gameObject.SetActive(false);
    }
}
