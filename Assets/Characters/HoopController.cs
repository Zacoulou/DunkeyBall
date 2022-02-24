using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopController : MonoBehaviour
{
    public Transform centerBasket;
    public BoxCollider2D ballEntry;
    public BoxCollider2D ballExit;

    bool hasEntered = false;
    bool hasExit = false;

    float timeAtLastCheck = 0.0f;
    int team;

    void OnTriggerEnter2D(Collider2D collision) {
        //Ball Entry sensor
        if (collision.CompareTag("Ball")) {
            //Reset timer if ball has not passed through both triggers in the given timeframe
            if (Time.realtimeSinceStartup - timeAtLastCheck > 1.0f) {
                hasEntered = false;
                hasExit = false;
            }

            //Checks to see if ball has entered the entry trigger first
            if (collision.IsTouching(ballEntry) && !hasEntered && !hasExit) {
                hasEntered = true;
                timeAtLastCheck = Time.realtimeSinceStartup;
            }

            //Checks to see if the ball has entered the exit trigger after passing through the entry trigger within the timeframe
            if (collision.IsTouching(ballExit) && !hasExit && hasEntered) {
                if (Time.realtimeSinceStartup - timeAtLastCheck <= 1.0f) {
                    hasExit = true;
                }
            }
        }
    }

}
