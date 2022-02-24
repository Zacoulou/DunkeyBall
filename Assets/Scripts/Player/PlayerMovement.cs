using System;
using EZCameraShake;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {
    //Character specific Traits
    [System.NonSerialized] public float runSpeed = 20.0f;
    public float onBallDunkMeterIncrement = 0.0005f;
    public float offBallDunkMeterIncrement = 0.0001f;
    private readonly float swatCost = 0.1f;
    private readonly float sprintCost = 0.002f;
    private readonly float shotDunkMeterIncrement = 0.05f;
    private readonly float blockDunkMeterIncrement = 0.2f;


    public int playerSortingOrder = 0;
    public Rigidbody2D rb;
    Vector2 i_movement;
    [System.NonSerialized] public bool isSprinting = false;
    public CharacterController2D controller;
    [SerializeField] private RagdollController rdController;
    public SwitchPart characterPartSwitcher;
    private MultipleTargetCamera MovingCam;

    public Animator animator;
    public Transform shotReleasePoint;
    public Transform dribblePos;
    public Transform defensiveReachPos;
    [NonSerialized] public Transform spawnPoint;
    public float defensiveReachRange;
    public LayerMask defensiveReachLayer;
    public SpriteRenderer ballSprite;
    public ParticleSystem dust;

    public PlayerInfoUI playerInfoUI;
    [NonSerialized] public Color playerColor;

    bool jump = false;
    bool wasJumping = false;
    [NonSerialized] public bool hasBall = false;
    [NonSerialized] public bool hasShot = false;

    public ShotMeter shotMeter;
    private float timeAtShot = 0.0f;
    private float timeAtSwat = 0.0f;
    private float timeAtBlock = 0.0f;
    private float timeatAtAnklesBroken = 0.0f;
    private readonly float anklesBrokenTimer = 2f;
    public bool anklesBroken = false;
    [NonSerialized] public Hoop hoop;
    [NonSerialized] public Basketball ball;
    private float shotTime = 1.0f;
    [NonSerialized] public int playerTeam;
    [NonSerialized] public int playerInputIndex;
    [NonSerialized] public bool stoppedState = false;
    [NonSerialized] public bool smooveMove = false; //smooth movement during knockback
    private readonly float smooveIncrement = 0.05f;
    [NonSerialized] public Vector2 lastControllerInput;
    private Timer timer;

    [NonSerialized] public string facingDirection = "right";
    [NonSerialized] public string hoopDirection;
    [NonSerialized] public bool flipDirectionFacing = false;

    //public float shotError = 0.1f;
    [NonSerialized] public int characterAppearanceIndices = 0;


    // Start is called before the first frame update
    void Start() {
        timer = GetComponent<Timer>();

        //Apply correct skin
        ApplySkin();
        ballSprite.enabled = false;

        //Change color of colorIndicator
        playerInfoUI.SetColor(playerColor);

        //trajectory = GameObject.FindGameObjectWithTag("Trajectory").GetComponent<Trajectory>();
        MovingCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MultipleTargetCamera>();

        //Find game ball and hoops, then set correct hoop for player
        ball = GameObject.FindObjectOfType(typeof(Basketball)) as Basketball;

        Hoop[] hoops = GameObject.FindObjectsOfType(typeof(Hoop)) as Hoop[];
        for (int i = 0; i < hoops.Length; i++) {
            if (hoops[i].team == this.playerTeam) {
                hoop = hoops[i] as Hoop;
                break;
            }
        }

        //set hoopDirection to direction of respective hoop
        if (playerTeam == 0)
            hoopDirection = "right";
        else
            hoopDirection = "left";
    }

    // Update is called once per frame
    void Update() {
        playerInfoUI.UpdateUILocation(this.transform.position.x, playerSortingOrder, animator.GetBool("isJumping"));
        animator.SetBool("hasBall", hasBall);

        // Orient player towards ball when they do not have possesion
        if (!hasBall && ((this.transform.position.x - ball.transform.position.x) > 0)) { //&& !facingDirection.Equals("left")
            facingDirection = "left";
            flipDirectionFacing = true;
        } else if (!hasBall && ((this.transform.position.x - ball.transform.position.x) < 0)) { //&& !facingDirection.Equals("right")
            facingDirection = "right";
            flipDirectionFacing = true;
        }

        if (hasBall && !stoppedState) {
            //orient player towards hoop when they have possesion
            if (!facingDirection.Equals(hoopDirection)) {
                facingDirection = hoopDirection;
                flipDirectionFacing = true;
            }

            DribbleBall();
        }
    }

    void FixedUpdate() {
        if (!stoppedState) {
            OnMove(lastControllerInput);
            Vector2 movement = new Vector2(i_movement.x, 0) * runSpeed;
            animator.SetFloat("xVel", Mathf.Abs(movement.x / 20));
            controller.Move(movement.x * Time.fixedDeltaTime, isSprinting, jump, flipDirectionFacing, facingDirection);
            flipDirectionFacing = false;
            jump = false;

            //if you have the ball, steadily add to dunkmeter, otherwise increase dunkmeter more slowly
            if (hasBall)
                playerInfoUI.ChangeDunkMeterValue(onBallDunkMeterIncrement);
            else {
                playerInfoUI.ChangeDunkMeterValue(offBallDunkMeterIncrement);
            }

            //if sprinting, decrease dunkmeter
            if (playerInfoUI.GetDunkMeterValue() < sprintCost) {
                isSprinting = false;
            }
            if (isSprinting) {
                playerInfoUI.ChangeDunkMeterValue(-sprintCost);
                if (!animator.GetBool("isJumping"))
                    CreateDust();
            }



        } else {
            animator.SetFloat("xVel", 0f);

            if (rdController.RagdollActive)
                controller.Move(0f, isSprinting, false, false, facingDirection);
            else
                controller.Move(0f, isSprinting, false, flipDirectionFacing, facingDirection);

            flipDirectionFacing = false;
        }
        animator.SetFloat("yVel", rb.velocity.y);
    }

    public void OnSetMovement(CallbackContext context) {
        lastControllerInput = context.ReadValue<Vector2>();
    }

    public void OnSetMovement(Vector2 movementVector) {
        lastControllerInput = movementVector;
    }

    public void OnMove(Vector2 movementVector) {
        if (movementVector.x >= -0.2f && movementVector.x <= 0.2f)
            movementVector.x = 0f;

        if (!smooveMove) {
            i_movement = movementVector;
        } else {
            i_movement.x = Mathf.Clamp((rb.velocity.x / 4f) + movementVector.x * smooveIncrement, -1f, 1f);
        }
    }

    public void OnStartShot() {
        if (hasBall) {

            animator.SetBool("isShooting", true);

            TimeManager.Instance.StartSlowMotion(2f, 0.1f);

            MovingCam.AddTarget(hoop.transform);

            shotMeter.StartShotMeter();
        }
    }

    public void OnReleaseShot() {
        if (hasBall) {
            animator.SetBool("isShooting", false);

            float releaseTiming = shotMeter.ReleaseShot();

            if (releaseTiming >= 0.9f) {
                releaseTiming = 1f;
                Debug.Log("Perfect Shot!  " + releaseTiming);
            }

            //Debug.Log(releaseTiming);

            float xVel = CalcXspeed(releaseTiming);
            float yVel = CalcYspeed();

            //move ball to shot release position
            ball.gameObject.SetActive(true);
            ball.PositionBall(shotReleasePoint.position.x, shotReleasePoint.position.y);

            //Apply calculated x and y velocity components to ball
            ball.ApplyForce(xVel, yVel);
            //set ball to shooting state
            ball.shootingState = true;

            //apply knockback to player
            //ApplyForce(-xVel* 50, -yVel * 50);

            //Apply backspin to shot depending on direction of shot
            if (this.playerTeam == 0) {
                ball.SetAngularVelocity(500.0f);
            } else if (this.playerTeam == 1) {
                ball.SetAngularVelocity(-500.0f);
            }

            hasBall = false;
            ballSprite.enabled = false;
            CancelDribble();
            ball.SetPlayerWithPossesion(null);
            hasShot = true;
            timeAtShot = Time.realtimeSinceStartup;

            //saves distance ball was shot from in order to calculate whether it was a 2 or 3 point shot
            if (!animator.GetBool("isJumping")) {
                hoop.XdistanceFromHoopAtShot = Mathf.Abs(hoop.centerBasket.position.x - this.transform.position.x);
            }

            playerInfoUI.ChangeDunkMeterValue(shotDunkMeterIncrement);
        }
    }

    public void OnSwat() {
        if (!hasBall && !stoppedState) {
            if ((Time.realtimeSinceStartup - timeAtSwat >= 0.5f) && playerInfoUI.GetDunkMeterValue() >= swatCost) {
                DefensiveReach();
            } else if (!rdController.RagdollActive) {
                rdController.ActivateRagdoll(rb.velocity, facingDirection);
                StopPlayer();
                timeatAtAnklesBroken = Time.realtimeSinceStartup;
                anklesBroken = true;
            }
        }
    }

    public void OnStartSprint() {
        if (playerInfoUI.GetDunkMeterValue() >= sprintCost)
            isSprinting = true;
    }

    public void OnEndSprint() {
        isSprinting = false;
    }

    public void OnJump() {

        //In the case of when the player wants to jump
        if (!stoppedState) {
            jump = true;
            animator.SetBool("isJumping", true);
            CancelDribble();

            if (!wasJumping) {
                hoop.XdistanceFromHoopAtShot = Mathf.Abs(hoop.centerBasket.position.x - this.transform.position.x);
                wasJumping = true;
            }
            //In the case of getting up after having their ankles broken
        } else if (stoppedState && rdController.RagdollActive && !animator.GetBool("isJumping") && (Time.realtimeSinceStartup - timeatAtAnklesBroken >= anklesBrokenTimer)) {
            rdController.DisableRagdoll();
            stoppedState = false;

            if (rdController.GetRagdollRot() < 0)
                animator.SetTrigger("getUpFront");
            else
                animator.SetTrigger("getUpBack");

            anklesBroken = false;
            jump = true;

            if (!wasJumping) {
                hoop.XdistanceFromHoopAtShot = Mathf.Abs(hoop.centerBasket.position.x - this.transform.position.x);
                wasJumping = true;
            }
        }
    }

    public void OnJumpRelease() {
        controller.ReleaseJump();
    }

    public void OnLanding() {
        animator.SetBool("isJumping", false);
        wasJumping = false;
        CreateDust();
    }

    float ApplyErrorTo(float value, float error) {
        return value * UnityEngine.Random.Range(1f - error, 1f + error);
    }

    float CalcXspeed(float releaseTiming) {
        float desiredX = hoop.centerBasket.position.x;
        float currentX = shotReleasePoint.position.x;
        float mapGrav = ball.GetComponent<Basketball>().gravScale * Math.Abs(Physics2D.gravity.y);

        //change shotTime based on proximity to hoop and gravity scale to alter arc and add error
        shotTime = ApplyErrorTo((29.43f / mapGrav) * (Math.Abs((desiredX - currentX))) / 20f, 1f - releaseTiming) + 0.6f;

        float xVel = (desiredX - currentX) / shotTime;

        return ApplyErrorTo(xVel, 1f - releaseTiming);
    }


    float CalcYspeed() {
        float desiredY = hoop.centerBasket.position.y;
        float currentY = shotReleasePoint.position.y;
        float gravity = ball.GetComponent<Basketball>().gravScale * Math.Abs(Physics2D.gravity.y);

        float yVel = ((0.5f * gravity * shotTime * shotTime) + (desiredY - currentY)) / shotTime;

        return yVel;
    }

    //Detect collisions between the GameObjects with Colliders attached
    void OnTriggerEnter2D(Collider2D collision) {

        //Debug.Log(collision.gameObject.tag);

        //Check for a match with the specific tag on any GameObject that collides with your GameObject
        if (collision.gameObject.CompareTag("Ball")) {
            //Ignore collision so it prevents ball from being trapped in player
            Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(), ball.physicsCollider);

            //check that player does not have ball, ball is not being shot, a certain time has passed since shooting or swatting, 
            // and no other player has the ball
            if (!hasBall && !(ball.shootingState && ball.rb.velocity.y >= 0f) && ((Time.realtimeSinceStartup - timeAtShot) >= 0.1f)
                && ((Time.realtimeSinceStartup - timeAtSwat) >= 0.1f) && ball.playerWithPossesion == null) {
                //Set ball in players possession
                hasBall = true;
                ball.ApplyForce(0.0f, 0.0f);
                ball.SetAngularVelocity(0.0f);
                ball.SetPlayerWithPossesion(this.gameObject);
                ball.gameObject.SetActive(false);
                ballSprite.enabled = true;
            }

        }
    }

    //Blocking shots
    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Ball")) {
            Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(), ball.physicsCollider);
            if (ball.shootingState && ball.rb.velocity.y > 0 && ((Time.realtimeSinceStartup - timeAtBlock) >= 0.3f) && ((Time.realtimeSinceStartup - timeAtShot) > 0.1f)) {
                //reverse direction of ball
                ball.ApplyForce(ball.rb.velocity.x * -2.0f, ball.rb.velocity.y * 0.8f);

                //Shake Camera for effect
                MovingCam.ShakeOnce(0.5f, 8f, .1f, 1f);

                ball.shootingState = false;
                timeAtBlock = Time.realtimeSinceStartup;
                playerInfoUI.ChangeDunkMeterValue(blockDunkMeterIncrement);
            }
        }
    }

    void DribbleBall() {
        //if the animator is not set to dribbling yet and the player is not jumping, start dribbling sound
        if (!animator.GetBool("isDribbling") && !animator.GetBool("isJumping")) {
            animator.SetBool("isDribbling", true);
            AudioManager.instance.Play("Dribbling");
        }

        ball.PositionBall(dribblePos.position.x, dribblePos.position.y);


    }

    void CancelDribble() {
        AudioManager.instance.Stop("Dribbling");

        animator.SetBool("isDribbling", false);

    }

    public void ShootBall() {

        //move ball to shot release position
        ball.gameObject.SetActive(true);
        ball.PositionBall(shotReleasePoint.position.x, shotReleasePoint.position.y);

        //Apply calculated x and y velocity components to ball
        ball.ApplyForce(CalcXspeed(0.95f), CalcYspeed());
        //set ball to shooting state
        ball.shootingState = true;

        //Apply backspin to shot depending on direction of shot
        if (this.playerTeam == 0) {
            ball.SetAngularVelocity(500.0f);
        } else if (this.playerTeam == 1) {
            ball.SetAngularVelocity(-500.0f);
        }

        hasBall = false;
        ballSprite.enabled = false;
        CancelDribble();
        ball.SetPlayerWithPossesion(null);
        hasShot = true;
        timeAtShot = Time.realtimeSinceStartup;

        //saves distance ball was shot from in order to calculate whether it was a 2 or 3 point shot
        if (!animator.GetBool("isJumping")) {
            hoop.XdistanceFromHoopAtShot = Mathf.Abs(hoop.centerBasket.position.x - this.transform.position.x);
        }

        playerInfoUI.ChangeDunkMeterValue(shotDunkMeterIncrement);
    }

    void ApplySkin() {
        characterPartSwitcher = this.GetComponent<SwitchPart>();
        characterPartSwitcher.switchParts(characterAppearanceIndices);

        SpriteRenderer[] spriteRenderers = this.GetComponentsInChildren<SpriteRenderer>();

        playerSortingOrder = 5 * this.playerInputIndex;

        for (int i = 0; i < spriteRenderers.Length; i++) {

            spriteRenderers[i].sortingOrder += playerSortingOrder;
            //Debug.Log("Index:   " + playerInputIndex);
            //Debug.Log(spriteRenderers[i].name + " | " + spriteRenderers[i].sortingOrder);
        }
    }

    void DefensiveReach() {
        Collider2D[] hitOpponents = Physics2D.OverlapCircleAll(defensiveReachPos.position, defensiveReachRange, defensiveReachLayer);

        AudioManager.instance.Play("Whack");
        animator.SetTrigger("Swat");
        foreach (Collider2D opponent in hitOpponents) {

            if (opponent.CompareTag("Player") && opponent.gameObject != gameObject) { //&& ball.playerWithPossesion != null
                if (this.transform.rotation.y < 0) {
                    opponent.GetComponent<PlayerMovement>().GetSwatted(-1f);
                } else {
                    opponent.GetComponent<PlayerMovement>().GetSwatted(1f);
                }
            }
        }

        timeAtSwat = Time.realtimeSinceStartup;
        playerInfoUI.ChangeDunkMeterValue(-swatCost);
    }


    public void GetSwatted(float direction) {
        if (hasBall) {
            ball.gameObject.SetActive(true);
            ball.PositionBall(shotReleasePoint.position.x, shotReleasePoint.position.y);
            ball.ApplyForce(1f * direction, 12f);
            hasBall = false;
            ballSprite.enabled = false;
            CancelDribble();
            ball.SetPlayerWithPossesion(null);

            if (animator.GetBool("isShooting")) {
                animator.SetBool("isShooting", false);
                shotMeter.ReleaseShot();
                hasShot = false;
            }
        }
        ApplyForce(300f * direction * rb.mass, 300f * rb.mass, 0.5f);
    }


    public void ApplyForce(float xVelocity, float yVelocity, float timerDuration) {
        //rb.velocity = new Vector2(xVelocity, yVelocity);
        rb.AddForce(new Vector2(xVelocity, yVelocity));
        smooveMove = true;
        timer.BeginTimer(timerDuration, DisableSmooveMove);
    }

    void DisableSmooveMove() {
        smooveMove = false;
    }

    public void SetXYVelocity(float xVelocity, float yVelocity) {
        rb.velocity = new Vector2(xVelocity, yVelocity);
    }

    public void StopPlayer() {
        stoppedState = true;
        i_movement = new Vector2(0f, 0f);
        //this.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
        SetXYVelocity(0f, 0f);

        jump = false;
        wasJumping = false;
        hasBall = false;
        ballSprite.enabled = false;
        hasShot = false;

        CancelDribble();
    }

    private void CreateDust() {
        dust.Play();
    }

    private void OnDrawGizmosSelected() {
        if (defensiveReachPos == null) {
            return;
        }
        Gizmos.DrawWireSphere(defensiveReachPos.position, defensiveReachRange);
    }

    public void ResetPlayerMovement() {
        StopPlayer();
        rdController.DisableRagdoll();
    }

}
