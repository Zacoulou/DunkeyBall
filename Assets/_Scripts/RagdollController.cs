using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour {
    [SerializeField] PlayerController pController;              //Reference to PlayerController
    [SerializeField] Animator animator;                         //Reference to Animator
    [SerializeField] private Transform centerBoneTransform;     //Used for scaling and centering
    [SerializeField] private RagdollJointInspectorData ragdollTailJoint;            //Tail ragdoll joints used assemble rig in inspector
    [SerializeField] private RagdollJointInspectorData[] ragdollJointInspectorInfo; //Array of all ragdoll joints used assemble rig in inspector

    private List<RagdollJoint> ragdollJoints = new List<RagdollJoint>();            //Array of all ragdoll joints used in the rig
    public bool RagdollActive { get; private set; }
    private Vector3 defaultTorsoScale;
    private float timeAtRagdoll = 0f;
    private float ragDollBufferTime = 0f;


    public void InitializeRagdollJoints(bool hasTail) {
        defaultTorsoScale = centerBoneTransform.localScale;

        //Initialize all joints
        foreach (RagdollJointInspectorData j in ragdollJointInspectorInfo) {
            RagdollJoint joint = new RagdollJoint(j.rb, j.capsuleCollider, j.hingeTransform, j.spriteParentTransform);
            joint.Initialize();
            ragdollJoints.Add(joint);
        }

        //Add tail to ragdoll if it is present
        if (hasTail) {
            RagdollJoint joint = new RagdollJoint(ragdollTailJoint.rb, ragdollTailJoint.capsuleCollider, ragdollTailJoint.hingeTransform, ragdollTailJoint.spriteParentTransform);
            joint.Initialize();
            ragdollJoints.Add(joint);
        }

        DisableRagdoll();
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

    public float GetWrappedRagdollRot() {
        return WrapAngle(centerBoneTransform.localEulerAngles.z);
    }

    public RagdollOrientation GetRagdollOrientation() {
        RagdollOrientation orientation = RagdollOrientation.STRAIGHT_UP;
        
        if (GetWrappedRagdollRot() < -45) {
            orientation = RagdollOrientation.LEANING_FORWARD;
        } else if (GetWrappedRagdollRot() > 45) {
            orientation = RagdollOrientation.LEANING_BACKWARD;
        }

        return orientation;
    }

    //Wrap angle between -180 and 180 degrees
    private static float WrapAngle(float angle) {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    public bool CheckRagDollBuffer() {
        return Time.realtimeSinceStartup - timeAtRagdoll >= ragDollBufferTime;
    }

}

public enum RagdollOrientation {
    STRAIGHT_UP,
    LEANING_FORWARD,
    LEANING_BACKWARD
}