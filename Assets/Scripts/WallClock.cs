using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Drives an analog wall clock from the computer's / headset's system time
/// (DateTime.Now) and shows the same time as a digital readout.
/// Put this on a parent object that contains the Clock_Analog prefab.
/// The hands are found automatically by name ("Hour", "Minute", "Second").
/// </summary>
public class WallClock : MonoBehaviour
{
    [Header("Clock hands (auto-found by name if left empty)")]
    public Transform hourHand;
    public Transform minuteHand;
    public Transform secondHand;

    [Header("Rotation")]
    [Tooltip("Axis the hands spin around, in the hands' parent space. For the Course Library clock this is X (the face normal).")]
    public Vector3 rotationAxis = Vector3.right;
    [Tooltip("Tick this if the hands turn anticlockwise.")]
    public bool reverseDirection = false;
    [Tooltip("Extra degrees added to every hand, in case 12 o'clock is not at 0.")]
    public float angleOffset = 0f;
    [Tooltip("Second hand sweeps smoothly instead of ticking once per second.")]
    public bool smoothSeconds = false;

    [Header("Digital readout")]
    public TMP_Text digitalText;
    public bool use24Hour = true;
    public bool showDate = true;
    [Tooltip("If no text is assigned, one is created under the clock when the scene starts.")]
    public bool createReadoutIfMissing = true;
    public Vector3 readoutLocalPosition = new Vector3(0.02f, -0.07f, 0f);
    public Vector3 readoutLocalEuler = new Vector3(0f, -90f, 0f);
    public float readoutFontSize = 0.42f;
    public Color readoutColor = new Color(0.95f, 0.95f, 0.92f);
    public Color readoutBackground = new Color(0.08f, 0.09f, 0.11f, 0.85f);

    Quaternion hourStart, minuteStart, secondStart;
    bool initialised;

    void OnEnable()
    {
        initialised = false;
        Init();
        if (digitalText == null && createReadoutIfMissing) CreateReadout();
    }

    void CreateReadout()
    {
        Transform clockRoot = hourHand ? hourHand.parent : transform;
        var go = new GameObject("Digital Readout");
        go.transform.SetParent(clockRoot, false);
        go.transform.localPosition = readoutLocalPosition;
        go.transform.localRotation = Quaternion.Euler(readoutLocalEuler);

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.rectTransform.sizeDelta = new Vector2(0.5f, 0.12f);
        tmp.fontSize = readoutFontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Top;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.color = readoutColor;
        tmp.characterSpacing = 4f;
        digitalText = tmp;
    }

    void Init()
    {
        if (hourHand == null)   hourHand   = FindChild("Hour");
        if (minuteHand == null) minuteHand = FindChild("Minute");
        if (secondHand == null) secondHand = FindChild("Second");

        // The hands' rotation in the prefab is "pointing at 12".
        if (hourHand)   hourStart   = hourHand.localRotation;
        if (minuteHand) minuteStart = minuteHand.localRotation;
        if (secondHand) secondStart = secondHand.localRotation;
        initialised = true;
    }

    Transform FindChild(string part)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t != transform && t.name.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0)
                return t;
        return null;
    }

    void Update()
    {
        if (!initialised) Init();

        DateTime now = DateTime.Now;
        float seconds = now.Second + (smoothSeconds ? now.Millisecond / 1000f : 0f);
        float minutes = now.Minute + now.Second / 60f;
        float hours   = (now.Hour % 12) + now.Minute / 60f;

        SetHand(hourHand,   hourStart,   hours   * 30f); // 360 / 12
        SetHand(minuteHand, minuteStart, minutes * 6f);  // 360 / 60
        SetHand(secondHand, secondStart, seconds * 6f);

        if (digitalText != null)
        {
            string time = now.ToString(use24Hour ? "HH:mm:ss" : "hh:mm:ss tt");
            string body = showDate
                ? time + "\n<size=50%>" + now.ToString("ddd d MMM yyyy").ToUpper() + "</size>"
                : time;
            string bg = ColorUtility.ToHtmlStringRGBA(readoutBackground);
            digitalText.text = "<mark=#" + bg + "> " + body.Replace("\n", " \n ") + " </mark>";
        }
    }

    void SetHand(Transform hand, Quaternion start, float angle)
    {
        if (hand == null) return;
        float sign = reverseDirection ? -1f : 1f;
        hand.localRotation = Quaternion.AngleAxis(sign * (angle + angleOffset), rotationAxis) * start;
    }
}
