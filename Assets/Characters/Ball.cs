using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody rb;
    public SphereCollider inRangeCollider;
    public SphereCollider physicsCollider;
    public SpriteRenderer spriteRenderer;
    GameObject playerWithPossesion = null;
    float gravScale = 1;
    bool deadBall = false;

    public void ApplyForce(float xVelocity, float yVelocity) {
        rb.velocity = new Vector2(xVelocity, yVelocity);
    }

    public void PositionBall(float xPos, float yPos) {
        this.transform.position = new Vector2(xPos, yPos);
    }

    public void SetAngularVelocity(Vector3 angularVel) {
        rb.angularVelocity = angularVel;
    }

    public void SetPlayerWithPossesion(GameObject player) {
        playerWithPossesion = player;
        ApplyForce(0.0f, 0.0f);
        SetAngularVelocity(new Vector3(0.0f, 0.0f, 0.0f));

        if (player != null) {
            SetDeadBall();
        }
    }

    public void SetGravityScale(bool state) {
        rb.useGravity = state;
    }

    public void SetDeadBall() {
        deadBall = true;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            SetDeadBall();
        }
    }
}
