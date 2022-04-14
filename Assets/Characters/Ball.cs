using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody rb;
    public SphereCollider inRangeCollider;
    public SphereCollider physicsCollider;
    public SpriteRenderer spriteRenderer;
    GameObject playerWithPossesion = null;
    float gravScale = 0.1f;
    bool deadBall = false;

    public void ApplyForce(Vector3 velocityVector) {
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
        ApplyForce(new Vector3(0.0f, 0.0f, 0.0f));
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

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            Debug.Log("DeadBall");
            SetDeadBall();
        }
    }
}
