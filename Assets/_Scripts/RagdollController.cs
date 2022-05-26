using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour {
    [SerializeField] PlayerController pController;              //Reference to PlayerController
    [SerializeField] Animator animator;                         //Reference to Animator
    [SerializeField] private Transform centerBoneTransform;     //Used to measure angle of torso when getting up and for scaling and centering

    [SerializeField] private RagdollJoint[] ragdollJoints;      //Array of all ragdoll joints used in the rig


    public bool RagdollActive { get; private set; }
    private Vector3 defaultTorsoScale;
    private float timeAtRagdoll = 0f;
    private float ragDollBufferTime = 0f;


    // Start is called before the first frame update
    void Awake() {
        defaultTorsoScale = centerBoneTransform.localScale;
        InitializeDictionaries();
        //DisableRagdoll();
    }

    void InitializeDictionaries() {
        foreach (RagdollJoint joint in ragdollJoints) {
            joint.Initialize();
        }
    }

    public void ActivateRagdoll(Vector3 currVel, float duration) {
        //Resets torsoScale to default so scaling from animations does not conflict with transforms
        centerBoneTransform.localScale = defaultTorsoScale;

        RagdollActive = true;
        animator.enabled = false;
        ragDollBufferTime = duration;

        foreach (RagdollJoint joint in ragdollJoints) {
            joint.EnableRagdoll(currVel);
        }

        timeAtRagdoll = Time.realtimeSinceStartup;
    }

    public void DisableRagdoll() {
        RagdollActive = false;
        animator.enabled = true;

        //Position player to where the ragdoll landed
        pController.SetPosition(centerBoneTransform.position);

        //Set all ragdoll components back to not using physics
        foreach (RagdollJoint joint in ragdollJoints) {
            joint.DisableRagdoll();
        }
    }

    public float GetRagdollRot() {
        return centerBoneTransform.localEulerAngles.z * Mathf.Rad2Deg;
    }

    public bool CheckRagDollBuffer() {
        return Time.realtimeSinceStartup - timeAtRagdoll >= ragDollBufferTime;
    }

}