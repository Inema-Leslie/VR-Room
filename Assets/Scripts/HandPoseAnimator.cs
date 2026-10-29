using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Animates the SimpleHands rigged hand from the controller's grip and trigger.
/// Grip  -> middle, ring and little fingers (and thumb) close into a fist.
/// Trigger -> index finger curls (and thumb pinches in a little).
/// The finger bones are found by name, so no Animator / clips are needed.
/// </summary>
public class HandPoseAnimator : MonoBehaviour
{
    public enum Side { Left, Right }

    [Header("Which controller drives this hand")]
    public Side side = Side.Right;

    [Tooltip("Optional. If empty, bindings to the XR controller's grip/trigger are made automatically.")]
    public InputActionReference gripAction;
    public InputActionReference triggerAction;

    [Header("Hand model (the WhiteHand / BlackHand instance)")]
    public Transform handModel;

    [Header("Feel")]
    [Tooltip("How fast fingers follow the buttons.")]
    public float speed = 18f;
    [Tooltip("Flip if the fingers bend backwards.")]
    public bool invertCurl = false;
    public Vector3 curlAxis = Vector3.right;

    [Header("Max bend in degrees (base, middle, tip)")]
    public Vector3 fingerCurl = new Vector3(70f, 95f, 65f);
    public Vector3 indexCurl  = new Vector3(65f, 85f, 55f);
    public Vector2 thumbCurl  = new Vector2(30f, 45f);
    [Tooltip("Thumb swings toward the index finger when gripping.")]
    public float thumbTuck = -25f;
    [Tooltip("Index bends a little when you only grip, so the fist looks natural.")]
    [Range(0, 1)] public float indexWithGrip = 0.3f;

    [Header("Testing without a headset (Play mode)")]
    public bool useTestValues = false;
    [Range(0, 1)] public float testGrip = 0f;
    [Range(0, 1)] public float testTrigger = 0f;

    [Header("Read-only")]
    [SerializeField] float grip;
    [SerializeField] float trigger;

    class Bone { public Transform t; public Quaternion rest; }
    Bone[] index, middle, ring, little, thumb;
    InputAction autoGrip, autoTrigger;

    void Awake()
    {
        if (handModel == null && transform.childCount > 0) handModel = transform.GetChild(0);
        if (handModel == null) { enabled = false; return; }

        // SimpleHands ships with an Animator whose controller is missing; turn it off
        // so it can never fight with this script.
        foreach (var a in handModel.GetComponentsInChildren<Animator>(true)) a.enabled = false;

        index  = Chain("IndexFinger", "Index2", "Index3");
        middle = Chain("MiddleFinger", "Middle2", "Middle3");
        ring   = Chain("RingFinger", "Ring2", "Ring3");
        little = Chain("LittleFinger", "Little2", "Little3");
        thumb  = Chain("Thumb", "Thumb2");

        string hand = side == Side.Left ? "{LeftHand}" : "{RightHand}";
        autoGrip = new InputAction("Grip", InputActionType.Value, expectedControlType: "Axis");
        autoGrip.AddBinding("<XRController>" + hand + "/grip");
        autoGrip.AddBinding("<XRController>" + hand + "/{Grip}");
        autoTrigger = new InputAction("Trigger", InputActionType.Value, expectedControlType: "Axis");
        autoTrigger.AddBinding("<XRController>" + hand + "/trigger");
        autoTrigger.AddBinding("<XRController>" + hand + "/{Trigger}");
    }

    Bone[] Chain(params string[] names)
    {
        var bones = new Bone[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            Transform t = Find(handModel, names[i]);
            bones[i] = new Bone { t = t, rest = t ? t.localRotation : Quaternion.identity };
        }
        return bones;
    }

    static Transform Find(Transform root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    void OnEnable()
    {
        autoGrip?.Enable(); autoTrigger?.Enable();
        if (gripAction) gripAction.action.Enable();
        if (triggerAction) triggerAction.action.Enable();
    }

    void OnDisable() { autoGrip?.Disable(); autoTrigger?.Disable(); }

    void OnDestroy() { autoGrip?.Dispose(); autoTrigger?.Dispose(); }

    float Read(InputActionReference r, InputAction fallback)
    {
        if (r != null && r.action != null) return r.action.ReadValue<float>();
        return fallback != null ? fallback.ReadValue<float>() : 0f;
    }

    void Update()
    {
        float g = useTestValues ? testGrip : Read(gripAction, autoGrip);
        float tr = useTestValues ? testTrigger : Read(triggerAction, autoTrigger);
        float k = 1f - Mathf.Exp(-speed * Time.deltaTime);
        grip = Mathf.Lerp(grip, Mathf.Clamp01(g), k);
        trigger = Mathf.Lerp(trigger, Mathf.Clamp01(tr), k);

        float idx = Mathf.Max(trigger, grip * indexWithGrip);
        Curl(index, indexCurl, idx);
        Curl(middle, fingerCurl, grip);
        Curl(ring, fingerCurl, grip);
        Curl(little, fingerCurl, grip);

        // Thumb: closes with grip, pinches slightly with trigger.
        float th = Mathf.Max(grip, trigger * 0.6f);
        float s = invertCurl ? -1f : 1f;
        if (thumb[0].t)
            thumb[0].t.localRotation = thumb[0].rest
                * Quaternion.AngleAxis(thumbTuck * th, Vector3.forward)
                * Quaternion.AngleAxis(s * thumbCurl.x * th, curlAxis);
        if (thumb[1].t)
            thumb[1].t.localRotation = thumb[1].rest * Quaternion.AngleAxis(s * thumbCurl.y * th, curlAxis);
    }

    void Curl(Bone[] chain, Vector3 max, float amount)
    {
        float s = invertCurl ? -1f : 1f;
        for (int i = 0; i < chain.Length; i++)
        {
            if (chain[i].t == null) continue;
            chain[i].t.localRotation = chain[i].rest * Quaternion.AngleAxis(s * max[i] * amount, curlAxis);
        }
    }
}
