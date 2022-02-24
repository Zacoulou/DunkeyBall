using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MovementController2D : MonoBehaviour {
    
    //INSPECTOR PANEL
    [SerializeField] PlayerController pController;              //Reference to PlayerController
    public UnityEvent OnLandEvent;                              //Reference to land event method
    
    //MOVEMENT VARIABLES
    Character.MovementStats movementStats;                      //Class of all player stats
    Vector2 movementVector;                                     //movement vector used to move player (accounts for speed and time)
    Vector3 m_Velocity = Vector3.zero;                          //Reference velocity vector used for smoothdamping
    bool isSprinting = false;                                   //Whether or not player is spriting
    bool canMove = true;                                        //Whether or not character is allowed to move
    [Range(0, .3f)] float m_MovementSmoothing = .05f;           // How much to smooth out the movement // 
    FacingDirection currFacingDirection = FacingDirection.RIGHT;// For determining which way the player is currently facing.

    //CONTROLLER VARIABLES    
    Vector2 rawJoystickInput;                                   //Raw joystick input
    const double joystickMovementDeadzone = 0.2f;               //Amount Player can move joystick with no registered input
    
    //SMOOVE MOVEMENT
    bool smooveMove = false;                                    //smooth movement during knockback
    const float defaultsmooveIncrement = 0.05f;                 //Default amount character input is recognized
    float smooveIncrement = defaultsmooveIncrement;             //Actual smooveIncrement used 

    //JUMP VARIABLES
    const float minimumJumpForceMultiplier = 0.5f;              //Multiplier for jump tap vs jump hold
    bool isHoldingJump = false;                                 //Whether or not the jump button is held
    bool pressingJump = false;                                  //Input state for jump button
    bool isJumping = false;                                     //If player is mid jump or not
    float variableJumpForce = 0f;                               //Global variable for determining additional jump force
    float totalJumpForce = 0f;                                  //Global variable for tracking total force used on current jump
    float timeSinceLastJumpHold = 0f;                           //Time check for determining when to apply additional force
    const float jumpTimeCheck = 0.02f;                          //Desired duration between jump checks
    const float airBorneForce = 5f;                             //Ability for character to move while airborne
    const float jumpBufferlength = 0.1f;                        //Max time before touching ground that jump is registered
    float jumpBufferTimerCheck = 0f;                            //Time stamp variable for checking duration

    //GROUND
    [SerializeField] LayerMask groundLayerMask;                 //A mask determining what is ground to the character
    [SerializeField] LayerMask OneWayPlatformLayerMask;         //A mask determining what is a one way platform to the character
    [SerializeField] Transform groundCheck;                     //A position marking where to check if the player is grounded
    Vector2 groundCheckDimensions = new Vector2(0.3f, 0.05f);   //dimensions of overlap rectangle to determine if grounded
    bool grounded;                                              //Whether or not the player is grounded.
    bool wasGrounded;                                           //Whether or not the player was grounded last frame
    const float coyoteTime = 0.1f;                              //Time after falling off ledge that you can still jump
    float lastGroundedTime = 0f;                                //Timestamp variable for measuring coyote time

    //WALL
    [SerializeField] Transform wallCheck;                       //A position marking where to check if the player is touching a wall
    Vector2 wallCheckDimensions = new Vector2(0.05f, 0.9f);     //dimensions of overlap rectangle to determine if touching a wall
    bool isTouchingWall;
    
    //WALL SLIDING
    bool isWallSliding;                                         //Whether or not the player is wallsliding
    float wallSlideSpeed = -1f;                                 //Multiplier for sliding down wall
    const float minWallHoldTime = 0.4f;                         //Minimum wall sliding time before you are allowed to fall off
    float initialWallHoldTimeStamp = 0f;                        //Timestamp variable for tracking initial wall hold
    float attemptToLeaveWallTimeStamp = 0f;                     //Timestamp variable for tracking time since player tried to leave wall
    bool canLeaveWallSlide = true;                              //Whether or not the player is allowed to leave a wallslide
    int wallTimerCheckStage = 0;                                //State for checking wall slide timers
        
    //WALL JUMP
    float wallJumpDirection = -1f;                              //Direction player should wall jump in (opposite of facing direction)
    Vector2 wallJumpAngle = new Vector2(1f, 3f);                //Angle of wall jump (This needs to be normalized before use)
    bool isWallJumping = false;                                 //Whether or not the character is currently mid walljump
    const float wallJumpDisableMovementTime = 0.05f;            //Buffer time where player cannot input any movement after wall jumping 
    
    enum FacingDirection {
        RIGHT = 0,
        LEFT = 1
    }

    public void SetMovementValues(Character.MovementStats stats) {
        movementStats = stats;
    }

    void SetPosition(Vector2 pos, FacingDirection direction) {
        this.gameObject.transform.position = pos;
        Flip(direction);
    }

    void Awake() {
        wallJumpAngle.Normalize();
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

    }

    void FixedUpdate() {
        CheckHoldingJump();
        CheckGrounded();
        CheckTouchingWall();
        CheckWallSliding();
        WallJump();
        Jump();
        Move();
    }

    // The player is grounded if a cast to the groundcheck position hits anything designated as ground
    void CheckGrounded() {
        wasGrounded = grounded;
        LayerMask acceptableGroundedLayers = groundLayerMask | OneWayPlatformLayerMask;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(groundCheck.position, groundCheckDimensions, 0, acceptableGroundedLayers);
        bool isTravelingUpwards = pController.rb.velocity.y > 0;

        if (colliders.Length > 0 && !isTravelingUpwards) {
            grounded = true;
            lastGroundedTime = Time.realtimeSinceStartup;
            isWallJumping = false;
            isJumping = false;

            if (!wasGrounded) {
                OnLandEvent.Invoke();
            }
        } else {
            grounded = false;
        }
    }

    // The player is can wallslide if a cast to the wallCheck position hits anything designated as ground
    void CheckTouchingWall() {
        isTouchingWall = false;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(wallCheck.position, wallCheckDimensions, 0, groundLayerMask);
        if (colliders.Length > 0) {
            isTouchingWall = true;
            
        }
    }

    void CheckWallSliding() {
        //Decides whether you are sliding or not
        if (isTouchingWall && !grounded && pController.rb.velocity.y < 0) {
            isWallSliding = true;
            isWallJumping = false;
            isJumping = false;
        } else {
            isWallSliding = false;
            canLeaveWallSlide = true;
            initialWallHoldTimeStamp = Time.realtimeSinceStartup;
            wallTimerCheckStage = 0;
        }

        //sets the wallslide speed
        if (isWallSliding) {
            if (wallTimerCheckStage == 0) {
                canLeaveWallSlide = false;
                wallTimerCheckStage = 1;
            }

            if ((Time.realtimeSinceStartup - initialWallHoldTimeStamp >= minWallHoldTime / 2.0f) && wallTimerCheckStage == 1) {
                wallTimerCheckStage = 2;
            }

            //Check for player input to start timer
            if (wallTimerCheckStage == 2) {
                //Same direction as wall jump
                if ((wallJumpDirection == 1 && rawJoystickInput.x >= joystickMovementDeadzone) ||
                    (wallJumpDirection == -1 && rawJoystickInput.x <= -joystickMovementDeadzone)) 
                {
                    attemptToLeaveWallTimeStamp = Time.realtimeSinceStartup;
                    wallTimerCheckStage = 3;
                }                
            }

            //prevents you from leaving the wall before end of timer
            if (wallTimerCheckStage == 3 && Time.realtimeSinceStartup - attemptToLeaveWallTimeStamp >= minWallHoldTime / 2.0f) {
                    canLeaveWallSlide = true;
            }
            
            //Controls wallslide velocity
            pController.rb.velocity = new Vector2(0f, wallSlideSpeed);
        }
    }

    void WallJump() {
        if (isWallSliding && CheckJumpBuffer() && !grounded) {
            StartCoroutine(DisableMovementForTime(wallJumpDisableMovementTime));
            isWallJumping = true;
            isHoldingJump = true;
            variableJumpForce = movementStats.jumpForce * minimumJumpForceMultiplier; ;
            totalJumpForce = variableJumpForce;
            float xForce = wallJumpAngle.x * variableJumpForce * pController.rb.mass / Time.timeScale * wallJumpDirection;
            float yForce = wallJumpAngle.y * variableJumpForce * pController.rb.mass / Time.timeScale;
            pController.rb.AddForce(new Vector2(xForce, yForce));
        }
    }

    IEnumerator DisableMovementForTime(float time) {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }



    public void Move() {

        //only control the player if grounded or airControl is turned on
        if (canMove && canLeaveWallSlide) {

            movementVector = new Vector2(rawJoystickInput.x * movementStats.movementSpeed * Time.fixedDeltaTime * 10f, 0f);

            if (isSprinting) {
                // increase the speed by the sprint multiplier
                movementVector.x *= movementStats.sprintMultiplier;
            }

            //REGULAR MOVEMENT
            if (grounded) {
                // Move the character by finding the target velocity
                Vector3 targetVelocity = new Vector2(movementVector.x, pController.rb.velocity.y);

                // And then smoothing it out and applying it to the character
                pController.rb.velocity = Vector3.SmoothDamp(pController.rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
            } 
            //AIRBORNE MOVEMENT
            else {
                //Adds force to have momentum influence movement in air
                pController.rb.AddForce(new Vector2(movementVector.x * pController.rb.mass * airBorneForce, 0f));
                if (Mathf.Abs(pController.rb.velocity.x) > Mathf.Abs(movementVector.x) && movementVector.x != 0f) {
                    Vector3 targetVelocity = new Vector2(movementVector.x, pController.rb.velocity.y);
                    pController.rb.velocity = Vector3.SmoothDamp(pController.rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
                }
            }

            //FACING DIRECTION
            if ((rawJoystickInput.x >= joystickMovementDeadzone) || (rawJoystickInput.x <= -joystickMovementDeadzone)) {
                //base direciton on input direction
                if (rawJoystickInput.x > 0 && (currFacingDirection != FacingDirection.RIGHT)) {
                    Flip(FacingDirection.RIGHT);
                } else if (rawJoystickInput.x < 0 && (currFacingDirection != FacingDirection.LEFT)) {
                    Flip(FacingDirection.LEFT);
                }
                
            } else {
                //base direction on movement direction
                if (pController.rb.velocity.x > joystickMovementDeadzone && (currFacingDirection != FacingDirection.RIGHT)) {
                    Flip(FacingDirection.RIGHT);
                } else if (pController.rb.velocity.x < -joystickMovementDeadzone && (currFacingDirection != FacingDirection.LEFT)) {
                    Flip(FacingDirection.LEFT);
                }
            }

        }
    }

    void CheckHoldingJump() {
        //Allow the jump height to vary based on how long button is held
        if (!grounded && isHoldingJump && CheckLastJumpInputTime()) {
            if (isWallJumping && pressingJump) {
                AddJumpHeight(movementStats.jumpForce, new Vector2(wallJumpAngle.x*wallJumpDirection, wallJumpAngle.y));
            } else if (pressingJump) {
                AddJumpHeight(movementStats.jumpForce, new Vector2(0f, 1f));
            }
            
            timeSinceLastJumpHold = Time.realtimeSinceStartup;
        }
    }

    bool CheckLastJumpInputTime() {
        return Time.realtimeSinceStartup - timeSinceLastJumpHold >= jumpTimeCheck;
    }

    // Switch the way the player is labelled as facing.
    private void Flip(FacingDirection direction) {
        currFacingDirection = direction;
        switch (direction) {
            case FacingDirection.LEFT:
                wallJumpDirection = 1;
                transform.Rotate(0f, 180f, 0f);
                break;

            case FacingDirection.RIGHT:
                wallJumpDirection = -1;
                transform.Rotate(0f, 180f, 0f);
                break;

            default:
                Debug.Log("Not a valid turn direction " + direction);
                break;
        }
        
        
    }


    void InitiateSmooveMove(float increment, float duration) {
        smooveMove = true;
        smooveIncrement = increment;
        Invoke("EndSmooveMove", duration);
    }

    void EndSmooveMove() {
        smooveMove = false;
        smooveIncrement = defaultsmooveIncrement;
    }

    public void SetJumpInput() {
        pressingJump = true;
        jumpBufferTimerCheck = Time.realtimeSinceStartup;
    }

    private void Jump() {
        if (CheckJumpBuffer() && !isJumping && !isWallJumping && CheckCoyoteTime()) {
            bool isMovingJoystickDown = rawJoystickInput.y <= -joystickMovementDeadzone;
            Collider2D[] oneWayColliders = Physics2D.OverlapBoxAll(groundCheck.position, groundCheckDimensions, 0, OneWayPlatformLayerMask);

            if (!isMovingJoystickDown || oneWayColliders.Length == 0) {
                isHoldingJump = true;
                isJumping = true;
                variableJumpForce = movementStats.jumpForce * minimumJumpForceMultiplier;
                totalJumpForce = variableJumpForce;
                pController.rb.AddForce(new Vector2(0f, variableJumpForce * pController.rb.mass / Time.timeScale));
            } else {
                Collider2D[] groundColliders = Physics2D.OverlapBoxAll(groundCheck.position, groundCheckDimensions, 0, groundLayerMask);
 
                //Checks if player is trying to jump down through one way platform
                if (isMovingJoystickDown && groundColliders.Length == 0 && oneWayColliders.Length > 0) {
                    isHoldingJump = false;
                    isJumping = true;
                    StartCoroutine(pController.collisionController.DisableCollisionForTime(oneWayColliders[0], 0.2f));
                    //Physics2D.IgnoreCollision(pController.collisionController.getBoxCollider2D(), );
                    Debug.Log("Fall through Platform");
                  
                }
            }

        }
    }

    bool CheckCoyoteTime() {
        return Time.realtimeSinceStartup - lastGroundedTime <= coyoteTime;
    }

    bool CheckJumpBuffer() {
        return Time.realtimeSinceStartup - jumpBufferTimerCheck <= jumpBufferlength;
    }

    public void ReleaseJump() {
        isHoldingJump = false;
        pressingJump = false;
    }

    public void AddJumpHeight(float maxForce, Vector2 normalizedJumpAngle) {

        variableJumpForce = maxForce * 0.1f;
        
        if (totalJumpForce + variableJumpForce >= maxForce) {
            variableJumpForce = maxForce - totalJumpForce;
            if (variableJumpForce < 0) {
                variableJumpForce = 0f;
            }
            isHoldingJump = false;
        }

        //Debug.Log(variableJumpForce + " | " + totalJumpForce);

        totalJumpForce += variableJumpForce;

        if (isHoldingJump) {
            float addedXForce = variableJumpForce * pController.rb.mass * normalizedJumpAngle.x;
            float addedYForce = variableJumpForce * pController.rb.mass * normalizedJumpAngle.y;
            
            pController.rb.AddForce(new Vector2(addedXForce, addedYForce));

        }
    }

    public void OnLanding() {
        if (pController.GetBallInPossession())
            pController.stateController.SetTriggerState(PlayerStateController.TriggerStates.LANDWITHBALL);
        else
            pController.stateController.SetTriggerState(PlayerStateController.TriggerStates.LAND);

        isWallJumping = false;
        isJumping = false;

        //CreateDust();
    }

    //Called once per fixedUpdate. Handles movement filter
    public void SetJoystickMovement(Vector2 movementVect) {

        //If controller input is inside deadzone, sets movement to zero
        if (movementVect.x >= -1 * joystickMovementDeadzone && movementVect.x <= joystickMovementDeadzone)
            movementVect.x = 0f;

        if (smooveMove) {
            //Account for player's current momentum
            rawJoystickInput.x = Mathf.Clamp((pController.rb.velocity.x / 4f) + movementVect.x * smooveIncrement, -1f, 1f);
        } else {
            //Use direct input
            rawJoystickInput = movementVect;
        }
    }

    public void OnStartSprint() {
        isSprinting = true;
    }

    public void OnEndSprint() {
        isSprinting = false;
    }

    public Vector2 GetMovementVector() {
        return movementVector;
    }

    public bool GetGrounded() {
        return wasGrounded;
    }

    public bool GetIsWallSliding() {
        return isWallSliding;
    }

    public bool GetIsSprinting() {
        return isSprinting;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(groundCheck.position, groundCheckDimensions);

        Gizmos.color = Color.green;
        Gizmos.DrawCube(wallCheck.position, wallCheckDimensions);
    }
}
