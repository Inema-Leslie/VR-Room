using UnityEngine;

/// <summary>Editor test helper: add to the door in Play mode to fake a hand pulling the handle.</summary>
public class DoorTester : MonoBehaviour
{
    public Vector3 pullDirection = new Vector3(0, 0, -1);
    public float speed = 0.4f;
    public float duration = 3f;
    Transform fake; float t;

    void Start()
    {
        var door = GetComponent<HingeDoorGrab>();
        var body = GetComponent<Rigidbody>();
        body.isKinematic = false;
        var handle = transform.Find("Handle");
        fake = new GameObject("Fake Hand").transform;
        fake.position = handle ? handle.position + new Vector3(0, 0, -0.3f) : transform.position;
        door.StartFollowing(fake);
        Debug.Log("[DoorTester] start door pos " + transform.position + " rot " + transform.eulerAngles);
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;
        if (t < duration) fake.position += pullDirection.normalized * speed * Time.fixedDeltaTime;
        if (Mathf.Abs(t - Mathf.Round(t * 4) / 4) < Time.fixedDeltaTime * 0.5f)
            Debug.Log($"[DoorTester] t={t:F2} doorPos={transform.position} yaw={transform.eulerAngles.y:F1} hand={fake.position}");
    }
}
