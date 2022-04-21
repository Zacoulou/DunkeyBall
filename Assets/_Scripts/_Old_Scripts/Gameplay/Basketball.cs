using System;
using UnityEngine;

public class Basketball : MonoBehaviour {
    public Rigidbody2D rb;
    public CircleCollider2D inRangeCollider;
    public CircleCollider2D physicsCollider;
    [System.NonSerialized] public GameObject playerWithPossesion = null;

    [System.NonSerialized] public float gravScale = 1;
    public bool shootingState = false;

    private MultipleTargetCamera MovingCam;

    void Start() {
        MovingCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MultipleTargetCamera>();
    }

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

        if (player != null) {
            this.GetComponent<SpriteRenderer>().sortingLayerName = "Players";
            this.GetComponent<SpriteRenderer>().sortingOrder = 4 + (playerWithPossesion.GetComponent<PlayerMovement>().playerInputIndex * 5);
            SetDeadBall();
        } else {
            this.GetComponent<SpriteRenderer>().sortingLayerName = "BackBoard";
            this.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }

    }

    public void SetGravity(bool state) {
        if (state)
            rb.gravityScale = gravScale;
        else
            rb.gravityScale = 0;
    }

    public void SetDeadBall() {
        shootingState = false;
        MovingCam.ResetTargetsToDefault();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Ground")) {
            //shootingState = false;
            SetDeadBall();
        }
        //else if (collision.gameObject.CompareTag("Hoop")) {
        //    Debug.Log("Hit Rim");
        //    MovingCam.RemoveTarget(collision.gameObject.transform);
        //}
        //else if (collision.gameObject.CompareTag("Wall")) {
        //    Debug.Log("Hit Wall");
        //}
    }
}
