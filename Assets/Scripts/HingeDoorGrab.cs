using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Makes a physics hinge door follow your hand like a real door.
/// The XR Grab Interactable is only used to detect the grab; this script turns the
/// door around its Hinge Joint by setting the Rigidbody's angular velocity toward
/// the hand, so the joint (and its limits) keep the door on its hinge.
/// No animation is used.
/// </summary>
[RequireComponent(typeof(HingeJoint), typeof(Rigidbody))]
public class HingeDoorGrab : MonoBehaviour
{
    public XRGrabInteractable grab;
    [Tooltip("How strongly the door follows the hand.")]
    public float followStrength = 6f;
    [Tooltip("Fastest the door may turn, in degrees per second.")]
    public float maxTurnSpeed = 90f;

    HingeJoint hinge;
    Rigidbody body;
    Transform hand;
    Vector3 grabPointLocal;
    float angleOffset;

    void Awake()
    {
        hinge = GetComponent<HingeJoint>();
        body = GetComponent<Rigidbody>();
        if (grab == null) grab = GetComponent<XRGrabInteractable>();

        // The grab only tells us who is holding the door; this script does the moving.
        grab.trackPosition = false;
        grab.trackRotation = false;
        grab.trackScale = false;
        grab.throwOnDetach = false;
        grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDestroy()
    {
        if (grab == null) return;
        grab.selectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    Vector3 Axis => transform.TransformDirection(hinge.axis).normalized;
    Vector3 Pivot => transform.TransformPoint(hinge.anchor);

    float AngleToHand()
    {
        Vector3 axis = Axis, pivot = Pivot;
        Vector3 door = Vector3.ProjectOnPlane(transform.TransformPoint(grabPointLocal) - pivot, axis);
        Vector3 toHand = Vector3.ProjectOnPlane(hand.position - pivot, axis);
        if (door.sqrMagnitude < 1e-4f || toHand.sqrMagnitude < 1e-4f) return 0f;
        return Vector3.SignedAngle(door, toHand, axis);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Use the controller itself (not its attach point) so the target never moves with the door.
        StartFollowing(args.interactorObject.transform);
    }

    /// <summary>Start turning the door toward this transform (also used for testing in the editor).</summary>
    public void StartFollowing(Transform target)
    {
        hand = target;
        // Remember where on the door we grabbed (the handle), in door space.
        grabPointLocal = transform.InverseTransformPoint(hand.position);
        // Keep the door still at the moment of grabbing, even if the hand isn't exactly on the handle.
        angleOffset = AngleToHand();
    }

    void OnRelease(SelectExitEventArgs args)
    {
        hand = null;
        if (!body.isKinematic)
        {
            body.angularVelocity = Vector3.zero;
            body.linearVelocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (hand == null || body.isKinematic) return;

        float delta = Mathf.DeltaAngle(angleOffset, AngleToHand());
        float speed = Mathf.Clamp(delta * followStrength, -maxTurnSpeed, maxTurnSpeed);
        body.angularVelocity = Axis * (speed * Mathf.Deg2Rad);
        // Only rotation is wanted: give the body exactly the linear velocity a rotation
        // about the hinge produces, so nothing can drag it off the hinge.
        body.linearVelocity = Vector3.Cross(body.angularVelocity, body.worldCenterOfMass - Pivot);
    }
}
