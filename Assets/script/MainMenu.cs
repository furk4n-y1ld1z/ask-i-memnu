using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string[] levelScenes = { "SampleScene", "Level2", "Level3" };
    [SerializeField] string title = "AŞK-I MEMNU";

    Font font;

    void Start()
    {
        Time.timeScale = 1f;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
    }

    void BuildUI()
    {
        GameObject canvasGO = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Arka plan
        Image bg = CreateRect("Background", canvas.transform).gameObject.AddComponent<Image>();
        bg.color = new Color(0.10f, 0.08f, 0.13f, 1f);
        RectTransform bgRt = bg.rectTransform;
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        // Baslik
        CreateLabel(canvas.transform, title, 96, FontStyle.Bold, new Vector2(0f, 320f), new Vector2(1400f, 200f), new Color(0.95f, 0.85f, 0.55f, 1f));

        // Butonlar
        CreateButton(canvas.transform, "OYNA", new Vector2(0f, 150f), () => LoadLevel(0));
        CreateButton(canvas.transform, "Bölüm 1", new Vector2(0f, 40f), () => LoadLevel(0));
        CreateButton(canvas.transform, "Bölüm 2", new Vector2(0f, -70f), () => LoadLevel(1));
        CreateButton(canvas.transform, "Bölüm 3", new Vector2(0f, -180f), () => LoadLevel(2));
        CreateButton(canvas.transform, "Çıkış", new Vector2(0f, -300f), Quit);
    }

    void LoadLevel(int index)
    {
        if (levelScenes == null || index < 0 || index >= levelScenes.Length)
            return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(levelScenes[index]);
    }

    void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ---- UI yardimcilari ----

    RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    void CreateLabel(Transform parent, string text, int size, FontStyle style, Vector2 pos, Vector2 sizeDelta, Color color)
    {
        RectTransform rt = CreateRect(text + "_Label", parent);
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;

        Text t = rt.gameObject.AddComponent<Text>();
        t.text = text;
        t.font = font;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
    }

    void CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rt = CreateRect(label + "_Button", parent);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(420f, 90f);

        Image img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0.85f, 0.75f, 0.45f, 1f);

        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        RectTransform textRt = CreateRect("Text", rt);
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        Text t = textRt.gameObject.AddComponent<Text>();
        t.text = label;
        t.font = font;
        t.fontSize = 40;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.12f, 0.08f, 0.05f, 1f);
    }
}
