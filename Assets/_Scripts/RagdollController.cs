using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour {
    [SerializeField] Animator animator;
    [SerializeField] private Transform torsoTransform;
    [SerializeField] private Transform torsoBoneTransform; //Used to measure angle of torso when getting up
    [SerializeField] private Rigidbody[] rbs;

    public bool RagdollActive { get; private set; }
    private Vector3 defaultTorsoScale;
    private Dictionary<Rigidbody, Vector3> initialPos = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, Quaternion> initialRot = new Dictionary<Rigidbody, Quaternion>();
    private float timeAtRagdoll = 0f;
    private float ragDollBufferTime = 0f;


    // Start is called before the first frame update
    void Awake() {
        //foreach (var rb in rbs) {
        //    initialPos.Add(rb, rb.transform.localPosition);
        //    initialRot.Add(rb, rb.transform.localRotation);
        //}
        //RecordTransform();
        defaultTorsoScale = torsoTransform.localScale;
        DisableRagdoll();
    }

    //void RecordTransform() {
    //    foreach (var rb in rbs) {
    //        initialPos[rb] = rb.transform.localPosition;
    //        initialRot[rb] = rb.transform.localRotation;
    //    }
    //}

    public void ActivateRagdoll(Vector2 currVel, float duration) {
        //Resets torsoScale to default so scaling from animations does not conflict with transforms
        torsoTransform.localScale = defaultTorsoScale;

        RagdollActive = true;
        animator.enabled = false;
        ragDollBufferTime = duration;

        foreach (var rb in rbs) {
            rb.gameObject.GetComponent<CapsuleCollider>().enabled = true;
            rb.isKinematic = false;
            rb.velocity = currVel;
        }

        timeAtRagdoll = Time.realtimeSinceStartup;
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
        return torsoBoneTransform.localEulerAngles.z * Mathf.Rad2Deg;
    }

    public bool CheckRagDollBuffer() {
        return Time.realtimeSinceStartup - timeAtRagdoll >= ragDollBufferTime;
    }

}