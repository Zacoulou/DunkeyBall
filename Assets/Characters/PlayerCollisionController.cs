using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour {
    [SerializeField] PlayerController pController;
    [SerializeField] BoxCollider2D boxCollider2D;


    //Detect collisions between the GameObjects with Colliders attached
    void OnTriggerEnter2D(Collider2D collision) {
        //Check for a match with the specific tag on any GameObject that collides with your GameObject
        if (collision.gameObject.CompareTag("Ball")) {
            Ball ball = collision.gameObject.GetComponentInParent<Ball>();

            if (pController.GetCanObtainBall()) {
                pController.ObtainBall(ball);
            }

        }
    }

    public BoxCollider2D getBoxCollider2D() {
        return boxCollider2D;
    }

    public IEnumerator DisableCollisionForTime(Collider2D collider, float time) {
        Physics2D.IgnoreCollision(boxCollider2D, collider);
        yield return new WaitForSeconds(time);
        Physics2D.IgnoreCollision(boxCollider2D, collider, false);
    }

    public IEnumerator DisableCollisionForTime(Collider2D collider1, Collider2D collider2, float time) {
        Physics2D.IgnoreCollision(collider1, collider2);
        yield return new WaitForSeconds(time);
        Physics2D.IgnoreCollision(collider1, collider2, false);
    }
}
