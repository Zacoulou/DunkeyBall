using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopController : MonoBehaviour
{
    public Transform centerBasket;
    public BoxCollider ballEntry;
    public BoxCollider ballExit;

    bool hasEntered = false;
    bool hasExit = false;

    float timeAtLastCheck = 0.0f;
    int team;

    void OnTriggerEnter(Collider collision) {
        //Ball Entry sensor
        if (collision.CompareTag("Ball")) {
            //Reset timer if ball has not passed through both triggers in the given timeframe
            if (Time.realtimeSinceStartup - timeAtLastCheck > 1.0f) {
                hasEntered = false;
                hasExit = false;
            }

            //Checks to see if ball has entered the entry trigger first
            if (collision.bounds.Intersects(ballEntry.bounds) && !hasEntered && !hasExit) {
                hasEntered = true;
                timeAtLastCheck = Time.realtimeSinceStartup;
            }

            //Checks to see if the ball has entered the exit trigger after passing through the entry trigger within the timeframe
            if (collision.bounds.Intersects(ballExit.bounds) && !hasExit && hasEntered) {
                if (Time.realtimeSinceStartup - timeAtLastCheck <= 1.0f) {
                    hasExit = true;
                    RegisterPoints();
                }
            }
        }
    }

    void RegisterPoints() {
        //AudioManager.instance.Play("Swish");
        //Debug.Log("THAT'S A GOSH DARN BUCKET BROTHER!!!");
        hasEntered = false;
        hasExit = false;

        //if (XdistanceFromHoopAtShot >= threePointDistance) {
        //    gameManager.UpdateScore(3, team);
        //    AudioManager.instance.Play("CrowdRoarFor3");
        //} else if (XdistanceFromHoopAtShot < threePointDistance) {
        //    gameManager.UpdateScore(2, team);
        //}

    }

}
