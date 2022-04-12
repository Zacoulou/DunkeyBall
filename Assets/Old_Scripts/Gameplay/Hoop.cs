using UnityEngine;

public class Hoop : MonoBehaviour {
    public Transform centerBasket;
    public BoxCollider2D ballEntry;
    public BoxCollider2D ballExit;

    private bool hasEntered = false;
    private bool hasExit = false;

    public GameManager gameManager;
    private float timeAtLastCheck = 0.0f;
    public float XdistanceFromHoopAtShot = 0.0f;
    private readonly float threePointDistance = 8.85f;

    public int team;


    void Awake() {
        gameManager = GameObject.FindObjectOfType(typeof(GameManager)) as GameManager;
    }

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
                    RegisterPoints();
                }
            }
        }
    }

    void RegisterPoints() {
        AudioManager.instance.Play("Swish");
        //Debug.Log("THAT'S A GOSH DARN BUCKET BROTHER!!!");
        hasEntered = false;
        hasExit = false;

        if (XdistanceFromHoopAtShot >= threePointDistance) {
            gameManager.UpdateScore(3, team);
            AudioManager.instance.Play("CrowdRoarFor3");
        } else if (XdistanceFromHoopAtShot < threePointDistance) {
            gameManager.UpdateScore(2, team);
        }

    }
}
