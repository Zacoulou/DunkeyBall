using System;
using UnityEngine;


// Struct to organanize all important variables of each joint in the ragdoll
[Serializable]
struct RagdollJoint {
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;

    //Hinge Components. Used to reset hinges to initial position / rotation
    [SerializeField] private Transform hingeTransform;
    private Vector3 hingeInitialPos;
    private Vector3 hingeInitialEulerRot;

    //SpriteParent Components. Used to reset sprites to initial position / rotation
    [SerializeField] private Transform spriteParentTransform;
    private Vector3 spriteParentInitialPos;
    private Vector3 spriteParentInitialEulerRot;

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