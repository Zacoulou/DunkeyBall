using EZCameraShake;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public GameObject basketballPreFab;
    private GameObject Team0Hoop;
    private GameObject Team1Hoop;
    public Transform hoop0Location;
    public Transform hoop1Location;
    public Transform tipOffLocation;

    public TextMeshProUGUI Team0ScoreText;
    public TextMeshProUGUI Team1ScoreText;
    public TextMeshProUGUI rightIndicatorText;
    public TextMeshProUGUI leftIndicatorText;
    public TextMeshProUGUI leftWinnerText;
    public TextMeshProUGUI rightWinnerText;
    public TextMeshProUGUI suddenDeathText;

    private int Team0Score = 0;
    private int Team1Score = 0;
    private string scoreString = "0";
    private InitializeLevel initLevelCode;
    private GameObject ball;
    private Basketball bball;

    private bool wait = false;
    private bool jumpBallTimer = false;
    public float timeAtScore = 0f;
    public float timeAtReset = 0f;
    public int lastTeamScored;
    private bool firstJumpBall = true;

    private GameSettings gameSettings;
    public bool gameOver = false;
    private bool overTime = false;

    private MultipleTargetCamera MovingCam;

    private void Awake() {
        //Use gamesettings as set by user in GameSettingsSpawner from GameConfigManager
        gameSettings = GameSettingsManager.Instance.gameSettings;

        MovingCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MultipleTargetCamera>();

        //Initialize all players in game
        initLevelCode = this.GetComponent<InitializeLevel>();

        //Spawns Ball and Hoops
        ball = Instantiate(basketballPreFab) as GameObject;
        ball.transform.position = new Vector2(3.0f, 1.0f);
        bball = ball.GetComponent<Basketball>();

        Team0Hoop = gameSettings.map.map.GetComponent<UniqueMapFeatures>().hoopLeft;
        Team0Hoop.GetComponent<Hoop>().team = 1;

        Team1Hoop = gameSettings.map.map.GetComponent<UniqueMapFeatures>().hoopRight;
        Team1Hoop.GetComponent<Hoop>().team = 0;
    }
    public void StartGame() {

        //Add ball to camera movement manager
        MovingCam.AddTarget(ball.transform);
        MovingCam.SetDefaultTargets();

        //Start game by reseting player positions and initiating tip off
        ResetPositions();
        jumpBallTimer = true;
        timeAtReset = Time.realtimeSinceStartup;
        gameOver = false;
        overTime = false;

        if (gameSettings.hastimer) {
            TimerController.instance.timer.enabled = true; // Display Timer text            
            TimerController.instance.beginTimer(gameSettings.endCondition + 1f, 0f); //Start gameTimer            
            TimerController.instance.pauseResumeTimer(false); //pause timer
        } else { TimerController.instance.timer.enabled = false; } // hide timer text
    }

    private void FixedUpdate() {

        //if wait timer has started, check when it is done and then reset players to spawn locations
        if (wait) {
            if (Time.realtimeSinceStartup >= (timeAtScore + 1.0f)) {
                ResetPositions();
                timeAtReset = Time.realtimeSinceStartup;
                jumpBallTimer = true;
                wait = false;
            }
        }

        //if jumpBallTimer has started, check when it is done and initiate jump ball
        if (jumpBallTimer) {
            if (Time.realtimeSinceStartup >= (timeAtReset + 2.0f)) {

                bball.inRangeCollider.enabled = true;
                if (firstJumpBall) {
                    firstJumpBall = false;
                    bball.ApplyForce(0.0f, 15.0f);
                } else if (lastTeamScored == 1)
                    bball.ApplyForce(-4.0f, 10.0f);
                else if (lastTeamScored == 0)
                    bball.ApplyForce(4.0f, 10.0f);

                //Allow players to move again
                for (int i = 0; i < initLevelCode.playerList.Count; i++) {
                    initLevelCode.playerList[i].GetComponent<PlayerMovement>().stoppedState = false;
                }
                jumpBallTimer = false;
                bball.SetGravity(true);

                if (gameSettings.hastimer)
                    TimerController.instance.pauseResumeTimer(true); //resume timer
            }
        }

        if (CheckGameOver() && !bball.shootingState && !gameOver && !overTime)
            EndMatch();

    }

    public void UpdateScore(int newPoints, int team) {
        if (!gameOver) {
            if (team == 0) {
                Team0Score += newPoints;
                lastTeamScored = 0;
                rightIndicatorText.gameObject.SetActive(true);
                rightIndicatorText.text = "+" + newPoints.ToString();
                rightIndicatorText.GetComponent<PointIndicatorTween>().StartAnimation(-1.0f, 2.0f, 1.0f);
            } else if (team == 1) {
                Team1Score += newPoints;
                lastTeamScored = 1;
                leftIndicatorText.gameObject.SetActive(true);
                leftIndicatorText.text = "+" + newPoints.ToString();
                leftIndicatorText.GetComponent<PointIndicatorTween>().StartAnimation(1.0f, 2.0f, 1.0f);
            }

            //Shake Camera for every point
            MovingCam.ShakeOnce(3f, 10f, .3f, 2f);

            if (gameSettings.hastimer)
                TimerController.instance.pauseResumeTimer(false);  //pause timer

            bball.inRangeCollider.enabled = false;//prevent further collisions with ball

            bball.shootingState = false;//reset ball to non shooting state

            DisplayScore(team);
            timeAtScore = Time.realtimeSinceStartup;
            wait = true; //wait long enough for the ball to go through the hoop

            if (CheckGameOver())
                EndMatch();
        }
    }

    private bool CheckGameOver() {
        bool isOver = false;

        switch (gameSettings.gameTypeIndex) {
            case 0: //Score
                if (Team1Score >= gameSettings.endCondition || Team0Score >= gameSettings.endCondition)
                    isOver = true;
                break;

            case 1: //Timed
                if (TimerController.instance.getElapsedTime() <= 0)
                    isOver = true;
                break;


            default:
                break;
        }

        return isOver;
    }

    public void DisplayScore(int team) {
        if (team == 0) {
            scoreString = Team0Score.ToString();
            if (Team0Score < 10)
                scoreString = "0" + Team0Score.ToString();

            Team0ScoreText.text = scoreString;
        } else if (team == 1) {
            scoreString = Team1Score.ToString();
            if (Team1Score < 10)
                scoreString = "0" + Team1Score.ToString();

            Team1ScoreText.text = scoreString;
        }
    }

    private void ResetPositions() {
        string[] t = { "Ready", "Set" };
        CountDownController.instance.StartCountDown(t); //start CountDown Timer with custom text

        for (int i = 0; i < initLevelCode.playerList.Count; i++) {
            initLevelCode.playerList[i].transform.position = initLevelCode.playerList[i].GetComponent<PlayerMovement>().spawnPoint.position;
            initLevelCode.playerList[i].GetComponent<PlayerMovement>().ResetPlayerMovement();
        }

        bball.transform.position = tipOffLocation.position; //center ball for jumpball
        bball.SetPlayerWithPossesion(null);
        bball.SetAngularVelocity(0f);
        bball.ApplyForce(0f, 0f);
        bball.SetGravity(false);
    }


    void EndMatch() {

        MovingCam.AddTarget(Team0Hoop.transform);
        MovingCam.AddTarget(Team1Hoop.transform);

        gameOver = true;
        wait = false;
        overTime = false;

        if (Team0Score > Team1Score) { // Team 0 wins
            leftWinnerText.gameObject.SetActive(true);
            leftWinnerText.GetComponent<PulseTween>().StartAnimation(5f);
        } else if (Team1Score > Team0Score) { // Team 1 wins
            rightWinnerText.gameObject.SetActive(true);
            rightWinnerText.GetComponent<PulseTween>().StartAnimation(5f);
        } else { //Case of Tie, goes to sudden death (first point wins)
            TimerController.instance.timer.enabled = false; // hides Timer text   
            suddenDeathText.gameObject.SetActive(true);
            suddenDeathText.GetComponent<PulseTween>().StartAnimation(2f);

            wait = true;
            gameOver = false;
            overTime = true;
            firstJumpBall = true;
        }

        bball.inRangeCollider.enabled = false;//prevent further collisions with ball
    }
}
