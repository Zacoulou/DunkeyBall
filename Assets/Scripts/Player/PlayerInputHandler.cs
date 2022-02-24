using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour {
    private PlayerConfiguration playerConfig;
    private PlayerMovement mover;
    [SerializeField] private PlayerControls controls;

    private void Awake() {
        mover = GetComponent<PlayerMovement>();
        controls = new PlayerControls();
    }

    public void InitializePlayer(PlayerConfiguration pc) {
        playerConfig = pc;
        mover.playerTeam = playerConfig.PlayerTeam; //Sets player's team
        mover.playerInputIndex = playerConfig.PlayerIndex;
        mover.characterAppearanceIndices = playerConfig.appearanceIndices; //sets Player's custom appearance
        mover.playerColor = playerConfig.playerColor;
        playerConfig.Input.onActionTriggered += Input_onActionTriggered;
    }

    private void Input_onActionTriggered(CallbackContext obj) {
        //Debug.Log("onactionTriggered:  " + obj.action.name + " | " + obj.action.ReadValueAsObject().ToString());


        //Only use player inputs when game is not paused
        if (!PauseMenu.gameIsPaused) {
            if (obj.action.name == controls.Gameplay.Move.name) {
                mover.OnSetMovement(obj);
            }

            if (obj.action.name == controls.Gameplay.Shoot.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                mover.OnStartShot();
            }

            if (obj.action.name == controls.Gameplay.Shoot.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
            {
                mover.OnReleaseShot();
            }

            if (obj.action.name == controls.Gameplay.Jump.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                mover.OnJump();
            }

            if (obj.action.name == controls.Gameplay.Jump.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
            {
                mover.OnJumpRelease();
            }

            if (obj.action.name == controls.Gameplay.Swat.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                mover.OnSwat();
            }

            if (obj.action.name == controls.Gameplay.Sprint.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                mover.OnStartSprint();
            }

            if (obj.action.name == controls.Gameplay.Sprint.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
            {
                mover.OnEndSprint();
            }
        }

        


        // Pause Button
        if (obj.action.name == controls.Gameplay.StartButton.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
        {
            PauseMenu.Instance.PauseResumeManager();
        }
    }

}
