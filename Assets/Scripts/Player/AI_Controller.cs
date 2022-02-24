using System;
using UnityEngine;

public class AI_Controller : MonoBehaviour {
    private PlayerMovement mover;
    [System.NonSerialized] public Basketball ball;
    [System.NonSerialized] public float maxAIspeed = 0.8f;
    private float currSpeed = 0.1f;
    private float acceleration = 0.05f;
    private float deadZone = 0.1f;

    private GameObject teamate = null;
    private AI_Controller aiTeamateController = null;
    private string switchCase;
    private Vector2 movement;
    private float sideOffset = 0.4f;
    public bool isAI = false;
    private bool shouldSprint = false;

    private void Awake() {
        mover = GetComponent<PlayerMovement>();
        ball = GameObject.FindObjectOfType(typeof(Basketball)) as Basketball;
    }

    public void SetTeamate(GameObject teamMember) {
        teamate = teamMember;
        if (teamate.GetComponent<AI_Controller>().isAI)
            aiTeamateController = teamate.GetComponent<AI_Controller>();

        Debug.Log("I've got a teamate!!  " + teamate.name);
    }

    public void InitializePlayer(int team, PlayerConfiguration playerConfig) {
        mover.playerTeam = team; //Sets player's team
        mover.playerInputIndex = playerConfig.PlayerIndex;
        mover.characterAppearanceIndices = playerConfig.appearanceIndices; //sets AI's appearance
        mover.playerColor = playerConfig.playerColor;
        isAI = true;
        EstablishSide();
    }

    // Update is called once per frame
    void Update() {
        //If players Ankles are broken attempt to get up as soon as possible
        if (mover.anklesBroken) {
            mover.OnJump();
            switchCase = "none";
        }

        //Set AI to state depending if AI has ball, nobody has ball, or opponent has ball, or teamate has ball
        if (mover.hasBall)
            switchCase = "offensiveMovement";
        else if (ball.playerWithPossesion == null)
            switchCase = "chasingBall";
        else if (ball.playerWithPossesion.GetComponent<PlayerMovement>().playerTeam != mover.playerTeam)
            switchCase = "defense";
        else if (ball.playerWithPossesion.GetComponent<PlayerMovement>().playerTeam == mover.playerTeam)
            switchCase = "offensiveSupport";

        if (mover.stoppedState)
            switchCase = "none";

        switch (switchCase) {
            case "defense":
                //if closer to opponent with ball than teamate or does not have a teamate, move to guard opponent with ball
                if (aiTeamateController == null || (Math.Abs(FindXDistanceToOpponentWithBall()) < Math.Abs(aiTeamateController.FindXDistanceToOpponentWithBall()))) {
                    //Set the player to sprint if they are behind player with ball and need to catch up
                    if (mover.playerTeam == 0 && FindXDistanceToOpponentWithBall() >= 0)
                        shouldSprint = false;
                    else if (mover.playerTeam == 1 && FindXDistanceToOpponentWithBall() <= 0)
                        shouldSprint = false;
                    else
                        shouldSprint = false;

                    SetDirectionTowards(FindXDistanceToOpponentWithBall() + sideOffset, deadZone, shouldSprint);
                } else { //provide help defense farther back
                    shouldSprint = false;
                    SetDirectionTowards(FindXDistanceToTeamate() + sideOffset*2, deadZone, shouldSprint);
                }

                //Attempt to swat ball away from player
                if (Math.Abs(FindXDistanceToOpponentWithBall()) <= 0.5f)
                    mover.OnSwat();

                //Jump when ball is in an upper diagonal position (opponent is likely jumping to shoot or has shot)
                if (FindYDistanceToBall() > 1.0f && FindYDistanceToBall() < 2.0f && Mathf.Abs(FindXDistanceToBall()) <= 2.0f)
                    mover.OnJump();

                break;

            case "chasingBall":
                //if closer to ball than teamate or does not have a teamate, move to ball
                if (aiTeamateController == null || (Math.Abs(FindXDistanceToBall()) < Math.Abs(aiTeamateController.FindXDistanceToBall()))) {
                    shouldSprint = false;
                    SetDirectionTowards(FindXDistanceToBall(), deadZone, shouldSprint);
                } else { //provide help farther back
                    shouldSprint = false;
                    SetDirectionTowards(FindXDistanceToTeamate() + sideOffset * 2, deadZone, shouldSprint);
                }

                //Jump when ball is in an upper diagonal reach only accessible by jumping
                if (FindYDistanceToBall() > 2f && FindYDistanceToBall() < 4f && Mathf.Abs(FindXDistanceToBall()) <= 3.0f) {
                    //check if ball is moving outside of reach and only jump if it is within reach
                    if (!(FindYDistanceToBall() > 2f && mover.ball.rb.velocity.y > 0))
                        mover.OnJump();
                }


                break;

            case "offensiveMovement":
                //Move to opponents basket
                shouldSprint = false;
                SetDirectionTowards(FindXDistanceToBasket() + (sideOffset * 4), deadZone, shouldSprint);

                //Once in an acceptable shooting range, shoot.
                if (Mathf.Abs(FindXDistanceToBasket()) <= 7.0f && Mathf.Abs(FindXDistanceToBasket()) >= Mathf.Abs(sideOffset) * 3)
                    Shoot();

                break;

            case "offensiveSupport":
                //Provide support from behind
                shouldSprint = false;
                SetDirectionTowards(FindXDistanceToTeamate() + sideOffset * 3, deadZone, shouldSprint);

                break;

            default:
                break;

        }
    }


    private float FindXDistanceToBall() {
        return mover.transform.position.x - ball.transform.position.x;
    }

    private float FindXDistanceToBasket() {
        return mover.transform.position.x - mover.hoop.transform.position.x;
    }

    private float FindXDistanceToOpponentWithBall() {
        return mover.transform.position.x - ball.playerWithPossesion.GetComponent<PlayerMovement>().transform.position.x;
    }

    private float FindXDistanceToTeamate() { 
        return mover.transform.position.x - teamate.GetComponent<PlayerMovement>().transform.position.x;
    }

    private float FindYDistanceToBall() {
        return ball.transform.position.y - mover.transform.position.y;
    }

    private void EstablishSide() {
        if (mover.playerTeam == 1)
            sideOffset *= -1;
    }

    private void SetDirectionTowards(float diffInXAxis, float deadZone, bool sprint) {
        if (diffInXAxis > deadZone)
            currSpeed -= acceleration;
        else if (diffInXAxis < -deadZone)
            currSpeed += acceleration;
        else
            currSpeed = 0f;

        if (currSpeed <= -maxAIspeed)
            currSpeed = -maxAIspeed;
        else if (currSpeed >= maxAIspeed)
            currSpeed = maxAIspeed;

        movement = new Vector2(currSpeed, 0f);
        mover.OnMove(movement);
        mover.OnSetMovement(movement);

        if (sprint) {
            mover.OnStartSprint();
        }
    }


    private void Shoot() {
        mover.OnJump();

        if (mover.transform.position.y >= 4f) {
            mover.ShootBall();
        }
    }
}
