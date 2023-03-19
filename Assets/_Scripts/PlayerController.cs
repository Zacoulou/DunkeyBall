using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MonoBehaviour, IController
{
    //CONTROLLERS
    public MovementController3D movementController;                 //Attached class with movement mechanics
    public AppearanceController appearanceController;               //Attached class to modify character appearance
    public PlayerAnimator playerAnimator;                           //Attached class to manage player animations
    public SpriteOrderController spriteOrderController;             //Attached class to manage sprites sorting order
    public ShootBall shootBall;                                     //Attached class to manage shooting
    public PlayerStateController stateController;                   //Attached class to manage logic of player state
    public PlayerCollisionController collisionController;           //Attached class to manage logic of player state
    public RagdollController ragdollController;                     //Attached class to manage ragdoll 
    //public GrappleLauncher grappleLauncher;                        //Attached class to manage grapple hook

    //ATTACHED COMPONENTS
    public Rigidbody rb;

    //CHARACTER VARIABLES
    Character character;                                    //Scriptable object of selected character
    int team;                                               //player team (0 or 1) CHANGE TO ENUM????

    //BALL RELATED VARIABLES
    Ball ballInPossession = null;
    bool hasBall = false;
    bool canObtainBall = true;
    const float timeBetweenGettingBall = 0.5f;
    
    //HOOP RELATED VARIABLES
    HoopController assignedHoop = null;                                      //Hoop script


    void Awake() {
    }

    public void InitializePlayerController(Character character) {
        appearanceController.SetAppearanceValues(character.appearanceDetails);
        ragdollController.InitializeRagdollJoints(appearanceController.GetHasTail());
        movementController.SetMovementValues(character.movementStats);
    }

    // Start is called before the first frame update
    void Start() {
        SetHasBall(false);
    }

    // Update is called once per frame
    void Update() {

    }

    // Called every fixed framerate frame
    void FixedUpdate() {
    }

    public void SetPosition(Vector3 pos) {
        gameObject.transform.position = pos;
    }

    public Vector3 GetPosition() {
        return gameObject.transform.position;
    }

    public void SetHoop(HoopController hoop) {
        assignedHoop = hoop;
    }

    public HoopController GetHoop() {
        return assignedHoop;
    }

    //Set ball in players possession
    public void ObtainBall(Ball ball) {
        ballInPossession = ball;
        SetHasBall(true);
        ball.SetPlayerWithPossesion(gameObject);
        ball.gameObject.SetActive(false);
    }

    public void SetHasBall(bool state) {
        appearanceController.SetBallVisible(state);
        hasBall = state;
        canObtainBall = false;

        //Add a delay before player can get the ball again if you are taking it away
        if (!state) {
            ballInPossession = null;
            Invoke("CheckCanObtainBall", timeBetweenGettingBall);
        }

    }

    private void CheckCanObtainBall() {
        //TODO: ADD ANY CONDITIONS TO CHECK IF PLAYER CAN GET BALL
        canObtainBall = true;
    }

    public bool GetCanObtainBall() {
        return canObtainBall;
    }

    public Ball GetBallInPossession() {
        return ballInPossession;
    }

    void Swat() {

    }

    void Dribble() {

    }

    void SpecialAbility() {

    }

    //-------------------------------------------------------------------------------------------
    //----------------------------------Control Handling ----------------------------------------
    //-------------------------------------------------------------------------------------------

    //Accepts input from UserInputHandler
    public void onLeftJoystickMovement(CallbackContext context) {
        movementController.SetJoystickMovement(context.ReadValue<Vector2>());
    }

    //Accepts input without controller, such as from AI
    public void onLeftJoystickMovement(Vector2 movementVect) {
        movementController.SetJoystickMovement(movementVect);
    }

    public void onPressButtonEast() {
        movementController.NotifyRagdollActivated();
        ragdollController.ActivateRagdoll(rb.velocity, 1f);
    }

    public void onPressButtonNorth() {
    }

    //Pressing jump button
    public void onPressButtonSouth() {
        movementController.SetJumpInput();
    }

    public void onPressButtonWest() {
        shootBall.OnStartShot();
    }

    public void onPressLeftTrigger() {
        
    }

    public void onPressRightTrigger() {
        //TODO: Check if you have required stamina
        movementController.OnStartSprint();   
    }

    public void onReleaseButtonEast() {
    }

    public void onReleaseButtonNorth() {
        
    }

    public void onReleaseButtonSouth() {
        movementController.ReleaseJump();
    }

    public void onReleaseButtonWest() {
        shootBall.ReleaseShot();
    }

    public void onReleaseLeftTrigger() {
        
    }

    public void onReleaseRightTrigger() {
        movementController.OnEndSprint();
    }

    public void onPressRightBumper() {
        //grappleLauncher.SetGrapplePoint();
    }

    public void onReleaseRightBumper() {
        //grappleLauncher.CancelGrapple();
    }

    public void onPressLeftBumper() {
    }

    public void onReleaseLeftBumper() {
    }
}
