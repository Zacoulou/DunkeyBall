using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    [SerializeField] PlayerController pController;
    [SerializeField] Transform shotReleasePoint;
    [SerializeField] ShotReleaseMeter shotMeter;
    bool isAiming = false;
    float ballGravity = 0f;
    Ball ball;
    HoopController hoop;

    //SHOT CHARACTERISTICS
    float minLandAngleDeg           = 40f;                          //Even with a valid trajectory, lower angles will likely bounce off front rim
    Vector2 normalShotMinMaxAngle   = new Vector2(25f, 55f);        //Range of worst timed shot to Best timed shot
    Vector2 floaterMinMaxAngle      = new Vector2(55f, 70f);        //Range of worst timed shot to Best timed shot
    const float backspin            = 500.0f;                       //The magnitude of the backspin applied to the ball
    float minShotErrorRadius        = 0f;                           // A perfect shot would fall into this circle
    float rimRadius                 = 1f;
    float maxErrorMultiplier        = 2f;


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
            hoop = pController.GetHoop();
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

                //Trajectory planning for shot
                Vector3 shotPosition = SelectShotPositionBasedOnReleaseTiming(releaseTiming, hoop.centerBasket.position);
                Vector3 shotVector = Shoot(releaseTiming, shotReleasePoint.position, shotPosition);

                //move ball to shot release position
                ball.gameObject.SetActive(true);
                ball.PositionBall(shotReleasePoint.position);

                //Apply calculated x and y velocity components to ball
                ball.SetVelocity(shotVector);

                //Apply backspin to shot depending on direction of shot
                Vector3 normalizedShotVector = shotVector.normalized;
                ball.SetAngularVelocity(new Vector3(-backspin * normalizedShotVector.z, 0f, backspin * normalizedShotVector.x));

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
        float maxErrorRadius        = rimRadius * maxErrorMultiplier; 
        float shotErrorRadius       = minShotErrorRadius + (1 - releaseTiming) * maxErrorRadius;

        //Vector3 shotErrorPoint = GetPointOnUnitCircleCircumference(shotErrorRadius);
        Vector2 shotErrorPoint2D    = UnityEngine.Random.insideUnitCircle*shotErrorRadius;
        Vector3 shotErrorPoint      = new Vector3(shotErrorPoint2D.x, 0f, shotErrorPoint2D.y);

        //Debug.Log("Shot ERROR Radius: " + shotErrorRadius);
        Debug.DrawRay(desiredPosition + shotErrorPoint, Vector3.up, Color.white, 5f);
        Debug.DrawRay(desiredPosition, Vector3.left * shotErrorRadius, Color.blue, 5f);

        return desiredPosition + shotErrorPoint;
    }

    Vector3 CalculateShotVector(float angleDeg, Vector3 startingPosition, Vector3 desiredPosition) {
        Vector3 delta       = desiredPosition - startingPosition;
        Vector3 velocity3D  = Vector3.zero;
        float xzDistance    = Mathf.Sqrt(Mathf.Pow(delta.x, 2) + Mathf.Pow(delta.z, 2));

        float angleRad      = angleDeg * Mathf.Deg2Rad;

        //Determine if trajectory has a valid solution
        if (delta.y < Mathf.Abs(xzDistance * Mathf.Tan(angleRad))) {
            float speed2D       = Mathf.Sqrt(ballGravity * Mathf.Pow(xzDistance, 2) / (2 * Mathf.Pow(Mathf.Cos(angleRad), 2) * (delta.y - xzDistance * Mathf.Tan(angleRad))));
            float xzMagnitude   = speed2D * Mathf.Cos(angleRad);
            float xzTheta       = Mathf.Atan(delta.z / delta.x);

            //Calculate the initial velocity of the ball
            velocity3D = new Vector3(
                xzMagnitude * Mathf.Abs(Mathf.Cos(xzTheta)) * delta.x / Mathf.Abs(delta.x),
                speed2D     * Mathf.Sin(angleRad),
                xzMagnitude * Mathf.Abs(Mathf.Sin(xzTheta)) * delta.z / Mathf.Abs(delta.z));

        } else {
            Debug.Log("IMPOSSIBLE Angle" + delta.y + "  " + xzDistance * Mathf.Tan(angleRad));
        }

        return velocity3D;
    }

    Vector3 Shoot(float releaseTiming, Vector3 startingPosition, Vector3 desiredPosition) {
        Vector3 result = Vector3.zero;
        Vector3 delta = desiredPosition - startingPosition;
        float xzDistance = Mathf.Sqrt(Mathf.Pow(delta.x, 2) + Mathf.Pow(delta.z, 2));

        float shotAngleDeg = LaunchAngleBasedOnReleaseTiming(releaseTiming);
        Vector3 velocity3D = CalculateShotVector(shotAngleDeg, startingPosition, desiredPosition);

        if (velocity3D != Vector3.zero) {
            //Trajectory Apex
            float maxHeight         = Mathf.Pow(velocity3D.y, 2) / (2 * Mathf.Abs(ballGravity));
            float deltaApexToTarget = startingPosition.y + maxHeight - desiredPosition.y;
            float timeToApex        = Mathf.Abs(velocity3D.y / ballGravity);
            float xzMagnitude       = Mathf.Sqrt(Mathf.Pow(velocity3D.x, 2) + Mathf.Pow(velocity3D.z, 2));
            float xDistanceToApex   = xzMagnitude * timeToApex;
            bool arcPassesApex      = xDistanceToApex < xzDistance;

            //Determine if landing angle needs to be calculated
            if (arcPassesApex) {
                float landingYVel               = Mathf.Sqrt(Mathf.Abs(2 * ballGravity * deltaApexToTarget));
                float landingAngleRad           = Mathf.Atan(Mathf.Abs(landingYVel / xzMagnitude));
                bool isLandingAngleAcceptable   = landingAngleRad >= minLandAngleDeg * Mathf.Deg2Rad;

                if (isLandingAngleAcceptable) {
                    //REGULAR SHOT. USE Velocity3D
                    result = velocity3D;
                } else {
                    //SHOT DOESN'T MEET MIN LANDING ANGLE... FLOATER?
                    Debug.Log("BAD Landing angle: " + landingAngleRad * Mathf.Rad2Deg);
                }
            } else {
                //SHOT IS A LASER TOWARDS BOTTOM OF HOOP... FLOATER?
                Debug.Log("Laser: apex" + xDistanceToApex + " <hoopDist " + xzDistance);
            }
        }

        return result;
    }

    float LaunchAngleBasedOnReleaseTiming(float releaseTiming) {
        float normalShotAngleRange = normalShotMinMaxAngle.y - normalShotMinMaxAngle.x;
        //float floaterAngleRange = floaterMinMaxAngle.y - floaterMinMaxAngle.x;

        float releaseAngle = (normalShotMinMaxAngle.x + releaseTiming * normalShotAngleRange);
        Debug.Log("Release angle: " + releaseAngle);
        return releaseAngle;
    }

    //Returns 1 if player is to the left, or -1 if player is to the right of the hoop
    public int GetXSideOfHoop() {
        int side = 1;
        if (!hoop) {hoop = pController.GetHoop();}
        if (hoop.centerBasket.position.x - shotReleasePoint.position.x < 0) { side = -1; }
        return side;
    }
}
