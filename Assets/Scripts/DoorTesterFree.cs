using UnityEngine;

/// <summary>Editor test helper: makes the door dynamic without any hand, and logs where it goes.</summary>
public class DoorTesterFree : MonoBehaviour
{
    float t;
    void Start()
    {
        var body = GetComponent<Rigidbody>();
        body.isKinematic = false;
        Debug.Log($"[DoorFree] start pos {transform.position} com {body.worldCenterOfMass} inertia {body.inertiaTensor} constraints {body.constraints} excl {body.excludeLayers.value}");
        foreach (var c in GetComponentsInChildren<Collider>()) Debug.Log($"[DoorFree] collider {c.name} {c.GetType().Name} trigger={c.isTrigger} layer={c.gameObject.layer} bounds={c.bounds}");
        var hits = Physics.OverlapBox(body.worldCenterOfMass, new Vector3(0.7f, 1.5f, 0.1f), transform.rotation, ~0, QueryTriggerInteraction.Ignore);
        foreach (var h in hits) Debug.Log($"[DoorFree] overlapping {h.name} layer={h.gameObject.layer} attached={h.attachedRigidbody}");
    }
    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;
        if (t < 1.5f && Mathf.Abs(t - Mathf.Round(t * 10) / 10) < Time.fixedDeltaTime * 0.5f)
            Debug.Log($"[DoorFree] t={t:F2} pos={transform.position} yaw={transform.eulerAngles.y:F1}");
    }
}
