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
        float zeroSpeedDeadzone = 0.01f;
        float rotateTime = 20f;
        float minRotation = -15f;
        float maxRotation = 15f;

        float rotTarget = 0f;

        if (!(rb.velocity.x >= -zeroSpeedDeadzone && rb.velocity.x <= zeroSpeedDeadzone)
            && !(rb.velocity.z >= -zeroSpeedDeadzone && rb.velocity.z <= zeroSpeedDeadzone))
        {
            rotTarget = Mathf.Clamp(Mathf.Atan(rb.velocity.z / rb.velocity.x) * -Mathf.Rad2Deg, minRotation, maxRotation);
        }

        float y = CustomMath.LerpThrough360Degrees(spriteTransform.eulerAngles.y, rotTarget, Time.deltaTime * rotateTime);
        spriteTransform.eulerAngles = new Vector3(0f, y, spriteTransform.eulerAngles.z);

    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            Debug.Log("DeadBall");
            SetDeadBall();
        }
    }
}
