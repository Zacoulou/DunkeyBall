﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour {
    [SerializeField] PlayerController pController;
    [SerializeField] CapsuleCollider capsuleCollider;


    //Detect collisions between the GameObjects with Colliders attached
    void OnTriggerEnter(Collider collision) {
        //Check for a match with the specific tag on any GameObject that collides with your GameObject
        if (collision.gameObject.CompareTag("Ball")) {
            Ball ball = collision.gameObject.GetComponentInParent<Ball>();

            if (pController.GetCanObtainBall()) {
                pController.ObtainBall(ball);
            }
        }
    }

    public CapsuleCollider getCapsuleCollider() {
        return capsuleCollider;
    }

    public IEnumerator DisableCollisionForTime(Collider collider, float time) {
        Physics.IgnoreCollision(capsuleCollider, collider);
        yield return new WaitForSeconds(time);
        Physics.IgnoreCollision(capsuleCollider, collider, false);
    }

    public IEnumerator DisableCollisionForTime(Collider collider1, Collider collider2, float time) {
        Physics.IgnoreCollision(collider1, collider2);
        yield return new WaitForSeconds(time);
        Physics.IgnoreCollision(collider1, collider2, false);
    }
}
