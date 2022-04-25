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
    bool isAiming = false;
    float ballGravity = 0f;
    Ball ball;

    public void OnStartShot() {
        //TimeManager.Instance.StartSlowMotion(2f, 0.1f);
        //MovingCam.AddTarget(hoop.transform);
        ball = pController.GetBallInPossession();
        if (ball != null) {
            isAiming = true;
            shotMeter.StartShotMeter();
            setBallGravity();
        }
    }

    void setBallGravity() {
        ballGravity = ball.GetComponent<CustomGravity>().gravityScale * CustomGravity.globalGravity;
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
                    Debug.Log("Perfect Shot!");
                }

                Vector3 shotPosition = SelectShotPositionBasedOnReleaseTiming(releaseTiming, hoop.centerBasket.position);
                Vector3 shotVector = CalculateShotVector(releaseTiming, shotPosition);                
                //Debug.Log(releaseTiming + " | " + xVel + " | " + yVel + " | " + zVel);

                //move ball to shot release position
                ball.gameObject.SetActive(true);
                ball.PositionBall(shotReleasePoint.position);

                //Apply calculated x and y velocity components to ball
                ball.ApplyForce(shotVector);

                //Apply backspin to shot depending on direction of shot
                if (shotReleasePoint.position.x < hoop.centerBasket.position.x) {
                    ball.SetAngularVelocity(new Vector3(0f, 0f, backspin));
                } else {
                    ball.SetAngularVelocity(new Vector3(0f, 0f, -backspin));
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

    public Vector3 GetPointOnUnitCircleCircumference(float radius) {
        float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * radius*2);
        return new Vector3(Mathf.Sin(randomAngle), 0f, Mathf.Cos(randomAngle)).normalized;
    }

    Vector3 SelectShotPositionBasedOnReleaseTiming(float releaseTiming, Vector3 desiredPosition) {
        //float minShotErrorRadius = 0f; // A perfect shot would fall into this circle
        float rimRadius = 1f;
        float maxErrorMultiplier = 2f;
        float maxErrorRadius = rimRadius * maxErrorMultiplier; 
        float shotErrorRadius = (1 - releaseTiming) * maxErrorRadius;

        //Vector3 shotErrorPoint = GetPointOnUnitCircleCircumference(shotErrorRadius);
        Vector2 shotErrorPoint2D = UnityEngine.Random.insideUnitCircle*shotErrorRadius;
        Vector3 shotErrorPoint = new Vector3(shotErrorPoint2D.x, 0f, shotErrorPoint2D.y);

        Debug.Log("Shot ERROR Radius: " + shotErrorRadius);
        Debug.DrawRay(desiredPosition + shotErrorPoint, Vector3.up, Color.white, 5f);
        Debug.DrawRay(desiredPosition, Vector3.left * shotErrorRadius, Color.blue, 5f);

        return desiredPosition + shotErrorPoint;
    }

    Vector3 CalculateShotVector(float releaseTiming, Vector3 desiredPosition) {
        Vector3 shotVector = new Vector3(0,0,0);
        float currentX = shotReleasePoint.position.x;
        float currentY = shotReleasePoint.position.y;
        float currentZ = shotReleasePoint.position.z;

        //change shotTime based on proximity to hoop and gravity scale to alter arc and add error
        float shotTime = Vector3.Distance(shotReleasePoint.position, desiredPosition) / 8f;
        shotTime = Mathf.Clamp(shotTime, 0.8f, 2f);

        shotVector.x = CalcXspeed(currentX, desiredPosition.x, shotTime);
        shotVector.y = CalcYspeed(currentY, desiredPosition.y, shotTime);
        shotVector.z = CalcZspeed(currentZ, desiredPosition.z, shotTime);

        return shotVector;
    }

    float CalcXspeed(float currentX, float desiredX, float shotTime) {
        return (desiredX - currentX) / shotTime; 
    }


    float CalcYspeed(float currentY, float desiredY, float shotTime) {
        return ((0.5f * -ballGravity * Mathf.Pow(shotTime, 2)) + (desiredY - currentY)) / shotTime;
    }


    float CalcZspeed(float currentZ, float desiredZ, float shotTime) {
        return (desiredZ - currentZ) / shotTime;
    }
}
