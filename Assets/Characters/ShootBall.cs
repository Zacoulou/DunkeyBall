using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    [SerializeField] PlayerController pController;
    [SerializeField] Transform shotReleasePoint;
    [SerializeField] ShotReleaseMeter shotMeter;
    const float backspin = 500.0f;
    float shotTime = 1.0f;
    bool isAiming = false;

    public void OnStartShot() {
        //TimeManager.Instance.StartSlowMotion(2f, 0.1f);
        //MovingCam.AddTarget(hoop.transform);
        if (pController.GetBallInPossession()) {
            isAiming = true;
            shotMeter.StartShotMeter();
        }
    }

    void cancelShot() {

    }

    public void ReleaseShot() {
        if (isAiming) {
            Ball ball = pController.GetBallInPossession();
            HoopController hoop = pController.GetHoop();
            isAiming = false;

            if (ball && hoop) {
                //SET ANIMATION
                if (pController.movementController.GetIsWallSliding()) {
                    pController.stateController.SetTriggerState(PlayerStateController.TriggerStates.RELEASEWALLSHOT);
                } else {
                    pController.stateController.SetTriggerState(PlayerStateController.TriggerStates.RELEASESHOT);
                }

                float releaseTiming = shotMeter.GetReleaseTiming();

                if (releaseTiming >= 0.9f) {
                    releaseTiming = 1f;
                    //Debug.Log("Perfect Shot!");
                }

                float xVel = CalcXspeed(releaseTiming, hoop.centerBasket.position.x);
                float yVel = CalcYspeed(hoop.centerBasket.position.y);

                //Debug.Log(releaseTiming + " | " + xVel + " | " + yVel);

                //move ball to shot release position
                ball.gameObject.SetActive(true);
                ball.PositionBall(shotReleasePoint.position.x, shotReleasePoint.position.y);

                //Apply calculated x and y velocity components to ball
                ball.ApplyForce(xVel, yVel);


                //Apply backspin to shot depending on direction of shot
                if (shotReleasePoint.position.x < hoop.centerBasket.position.x) {
                    ball.SetAngularVelocity(backspin);
                } else {
                    ball.SetAngularVelocity(-backspin);
                }

                pController.SetHasBall(false);
            }
        }
    }

    public bool GetIsAiming() {
        return isAiming;
    }
    float ApplyErrorTo(float value, float error) {
        return value * UnityEngine.Random.Range(1f - error, 1f + error);
    }

    float CalcXspeed(float releaseTiming, float desiredX) {
        float currentX = shotReleasePoint.position.x;
        float mapGrav = Physics2D.gravity.y;

        //change shotTime based on proximity to hoop and gravity scale to alter arc and add error
        shotTime = ApplyErrorTo((-29.43f / mapGrav) * (Math.Abs(desiredX - currentX)) / 20f, 1f - releaseTiming) + 0.6f;

        float xVel = (desiredX - currentX) / shotTime;

        return ApplyErrorTo(xVel, 1f - releaseTiming);
    }


    float CalcYspeed(float desiredY) {
        float currentY = shotReleasePoint.position.y;
        float gravity = Physics2D.gravity.y;

        float yVel = ((0.5f * -gravity * shotTime * shotTime) + (desiredY - currentY)) / shotTime;

        return yVel;
    }
}
