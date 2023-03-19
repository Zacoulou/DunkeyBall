using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MovementController3D : MonoBehaviour {
    
    //INSPECTOR PANEL
    [SerializeField] PlayerController pController;              //Reference to PlayerController
    public UnityEvent OnLandEvent;                              //Reference to land event method
    
    //MOVEMENT VARIABLES
    Character.MovementStats movementStats;                      //Class of all player stats
    Vector3 movementVector;                                     //movement vector used to move player (accounts for speed and time)
    Vector3 m_Velocity = Vector3.zero;                          //Reference velocity vector used for smoothdamping
    bool isSprinting = false;                                   //Whether or not player is spriting
    bool canMove = true;                                        //Whether or not character is allowed to move
    [Range(0, .3f)] float m_MovementSmoothing = .05f;           // How much to smooth out the movement // 
    FacingDirection currFacingDirection = FacingDirection.RIGHT;// For determining which way the player is currently facing.

    //CONTROLLER VARIABLES    
    Vector2 rawJoystickInput;                                   //Raw joystick input
    const double joystickMovementDeadzone = 0.2f;               //Amount Player can move joystick with no registered input
    bool registerPlayerMovementInput = true;                    //Whether or not the player's movement input should be ignored or not
    
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
    [SerializeField] float groundCheckRadius;                   //dimensions of overlap rectangle to determine if grounded
    bool grounded;                                              //Whether or not the player is grounded.
    bool wasGrounded;                                           //Whether or not the player was grounded last frame
    const float coyoteTime = 0.1f;                              //Time after falling off ledge that you can still jump
    float lastGroundedTime = 0f;                                //Timestamp variable for measuring coyote time

    //WALL
    [SerializeField] Transform wallCheck;                       //A position marking where to check if the player is touching a wall
    [SerializeField] Vector3 wallCheckBox;                      //dimensions of overlap rectangle to determine if touching a wall
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
    float inverseMovementDir = -1f;                             //Direction player should wall jump in (opposite of facing direction)
    Vector2 wallJumpAngle = new Vector2(1f, 3f);                //Angle of wall jump (This needs to be normalized before use)
    bool isWallJumping = false;                                 //Whether or not the character is currently mid walljump
    const float wallJumpDisableMovementTime = 0.05f;            //Buffer time where player cannot input any movement after wall jumping 

    //SLOPE MOVEMENT
    float slopeAngle = 0f;
    float groundRayDistance = 1;
    RaycastHit slopeHit;
    float minGroundAngle = 1f;
    float maxGroundAngle = 40f;

    //Character Rotation
    [SerializeField] private Transform SpritesTransform;
    const float leftRot = 180f;
    const float rightRot = 0f;
    float currRot = 0f;
    FacingDirection previousFacingDirection = FacingDirection.RIGHT;

    const float rotateTime = 0.05f;
    const float minRotation = -15f;
    const float maxRotation = 15f;


    public enum FacingDirection {
        RIGHT = 0,
        LEFT = 1
    }

    public void SetMovementValues(Character.MovementStats stats) {
        movementStats = stats;
    }

    void SetPositionAndDirection(Vector2 pos, FacingDirection direction) {
        gameObject.transform.position = pos;
        Flip(direction);
    }

    void Awake() {
        wallJumpAngle.Normalize();
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

    }

    void FixedUpdate() {
        if (registerPlayerMovementInput) {
            CheckHoldingJump();
            CheckGrounded();
            CheckTouchingWall();
            CheckWallSliding();
            WallJump();
            Move();
            RotateToFaceMovementDirection();
        }
        CheckJump();
        
    }

    // The player is grounded if a cast to the groundcheck position hits anything designated as ground
    void CheckGrounded() {
        wasGrounded = grounded;
        LayerMask acceptableGroundedLayers = groundLayerMask | OneWayPlatformLayerMask;

        Collider[] colliders = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, acceptableGroundedLayers);

        if (colliders.Length > 0 ){
            grounded = true;
            lastGroundedTime = Time.realtimeSinceStartup;
            isWallJumping = false;
            isJumping = false;
            measureSlopeAngle();

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
        Collider[] colliders = Physics.OverlapBox(wallCheck.position, wallCheckBox, Quaternion.identity, groundLayerMask);
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
                if ((inverseMovementDir == 1 && rawJoystickInput.x >= joystickMovementDeadzone) ||
                    (inverseMovementDir == -1 && rawJoystickInput.x <= -joystickMovementDeadzone)) 
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
            float xForce = wallJumpAngle.x * variableJumpForce * pController.rb.mass / Time.timeScale * inverseMovementDir;
            float yForce = wallJumpAngle.y * variableJumpForce * pController.rb.mass / Time.timeScale;
            pController.rb.AddForce(new Vector2(xForce, yForce));
        }
    }

    IEnumerator DisableMovementForTime(float time) {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    float measureSlopeAngle() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, (pController.collisionController.getCapsuleCollider().height) / 2 + groundRayDistance)) {
            slopeAngle = Vector3.Angle(slopeHit.normal, Vector3.up);
        }
        return slopeAngle;
    }

    Vector3 getSlopeDirection(Vector3 moveVector) {
        moveVector = new Vector3(moveVector.x, 0f, moveVector.z);
        Vector3 slopeVector = Vector3.ProjectOnPlane(moveVector, slopeHit.normal);

        return slopeVector;
    }

    Vector3 adjustMovementForSlope(Vector3 moveVector) {
        Vector3 resultant = moveVector;
        
        //ADJUST movement to align with slope
        Vector3 slopeVector = getSlopeDirection(moveVector);

        //Get slopes in XY plane and ZY plane
        float XYangle = Mathf.Atan2(slopeVector.y, Mathf.Abs(slopeVector.x));
        float ZYangle = Mathf.Atan2(slopeVector.y, Mathf.Abs(slopeVector.z));

        // Flat ground is seen as 90 degrees but should be zero, so translate it to zero if between 89 - 90 degrees
        if (isInRange(XYangle, 1.55334f, 1.58825f)) { XYangle = 0f; }
        if (isInRange(ZYangle, 1.55334f, 1.58825f)) { ZYangle = 0f; }

        //The Y velocity only cares about which angle is greater, so base Y on larger values
        float largerAngle = ZYangle >= XYangle ? ZYangle : XYangle;
        float yMovementBasis = XYangle >= ZYangle ? movementVector.x : movementVector.z;

        //Set X, Y, Z magnitudes based on the slope
        resultant.y += Mathf.Abs(yMovementBasis) * Mathf.Sin(largerAngle);
        resultant.x *= Mathf.Cos(XYangle);
        resultant.z *= Mathf.Cos(ZYangle);

        return resultant;
    }

    bool isInRange(float value, float lowerBound, float upperBound) {
        return value >= lowerBound && value <= upperBound;
    }
    
    public void Move() {

        //only control the player if grounded or airControl is turned on
        if (canMove && canLeaveWallSlide) {
            float x_Vector = rawJoystickInput.x * movementStats.movementSpeed * Time.fixedDeltaTime * 10f;
            float z_Vector = rawJoystickInput.y * movementStats.movementSpeed * Time.fixedDeltaTime * 10f;
            movementVector = new Vector3(x_Vector, 0.0f, z_Vector);

            if (isSprinting) {
                // increase the speed by the sprint multiplier
                Vector2 clampedJoystick = Vector2.ClampMagnitude(rawJoystickInput, 1f).normalized;
                Vector3 transformedClampedJoystick = new Vector3(clampedJoystick.x, 0f, clampedJoystick.y);
                movementVector.x += transformedClampedJoystick.x * movementStats.sprintMultiplier;
                movementVector.z += transformedClampedJoystick.z * movementStats.sprintMultiplier;
            }

            //Adjust movement based on the slope the character is on to allow for same speed regardless of slope
            if (isInRange(slopeAngle, minGroundAngle, maxGroundAngle)) {
                movementVector = adjustMovementForSlope(movementVector);
            } else if (slopeAngle >= maxGroundAngle){
                //SLIDE
                movementVector = getSlopeDirection(movementVector).normalized * -3f;
            }

            //REGULAR MOVEMENT
            if (grounded) {
                // Move the character by finding the target velocity
                Vector3 targetVelocity = new Vector3(movementVector.x, pController.rb.velocity.y + movementVector.y, movementVector.z);

                // And then smoothing it out and applying it to the character
                pController.rb.velocity = Vector3.SmoothDamp(pController.rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            } 
            //AIRBORNE MOVEMENT
            else {
                //Adds force to have momentum influence movement in air
                float movementForce = pController.rb.mass * airBorneForce;
                pController.rb.AddForce(new Vector3(movementVector.x * movementForce, 0f, movementVector.z * movementForce));
                if ((Mathf.Abs(pController.rb.velocity.x) > Mathf.Abs(movementVector.x) && movementVector.x != 0f)
                    || (Mathf.Abs(pController.rb.velocity.z) > Mathf.Abs(movementVector.z) && movementVector.z != 0f)) {
                    Vector3 targetVelocity = new Vector3(movementVector.x, pController.rb.velocity.y, movementVector.z);
                    pController.rb.velocity = Vector3.SmoothDamp(pController.rb.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
                }
            }

            //FACING DIRECTION
            if (pController.stateController.GetCurrentState() == PlayerStateController.PlayerStates.SHOOTING) {
                //base direction on relation to hoop
                if (GetXSideOfHoop() == 1) { Flip(FacingDirection.RIGHT); }
                else if (GetXSideOfHoop() == -1) { Flip(FacingDirection.LEFT); }
            } else {
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
    }

    void CheckHoldingJump() {
        //Allow the jump height to vary based on how long button is held
        if (!grounded && isHoldingJump && CheckLastJumpInputTime()) {
            if (isWallJumping && pressingJump) {
                AddJumpHeight(movementStats.jumpForce, new Vector2(wallJumpAngle.x*inverseMovementDir, wallJumpAngle.y));
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
        if (currFacingDirection != direction) {
            currFacingDirection = direction;
            switch (direction) {
                case FacingDirection.LEFT:
                    inverseMovementDir = 1;
                    currRot = leftRot;
                    transform.eulerAngles = new Vector3(0f, leftRot, 0f);
                    break;

                case FacingDirection.RIGHT:
                    inverseMovementDir = -1;
                    currRot = rightRot;
                    transform.eulerAngles = new Vector3(0f, rightRot, 0f);
                    break;
            }
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

    private void CheckJump() {
        //RECOVER FROM RAGDOLL
        if (CheckJumpBuffer() && pController.ragdollController.RagdollActive && pController.ragdollController.CheckRagDollBuffer()) {
            RagdollOrientation ragdollOrientation = pController.ragdollController.GetRagdollOrientation();
            pController.ragdollController.DisableRagdoll();
            
            SetRegisterPlayerMovementInput(true);
            PerformJump();

            //Determine which orientation to get up
            switch (ragdollOrientation) {
                case RagdollOrientation.LEANING_BACKWARD:
                    pController.stateController.SetTriggerState(PlayerStateController.TriggerStates.GET_UP_BACK);
                    break;
                case RagdollOrientation.LEANING_FORWARD:
                    pController.stateController.SetTriggerState(PlayerStateController.TriggerStates.GET_UP_FRONT);
                    break;
            }

        }
        //REGULAR JUMP
        else if (CheckJumpBuffer() && !isJumping && !isWallJumping && CheckCoyoteTime()) {
            PerformJump();
        }
    }

    private void PerformJump() {
        isHoldingJump = true;
        isJumping = true;
        variableJumpForce = movementStats.jumpForce * minimumJumpForceMultiplier;
        totalJumpForce = variableJumpForce;
        pController.rb.velocity = new Vector3(pController.rb.velocity.x, 0f, pController.rb.velocity.z);
        pController.rb.AddForce(new Vector2(0f, variableJumpForce * pController.rb.mass / Time.timeScale));
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

        if (movementVect.y >= -1 * joystickMovementDeadzone && movementVect.y <= joystickMovementDeadzone)
            movementVect.y = 0f;

        if (smooveMove) {
            //Account for player's current momentum
            rawJoystickInput.x = Mathf.Clamp((pController.rb.velocity.x / 4f) + movementVect.x * smooveIncrement, -1f, 1f);
            rawJoystickInput.y = Mathf.Clamp((pController.rb.velocity.z / 4f) + movementVect.y * smooveIncrement, -1f, 1f);
        } else {
            //Use direct input
            rawJoystickInput = movementVect;
        }
    }

    void RotateToFaceMovementDirection() {
        float rotTarget = 0f;

        //In shooting state, rotate character to face hoop
        if (pController.stateController.GetCurrentState() == PlayerStateController.PlayerStates.SHOOTING) {
            Vector3 distanceToHoop = gameObject.transform.position - pController.GetHoop().GetHoopCenterPosition();
            rotTarget = Mathf.Clamp(Mathf.Atan(distanceToHoop.z / distanceToHoop.x) * -Mathf.Rad2Deg, minRotation, maxRotation);
        } else {
            //Player is moving in any direction
            if (movementVector.x != 0f || movementVector.z != 0f) {
                rotTarget = Mathf.Clamp(Mathf.Atan(movementVector.z / movementVector.x) * -Mathf.Rad2Deg, minRotation, maxRotation);
            }
            //Invert rotation when no xVelocity and player is facing left, because otherwise it assumes facing right
            if (movementVector.x == 0f && currFacingDirection == FacingDirection.LEFT) {
                rotTarget *= -1f;
            }
        }

        if (previousFacingDirection != currFacingDirection) {
            previousFacingDirection = currFacingDirection;
            SpritesTransform.eulerAngles = new Vector3(0f, currRot + rotTarget, 0f);
        } else {
            float trueTarget = rotTarget + currRot;
            if (trueTarget < 0f && rotTarget < 0f) {
                trueTarget = 360f + rotTarget;
            }

            float y = CustomMath.LerpThrough360Degrees(SpritesTransform.eulerAngles.y, trueTarget, Time.deltaTime * 20f);

            SpritesTransform.eulerAngles = new Vector3(0f, y, 0f);
        }

    }

    public void SetRegisterPlayerMovementInput(bool state) {
        registerPlayerMovementInput = state;
    }

    public void NotifyRagdollActivated()
    {
        isWallJumping = false;
        SetRegisterPlayerMovementInput(false);
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

    public FacingDirection GetCurrentFacingDirection() {
        return currFacingDirection;
    }

    //Returns 1 if player is to the left, or -1 if player is to the right of the hoop
    private int GetXSideOfHoop() {
        int side = 1;
        if (pController.GetHoop().GetHoopCenterPosition().x - gameObject.transform.position.x < 0) { side = -1; }
        return side;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawCube(wallCheck.position, wallCheckBox);
    }
}
