using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody rb;
    public SphereCollider inRangeCollider;
    public SphereCollider physicsCollider;
    public SpriteRenderer spriteRenderer;
    [SerializeField] private Transform spriteTransform;
    GameObject playerWithPossesion = null;
    bool deadBall = false;

    private void FixedUpdate() {
        RotateToFaceMovementDirection();
    }

    public void SetVelocity(Vector3 velocityVector) {
        rb.velocity = velocityVector;
    }

    public void PositionBall(Vector3 transform) {
        this.transform.position = transform;
    }

    public void SetAngularVelocity(Vector3 angularVel) {
        rb.angularVelocity = angularVel;
    }

    public void SetPlayerWithPossesion(GameObject player) {
        playerWithPossesion = player;
        SetVelocity(new Vector3(0.0f, 0.0f, 0.0f));
        SetAngularVelocity(new Vector3(0.0f, 0.0f, 0.0f));

        if (player != null) {
            SetDeadBall();
        }
    }

    public void SetUsingGravity(bool state) {
        rb.useGravity = state;
    }

    public void SetDeadBall() {
        deadBall = true;
    }

    void RotateToFaceMovementDirection() {
        float rotateTime = 0.05f;
        float minRotation = -15f;
        float maxRotation = 15f;

        spriteTransform.eulerAngles = new Vector3(0f, 0f, spriteTransform.eulerAngles.z);

        float rotTarget = 0f;
        if (rb.velocity.x != 0f || rb.velocity.z != 0f) {
            rotTarget = Mathf.Clamp(Mathf.Atan(rb.velocity.z / rb.velocity.x) * -Mathf.Rad2Deg, minRotation, maxRotation);
        }

        spriteTransform.LeanRotateY(rotTarget, rotateTime);

    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            Debug.Log("DeadBall");
            SetDeadBall();
        }
    }
}
