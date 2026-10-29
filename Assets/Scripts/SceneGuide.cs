using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Builds the project's world-space menus at runtime, in the style of the Leslie Room canvas
/// (light panel, dark bold text, grey rounded buttons).
///  - Main Menu mode: a title and one button per scene.
///  - Room mode: a step-by-step guide (one tip per page) with Back / Next, a "Main Menu" button,
///    and a small bar that stays after the guide is closed. Press Y (left controller) to bring the
///    guide back in front of you from anywhere.
/// </summary>
public class SceneGuide : MonoBehaviour
{
    public enum Mode { MainMenu, Room }

    [System.Serializable]
    public class Page
    {
        public string title;
        [TextArea(2, 6)] public string body;
    }

    [System.Serializable]
    public class SceneButton
    {
        public string label;
        public string sceneName;
    }

    public Mode mode = Mode.Room;

    [Header("Main Menu")]
    public string menuTitle = "Welcome to Leslie's project";
    public string menuSubtitle = "Choose a scene to explore";
    public List<SceneButton> sceneButtons = new List<SceneButton>();

    [Header("Room guide")]
    public List<Page> pages = new List<Page>();
    public string mainMenuScene = "Main Menu";
    [Tooltip("Optional panel (e.g. the old Settings panel) opened by a Settings button.")]
    public GameObject settingsPanel;

    [Header("Placement")]
    public float distance = 1.6f;
    public float height = 1.45f;

    [Header("Style (matches the Leslie Room canvas)")]
    public TMP_FontAsset font;
    public Sprite roundedSprite;
    public Color panelColor = new Color(1f, 1f, 1f, 0.96f);
    public Color textColor = new Color(0.04f, 0.04f, 0.04f, 1f);
    public Color buttonNormal = new Color(0.40f, 0.40f, 0.40f, 1f);
    public Color buttonHighlighted = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color buttonPressed = new Color(0.08f, 0.08f, 0.08f, 1f);

    Canvas canvas;
    GameObject guidePanel, bar;
    TMP_Text titleText, bodyText, pageText;
    Button backButton, nextButton;
    TMP_Text nextLabel;
    int page;
    InputAction reopenAction;

    void Start()
    {
        canvas = CreateCanvas(mode == Mode.MainMenu ? "Main Menu Canvas" : "Guide Canvas", new Vector2(1000, 620));
        PlaceInFront(canvas.transform, true);

        if (mode == Mode.MainMenu) BuildMainMenu();
        else BuildRoomGuide();
    }

    void OnEnable()
    {
        if (mode != Mode.Room) return;
        reopenAction = new InputAction("Open Guide", InputActionType.Button);
        reopenAction.AddBinding("<XRController>{LeftHand}/secondaryButton");
        reopenAction.AddBinding("<XRController>{LeftHand}/menu");
        reopenAction.performed += _ => OpenGuide(true);
        reopenAction.Enable();
    }

    void OnDisable()
    {
        reopenAction?.Disable();
        reopenAction?.Dispose();
        reopenAction = null;
    }

    // ---------- Main menu ----------

    void BuildMainMenu()
    {
        var panel = CreatePanel(canvas.transform, "Panel", new Vector2(1000, 620));
        CreateText(panel.transform, "Title", menuTitle, 66, FontStyles.Bold, new Vector2(0, 200), new Vector2(920, 110), true);
        CreateText(panel.transform, "Subtitle", menuSubtitle, 44, FontStyles.Normal, new Vector2(0, 95), new Vector2(920, 80));

        int n = Mathf.Max(1, sceneButtons.Count);
        float width = 380, gap = 60;
        float start = -((n - 1) * (width + gap)) / 2f;
        for (int i = 0; i < sceneButtons.Count; i++)
        {
            string scene = sceneButtons[i].sceneName;
            CreateButton(panel.transform, sceneButtons[i].label, new Vector2(start + i * (width + gap), -110),
                new Vector2(width, 150), 46, () => LoadScene(scene));
        }
    }

    // ---------- Room guide ----------

    void BuildRoomGuide()
    {
        guidePanel = CreatePanel(canvas.transform, "Guide", new Vector2(1000, 620));
        titleText = CreateText(guidePanel.transform, "Title", "", 62, FontStyles.Bold, new Vector2(0, 215), new Vector2(920, 100), true);
        bodyText = CreateText(guidePanel.transform, "Body", "", 42, FontStyles.Normal, new Vector2(0, 25), new Vector2(860, 260));
        pageText = CreateText(guidePanel.transform, "Page", "", 28, FontStyles.Normal, new Vector2(0, -130), new Vector2(300, 40));

        backButton = CreateButton(guidePanel.transform, "Back", new Vector2(-330, -225), new Vector2(240, 100), 40, () => ShowPage(page - 1));
        CreateButton(guidePanel.transform, "Main Menu", new Vector2(0, -225), new Vector2(300, 100), 40, () => LoadScene(mainMenuScene));
        nextButton = CreateButton(guidePanel.transform, "Next", new Vector2(330, -225), new Vector2(240, 100), 40, NextOrClose);
        nextLabel = nextButton.GetComponentInChildren<TMP_Text>();

        // Small bar that stays in the room after the guide is closed.
        bar = CreatePanel(canvas.transform, "Menu Bar", settingsPanel ? new Vector2(860, 130) : new Vector2(600, 130));
        bar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -230);
        float x = settingsPanel ? -280 : -145;
        CreateButton(bar.transform, "Guide", new Vector2(x, 0), new Vector2(250, 90), 38, () => OpenGuide(false));
        CreateButton(bar.transform, "Main Menu", new Vector2(x + 290, 0), new Vector2(270, 90), 38, () => LoadScene(mainMenuScene));
        if (settingsPanel)
            CreateButton(bar.transform, "Settings", new Vector2(x + 570, 0), new Vector2(250, 90), 38,
                () => settingsPanel.SetActive(!settingsPanel.activeSelf));
        bar.SetActive(false);

        ShowPage(0);
    }

    void ShowPage(int index)
    {
        if (pages.Count == 0) return;
        page = Mathf.Clamp(index, 0, pages.Count - 1);
        titleText.text = pages[page].title;
        bodyText.text = pages[page].body;
        pageText.text = (page + 1) + " / " + pages.Count;
        backButton.gameObject.SetActive(page > 0);
        nextLabel.text = page == pages.Count - 1 ? "Let's go!" : "Next";
    }

    void NextOrClose()
    {
        if (page < pages.Count - 1) { ShowPage(page + 1); return; }
        guidePanel.SetActive(false);
        bar.SetActive(true);
    }

    void OpenGuide(bool inFrontOfPlayer)
    {
        if (guidePanel == null) return;
        if (inFrontOfPlayer) PlaceInFront(canvas.transform, false);
        bar.SetActive(false);
        guidePanel.SetActive(true);
        ShowPage(0);
    }

    void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        SceneManager.LoadScene(sceneName);
    }

    // ---------- Placement ----------

    void PlaceInFront(Transform t, bool useOrigin)
    {
        Transform reference = null;
        if (!useOrigin && Camera.main) reference = Camera.main.transform;
        if (reference == null)
        {
            var origin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            reference = origin ? origin.transform : transform;
        }
        Vector3 forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up);
        if (forward.sqrMagnitude < 1e-4f) forward = Vector3.forward;
        forward.Normalize();
        Vector3 basePos = reference.position;
        t.position = new Vector3(basePos.x, 0, basePos.z) + forward * distance + Vector3.up * height;
        t.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    // ---------- UI building helpers ----------

    Canvas CreateCanvas(string name, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        go.AddComponent<GraphicRaycaster>();
        go.AddComponent<TrackedDeviceGraphicRaycaster>();
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.localScale = Vector3.one * 0.001f; // 1000 units = 1 metre
        return c;
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.color = panelColor;
        if (roundedSprite) { img.sprite = roundedSprite; img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = 0.25f; }
        return go;
    }

    TMP_Text CreateText(Transform parent, string name, string text, float size, FontStyles style, Vector2 pos, Vector2 box, bool singleLine = false)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (font) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = singleLine ? TextWrappingModes.NoWrap : TextWrappingModes.Normal;
        if (singleLine)
        {
            // Shrink long titles so they always fit on one line.
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = size;
            tmp.fontSizeMin = size * 0.5f;
        }
        tmp.raycastTarget = false;
        return tmp;
    }

    Button CreateButton(Transform parent, string label, Vector2 pos, Vector2 size, float fontSize, UnityAction onClick)
    {
        var go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.color = Color.white;
        if (roundedSprite) { img.sprite = roundedSprite; img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = 0.3f; }
        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = buttonNormal;
        colors.highlightedColor = buttonHighlighted;
        colors.pressedColor = buttonPressed;
        colors.selectedColor = buttonNormal;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);
        var text = CreateText(go.transform, "Label", label, fontSize, FontStyles.Bold, Vector2.zero, size - new Vector2(20, 0), true);
        text.color = Color.white;
        return btn;
    }
}
