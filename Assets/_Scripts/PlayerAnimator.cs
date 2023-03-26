using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] PlayerController pController;
    [SerializeField] Animator animator;
    bool canUpdate = true;
    Dictionary<int, float> clipLengthDict = new Dictionary<int, float>();
    private bool isCoroutineExecuting = false;

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
                    PlayAnimation(LEGS_IDLE, Layer.LEGS, false);
                    PlayAnimation(BODY_IDLE, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.IDLE_DRIBBLING:
                    PlayAnimation(LEGS_IDLE, Layer.LEGS, false);
                    PlayAnimation(BODY_DRIBBLE, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.RUN_DRIBBLING:
                    PlayAnimation(LEGS_RUN, Layer.LEGS, false);
                    PlayAnimation(BODY_DRIBBLE, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.RUNNING:
                    PlayAnimation(LEGS_RUN, Layer.LEGS, false);
                    PlayAnimation(BODY_RUN, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.SPRINTING:
                    PlayAnimation(LEGS_SPRINTING, Layer.LEGS, false);
                    PlayAnimation(BODY_SPRINTING, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.SPRINTING_DRIBBLING: //TODO: FINISH THIS
                    PlayAnimation(LEGS_SPRINTING, Layer.LEGS, false);
                    PlayAnimation(BODY_SPRINTING_DRIBBLING, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.JUMPING:
                    PlayAnimation(LEGS_JUMP, Layer.LEGS, false);
                    PlayAnimation(BODY_JUMP, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.JUMPING_WITH_BALL:
                    PlayAnimation(LEGS_JUMP, Layer.LEGS, false);
                    PlayAnimation(BODY_HOLDBALL, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.SHOOTING:
                    PlayAnimation(BODY_RAISESHOT, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.WALLSHOOTING:
                    PlayAnimation(BODY_AIM_WALL_SHOT, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.FALLING_WITHOUT_BALL:
                    PlayAnimation(LEGS_FALLING, Layer.LEGS, false);
                    PlayAnimation(BODY_FALLING, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.FALLING_WITH_BALL:
                    PlayAnimation(LEGS_FALLING, Layer.LEGS, false);
                    PlayAnimation(BODY_HOLDBALL, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.WALLSLIDING:
                    PlayAnimation(LEGS_WALL_SLIDE, Layer.LEGS, false);
                    PlayAnimation(BODY_WALL_SLIDE, Layer.BODY, false);
                    break;

                case PlayerStateController.PlayerStates.RAGDOLL:
                    break;

                default:
                    break;
            }
        }
    }

    public void TriggerAnimationBasedOnPlayerState(PlayerStateController.TriggerStates state, Action onComplete = null) {
        switch (state) {
            case PlayerStateController.TriggerStates.LAND:
                PlayAnimation(LEGS_LANDING, Layer.LEGS, true, onComplete);
                PlayAnimation(BODY_LANDING, Layer.BODY, false, onComplete);
                break;

            case PlayerStateController.TriggerStates.LANDWITHBALL:
                PlayAnimation(LEGS_LANDING, Layer.LEGS, true, onComplete);
                PlayAnimation(BODY_LANDING_WITH_BALL, Layer.BODY, false, onComplete);
                break;

            case PlayerStateController.TriggerStates.RELEASESHOT:
                PlayAnimation(BODY_RELEASESHOT, Layer.BODY, true, onComplete);
                break;

            case PlayerStateController.TriggerStates.RELEASEWALLSHOT:
                PlayAnimation(BODY_RELEASE_WALL_SHOT, Layer.BODY, true, onComplete);
                break;

            case PlayerStateController.TriggerStates.SWAT:
                PlayAnimation(BODY_SWAT, Layer.BODY, true, onComplete);
                break;

            case PlayerStateController.TriggerStates.GET_UP_FRONT:
                PlayAnimation(BODY_GETUPFRONT, Layer.BODY, true, onComplete);
                break;

            case PlayerStateController.TriggerStates.GET_UP_BACK:
                PlayAnimation(BODY_GETUPBACK, Layer.BODY, true, onComplete);
                break;

            default:
                break;
        }
    }

    void StopAnimationUpdates(int stateHash, Action onComplete = null) {
        canUpdate = false;
        float delay = clipLengthDict[stateHash];
        Invoke("ContinueAnimationUpdates", delay);
        
        if (onComplete != null)
        {
            StartCoroutine(ExecuteAfterTime(delay, onComplete));
        }
    }

    private IEnumerator ExecuteAfterTime(float time, Action task)
    {
        if (isCoroutineExecuting)
            yield break;

        isCoroutineExecuting = true;
        yield return new WaitForSeconds(time);
        task();
        isCoroutineExecuting = false;
    }

    void ContinueAnimationUpdates() {
        canUpdate = true;
    }

    // Plays an animation and calls respective callback on complete if provided
    void PlayAnimation(int HashID, Layer layer, bool waitForFinish, Action onComplete = null) {
        animator.Play(HashID, (int)layer);
        if (waitForFinish) {
            StopAnimationUpdates(HashID, onComplete);
        }
    }
}
