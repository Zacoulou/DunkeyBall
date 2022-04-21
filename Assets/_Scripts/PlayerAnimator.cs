using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] PlayerController pController;
    [SerializeField] Animator animator;
    bool canUpdate = true;
    Dictionary<int, float> clipLengthDict = new Dictionary<int, float>();

    //FULL BODY
    
    int BODY_GETUPBACK = Animator.StringToHash("Get_Up_Back");
    int BODY_GETUPFRONT = Animator.StringToHash("Get_Up_Front");

    //BODY
    int BODY_IDLE = Animator.StringToHash("Body_Idle");
    int BODY_RUN = Animator.StringToHash("Body_Run");
    int BODY_SPRINTING = Animator.StringToHash("Body_Sprint");
    int BODY_DRIBBLE = Animator.StringToHash("Body_Dribble");
    int BODY_SPRINTING_DRIBBLING = Animator.StringToHash("Body_Sprint_Dribble");
    int BODY_FALLING = Animator.StringToHash("Body_Falling");
    int BODY_HOLDBALL = Animator.StringToHash("Body_HoldBall");
    int BODY_RAISESHOT = Animator.StringToHash("Body_RaiseShot");
    int BODY_HOLDSHOT = Animator.StringToHash("Body_HoldShot");
    int BODY_RELEASESHOT = Animator.StringToHash("Body_ReleaseShot");
    int BODY_JUMP = Animator.StringToHash("Body_Jump");
    int BODY_LANDING = Animator.StringToHash("Body_Landing");
    int BODY_LANDING_WITH_BALL = Animator.StringToHash("Body_Landing_With_Ball");
    int BODY_SWAT = Animator.StringToHash("Body_Swat");
    int BODY_WALL_SLIDE = Animator.StringToHash("Body_Wall_Slide");
    int BODY_AIM_WALL_SHOT = Animator.StringToHash("Wall_Slide_Aim_Shot");
    int BODY_RELEASE_WALL_SHOT = Animator.StringToHash("Wall_Slide_Release_Shot");


    //LEGS
    int LEGS_IDLE = Animator.StringToHash("Legs_Idle");
    int LEGS_FALLING = Animator.StringToHash("Legs_Falling");
    int LEGS_JUMP = Animator.StringToHash("Legs_Jump");
    int LEGS_LANDING = Animator.StringToHash("Legs_Landing");
    int LEGS_RUN = Animator.StringToHash("Legs_Run");
    int LEGS_SPRINTING = Animator.StringToHash("Legs_Sprint");
    int LEGS_WALL_SLIDE = Animator.StringToHash("Legs_Wall_Slide");

    enum Layer{
        BODY = 0,
        LEGS = 1
    }

    private void Awake() {
        SetClipLengthDict();
    }

    //Sets Dictionary with hashID and respective clip duration
    void SetClipLengthDict() {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips) {
            clipLengthDict.Add(Animator.StringToHash(clip.name), clip.length);
        }
    }

    public void SetAnimationBasedOnPlayerState(PlayerStateController.PlayerStates state) {
        if (canUpdate) {
            switch (state) {
                case PlayerStateController.PlayerStates.IDLE:
                    PlayAnimation(LEGS_IDLE, Layer.LEGS);
                    PlayAnimation(BODY_IDLE, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.IDLE_DRIBBLING:
                    PlayAnimation(LEGS_IDLE, Layer.LEGS);
                    PlayAnimation(BODY_DRIBBLE, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.RUN_DRIBBLING:
                    PlayAnimation(LEGS_RUN, Layer.LEGS);
                    PlayAnimation(BODY_DRIBBLE, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.RUNNING:
                    PlayAnimation(LEGS_RUN, Layer.LEGS);
                    PlayAnimation(BODY_RUN, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.SPRINTING:
                    PlayAnimation(LEGS_SPRINTING, Layer.LEGS);
                    PlayAnimation(BODY_SPRINTING, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.SPRINTING_DRIBBLING: //TODO: FINISH THIS
                    PlayAnimation(LEGS_SPRINTING, Layer.LEGS);
                    PlayAnimation(BODY_SPRINTING_DRIBBLING, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.JUMPING:
                    PlayAnimation(LEGS_JUMP, Layer.LEGS);
                    PlayAnimation(BODY_JUMP, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.JUMPING_WITH_BALL:
                    PlayAnimation(LEGS_JUMP, Layer.LEGS);
                    PlayAnimation(BODY_HOLDBALL, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.SHOOTING:
                    PlayAnimation(BODY_RAISESHOT, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.WALLSHOOTING:
                    PlayAnimation(BODY_AIM_WALL_SHOT, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.FALLING_WITHOUT_BALL:
                    PlayAnimation(LEGS_FALLING, Layer.LEGS);
                    PlayAnimation(BODY_FALLING, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.FALLING_WITH_BALL:
                    PlayAnimation(LEGS_FALLING, Layer.LEGS);
                    PlayAnimation(BODY_HOLDBALL, Layer.BODY);
                    break;

                case PlayerStateController.PlayerStates.WALLSLIDING:
                    PlayAnimation(LEGS_WALL_SLIDE, Layer.LEGS);
                    PlayAnimation(BODY_WALL_SLIDE, Layer.BODY);
                    break;

                default:
                    break;
            }
        }
    }

    public void TriggerAnimationBasedOnPlayerState(PlayerStateController.TriggerStates state) {
        switch (state) {
            case PlayerStateController.TriggerStates.LAND:
                PlayAnimation(LEGS_LANDING, Layer.LEGS, true);
                PlayAnimation(BODY_LANDING, Layer.BODY);
                break;

            case PlayerStateController.TriggerStates.LANDWITHBALL:
                PlayAnimation(LEGS_LANDING, Layer.LEGS, true);
                PlayAnimation(BODY_LANDING_WITH_BALL, Layer.BODY);
                break;

            case PlayerStateController.TriggerStates.RELEASESHOT:
                PlayAnimation(BODY_RELEASESHOT, Layer.BODY, true);
                break;

            case PlayerStateController.TriggerStates.RELEASEWALLSHOT:
                PlayAnimation(BODY_RELEASE_WALL_SHOT, Layer.BODY, true);
                break;

            case PlayerStateController.TriggerStates.SWAT:
                PlayAnimation(BODY_SWAT, Layer.BODY, true);
                break;

            case PlayerStateController.TriggerStates.GET_UP_FRONT:
                PlayAnimation(BODY_GETUPFRONT, Layer.BODY, true);
                break;

            case PlayerStateController.TriggerStates.GET_UP_BACK:
                PlayAnimation(BODY_GETUPBACK, Layer.BODY, true);
                break;

            default:
                break;
        }
    }

    void StopAnimationUpdates(int stateHash) {
        canUpdate = false;
        float delay = clipLengthDict[stateHash];
        Invoke("ContinueAnimationUpdates", delay);
    }

    void ContinueAnimationUpdates() {
        canUpdate = true;
    }

    
    void PlayAnimation(int HashID, Layer layer, bool waitForFinish = false) {
        animator.Play(HashID, (int)layer);
        if (waitForFinish) {
            StopAnimationUpdates(HashID);
        }
    }


    //VARIABLES
    //Vector2 moveVector = new Vector2(0f, 0f);
    //bool isJumping = false;
    //bool hasBall = false;
    //bool isDribbling = false;
    //bool isShooting = false;

    //Triggers
    //Swat
    //Get up front
    //Get up back

    //public enum LegStates {
    //    IDLE,
    //    RUNNING,
    //    JUMPING,
    //    FALLING,
    //    LANDING
    //}

    //public enum BodyStates {
    //    IDLE,
    //    RUNNING,
    //    JUMPING,
    //    FALLING,
    //    LANDING,
    //    DRIBBLING,
    //    HOLDBALL,
    //    HOLDSHOT,
    //    RAISESHOT,
    //    RELEASESHOT,
    //    SWAT,
    //    GETUPBACK,
    //    GETUPFRONT
    //}

    //public void SetBodyState(BodyStates state) {
    //    PlayAnimation(BODY_RUN, Layer.BODY);
    //    PlayAnimation(LEGS_RUN, Layer.LEGS);
    //}

    //public void SetLegState(LegStates state) {
    //    PlayAnimation(LEGS_RUN, Layer.LEGS);
    //}
}
