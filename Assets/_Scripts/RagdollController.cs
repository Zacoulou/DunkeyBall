using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour {
    [SerializeField] Animator animator;
    public bool RagdollActive { get; private set; }

    public Transform torsoTransform;
    private Vector3 defaultTorsoScale;

    private HingeJoint[] joints;
    [SerializeField] private Rigidbody[] rbs;

    private Dictionary<Rigidbody, Vector3> initialPos = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, Quaternion> initialRot = new Dictionary<Rigidbody, Quaternion>();

    // Start is called before the first frame update
    void Awake() {
        joints = GetComponentsInChildren<HingeJoint>();

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

    public void ActivateRagdoll(Vector2 currVel, MovementController3D.FacingDirection facingDirection) {
        //Resets torsoScale to default so scaling from animations does not conflict with transforms
        torsoTransform.localScale = defaultTorsoScale;
        
        RagdollActive = true;
        animator.enabled = false;

        foreach (var rb in rbs) {
            rb.gameObject.GetComponent<CapsuleCollider>().enabled = true;
            rb.isKinematic = false;
            rb.velocity = currVel;
        }

    }

    public void DisableRagdoll() {
        RagdollActive = false;
        animator.enabled = true;

        foreach (var rb in rbs) {
            rb.gameObject.GetComponent<CapsuleCollider>().enabled = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public float GetRagdollRot() {
        return rbs[0].transform.localRotation.z;
    }

}