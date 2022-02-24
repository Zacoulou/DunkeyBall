using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody2D rb;
    public CircleCollider2D inRangeCollider;
    public CircleCollider2D physicsCollider;
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

    public void SetAngularVelocity(float angularVel) {
        rb.angularVelocity = angularVel;
    }

    public void SetPlayerWithPossesion(GameObject player) {
        playerWithPossesion = player;
        ApplyForce(0.0f, 0.0f);
        SetAngularVelocity(0.0f);

        if (player != null) {
            SetDeadBall();
        }
    }

    public void SetGravityScale(bool state) {
        if (state)
            rb.gravityScale = gravScale;
        else
            rb.gravityScale = 0;
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
