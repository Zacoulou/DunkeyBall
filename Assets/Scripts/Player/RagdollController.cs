using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour {
    [SerializeField] Animator animator;
    public bool RagdollActive { get; private set; }

    public Transform torsoTransform;
    private Vector3 defaultTorsoScale;

    private HingeJoint2D[] joints;
    [SerializeField] private Rigidbody2D[] rbs;

    private Dictionary<Rigidbody2D, Vector3> initialPos = new Dictionary<Rigidbody2D, Vector3>();
    private Dictionary<Rigidbody2D, Quaternion> initialRot = new Dictionary<Rigidbody2D, Quaternion>();

    // Start is called before the first frame update
    void Awake() {
        joints = GetComponentsInChildren<HingeJoint2D>();

        foreach (var rb in rbs) {
            initialPos.Add(rb, rb.transform.localPosition);
            initialRot.Add(rb, rb.transform.localRotation);
        }
        RecordTransform();
        defaultTorsoScale = torsoTransform.localScale;
        DisableRagdoll();
    }

    void RecordTransform() {
        foreach (var rb in rbs) {
            initialPos[rb] = rb.transform.localPosition;
            initialRot[rb] = rb.transform.localRotation;
        }
    }

    public void ActivateRagdoll(Vector2 currVel, string facingDirection = "right") {
        //Resets torsoScale to default so scaling from animations does not conflict with transforms
        torsoTransform.localScale = defaultTorsoScale;

        if (currVel.x <= 2f && currVel.x >= -2f) {
            if (facingDirection.Equals("right"))
                currVel = new Vector2(-2f, currVel.y);
            else
                currVel = new Vector2(2f, currVel.y);
        }
        
        RagdollActive = true;
        animator.enabled = false;

        foreach (var rb in rbs) {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.velocity = currVel;
        }

        foreach (var joint in joints) {
            joint.enabled = true;
        }
    }

    public void DisableRagdoll() {
        RagdollActive = false;
        animator.enabled = true;

        foreach (var rb in rbs) {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        foreach (var joint in joints) {
            joint.enabled = false;
        }
    }

    public float GetRagdollRot() {
        return rbs[0].transform.localRotation.z;
    }

}