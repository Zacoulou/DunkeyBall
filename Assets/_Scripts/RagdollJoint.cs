using System;
using UnityEngine;

//Struct used in the inspector to generate joints
[Serializable]
struct RagdollJointInspectorData {
    public Rigidbody rb;
    public CapsuleCollider capsuleCollider;
    public Transform hingeTransform;
    public Transform spriteParentTransform;
}



// Struct to organanize all important variables of each joint in the ragdoll
struct RagdollJoint {
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    //Hinge Components. Used to reset hinges to initial position / rotation
    private Transform hingeTransform;
    private Vector3 hingeInitialPos;
    private Vector3 hingeInitialEulerRot;

    //SpriteParent Components. Used to reset sprites to initial position / rotation
    private Transform spriteParentTransform;
    private Vector3 spriteParentInitialPos;
    private Vector3 spriteParentInitialEulerRot;

    public RagdollJoint(Rigidbody rigidBody, CapsuleCollider collider, Transform hingeT, Transform spriteParentT) {
        rb = rigidBody;
        capsuleCollider = collider;
        hingeTransform = hingeT;
        spriteParentTransform = spriteParentT;

        hingeInitialPos = Vector3.zero;
        hingeInitialEulerRot = Vector3.zero;
        spriteParentInitialPos = Vector3.zero;
        spriteParentInitialEulerRot = Vector3.zero;
    }

    public void Initialize() {
        hingeInitialPos = hingeTransform.localPosition;
        hingeInitialEulerRot = hingeTransform.localEulerAngles;

        spriteParentInitialPos = spriteParentTransform.localPosition;
        spriteParentInitialEulerRot = spriteParentTransform.localEulerAngles;
    }

    private void ResetPositionAndRotation() {
        hingeTransform.localPosition = hingeInitialPos;
        hingeTransform.localEulerAngles = hingeInitialEulerRot;

        spriteParentTransform.localPosition = spriteParentInitialPos;
        spriteParentTransform.localEulerAngles = spriteParentInitialEulerRot;
        spriteParentTransform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void EnableRagdoll(Vector3 currVel) {
        ResetPositionAndRotation();

        capsuleCollider.enabled = true;
        rb.isKinematic = false;
        rb.velocity = currVel;
    }

    public void DisableRagdoll() {
        ResetPositionAndRotation();

        capsuleCollider.enabled = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
    }
}