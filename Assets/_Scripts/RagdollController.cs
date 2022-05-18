using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour {
    [SerializeField] PlayerController pController;              //Reference to PlayerController
    [SerializeField] Animator animator;                         //Reference to Animator
    [SerializeField] private Transform torsoTransform;          //Transform of the Torso. Used for scaling and centering
    [SerializeField] private Transform torsoBoneTransform;      //Used to measure angle of torso when getting up
    [SerializeField] private Rigidbody[] rbs;                   //Array of all rigid bodies used in ragdoll
    [SerializeField] private Rigidbody rbCenter;                //Rigidbody used to position player where the ragdoll ended up
    private Dictionary<Rigidbody, CapsuleCollider> rbCapsuleDict = new Dictionary<Rigidbody, CapsuleCollider>(); //Dictionary for referencing capsule colliders
    private Dictionary<Rigidbody, Transform> rbTransformDict = new Dictionary<Rigidbody, Transform>(); //Dictionary for referencing joint transforms
    private Dictionary<Rigidbody, Vector3> initialJointPositions = new Dictionary<Rigidbody, Vector3>(); //Dictionary for referencing initial joint positions

    public bool RagdollActive { get; private set; }
    private Vector3 defaultTorsoScale;
    private float timeAtRagdoll = 0f;
    private float ragDollBufferTime = 0f;


    // Start is called before the first frame update
    void Awake() {
        defaultTorsoScale = torsoTransform.localScale;
        DisableRagdoll();
        InitializeDictionaries();
    }

    void InitializeDictionaries() {
        foreach (var rb in rbs) {
            rbCapsuleDict.Add(rb, rb.gameObject.GetComponent<CapsuleCollider>());
            rbTransformDict.Add(rb, rb.transform);
            initialJointPositions.Add(rb, rb.transform.localPosition);

            Debug.Log(rb.name + "  " + rb.transform.localPosition);
        }
    }

    public void ActivateRagdoll(Vector2 currVel, float duration) {
        //Resets torsoScale to default so scaling from animations does not conflict with transforms
        torsoTransform.localScale = defaultTorsoScale;

        RagdollActive = true;
        animator.enabled = false;
        ragDollBufferTime = duration;

        foreach (var rb in rbs) {
            //Position joints to initial locations
            Transform jointTransform = rbTransformDict[rb];
            Vector3 initialPos = initialJointPositions[rb];
            jointTransform.localPosition = initialPos;

            //Set ragdoll components to use physics
            CapsuleCollider capsule = rbCapsuleDict[rb];
            capsule.enabled = true;
            rb.isKinematic = false;
            rb.velocity = currVel;
        }

        timeAtRagdoll = Time.realtimeSinceStartup;
    }

    public void DisableRagdoll() {
        RagdollActive = false;
        animator.enabled = true;

        //Position player to where the ragdoll landed
        pController.SetPosition(rbCenter.position);

        //Set all ragdoll components back to not using physics
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