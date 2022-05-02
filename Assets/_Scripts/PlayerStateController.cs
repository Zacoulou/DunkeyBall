using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    [SerializeField] PlayerController pController;
    PlayerStates currentState;
    float deadSpeed = 0.1f;

    public enum PlayerStates {
        IDLE,
        IDLE_DRIBBLING,
        RUN_DRIBBLING,
        RUNNING,
        SPRINTING,
        SPRINTING_DRIBBLING,
        JUMPING,
        JUMPING_WITH_BALL,
        SHOOTING,
        WALLSHOOTING,
        FALLING_WITHOUT_BALL,
        FALLING_WITH_BALL,
        WALLSLIDING,
        RAGDOLL,

        NONE
    }

    public enum TriggerStates {
        LAND,
        LANDWITHBALL,
        RELEASESHOT,
        RELEASEWALLSHOT,
        SWAT,
        GET_UP_FRONT,
        GET_UP_BACK
    }

    public PlayerStates GetCurrentState() {
        return currentState;
    }

    private void SetPlayerState(PlayerStates state) {
        currentState = state;
        pController.playerAnimator.SetAnimationBasedOnPlayerState(state);
    }

    public void SetTriggerState(TriggerStates state) {
        currentState = PlayerStates.NONE;
        pController.playerAnimator.TriggerAnimationBasedOnPlayerState(state);
    }

    void FixedUpdate() {
        PlayerStates state = PlayerStates.NONE;

        //RAGDOLL
        if (pController.ragdollController.RagdollActive) {
            state = PlayerStates.RAGDOLL;
        }
        //NOT MOVING: IDLE STATE
        else if (!IsMovingX() && !IsMovingZ() && !IsMovingY()) {
            if (pController.GetBallInPossession())
                state = PlayerStates.IDLE_DRIBBLING;
            else
                state = PlayerStates.IDLE;
        }
        //JUMPING: JUMPING
        else if (!WasGrounded()) {
            if (IsWallSliding()) {
                state = PlayerStates.WALLSLIDING;

            } else {
                //TRAVELING UPWARDS
                if (YMovement() > deadSpeed) {
                    if (pController.GetBallInPossession())
                        state = PlayerStates.JUMPING_WITH_BALL;
                    else
                        state = PlayerStates.JUMPING;
                }
                //TRAVELING DOWNWARDS
                else if (YMovement() < -deadSpeed) {
                    if (pController.GetBallInPossession())
                        state = PlayerStates.FALLING_WITH_BALL;
                    else
                        state = PlayerStates.FALLING_WITHOUT_BALL;
                }
            }
        }
        //MOVING AND GROUNDED: RUNNING OR DRIBBLING
        else if ((IsMovingX() || IsMovingZ()) && WasGrounded()) {
            if (pController.GetBallInPossession()) {
                if (pController.movementController.GetIsSprinting())
                    state = PlayerStates.SPRINTING_DRIBBLING;
                else
                    state = PlayerStates.RUN_DRIBBLING;
            } else {
                if (pController.movementController.GetIsSprinting())
                    state = PlayerStates.SPRINTING;
                else
                    state = PlayerStates.RUNNING;
            }
                
        }


        //If Aiming, override previous states and set to aiming
        if (pController.shootBall.GetIsAiming()) {
            if (pController.movementController.GetIsWallSliding()) {
                state = PlayerStates.WALLSHOOTING;
            } else {
                state = PlayerStates.SHOOTING;
            }
                
        }
        SetPlayerState(state);
    }

    bool IsMovingX() {
        float xVect = pController.rb.velocity.x;
        return (xVect > deadSpeed || xVect < -deadSpeed);
    }

    bool IsMovingY() {
        float yVect = pController.rb.velocity.y;
        return (yVect > deadSpeed || yVect < -deadSpeed);
    }

    bool IsMovingZ() {
        float zVect = pController.rb.velocity.z;
        return (zVect > deadSpeed || zVect < -deadSpeed);
    }

    float YMovement() {
        return pController.rb.velocity.y;
    }

    bool WasGrounded() {
        return pController.movementController.GetGrounded();
    }

    bool IsWallSliding() {
        return pController.movementController.GetIsWallSliding();
    }
}
