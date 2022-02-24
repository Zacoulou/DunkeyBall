using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class UserInputHandler : User {
    PlayerControls controls;
    GameObject player;
    PlayerController pController;
    int designatedTeam;
    PlayerInput playerInput;
    CharacterFactory characterFactory;

    //First Method called upon creation
    public void InitializeUserInputHandler(PlayerInput input, Character character){
        controls = new PlayerControls();
        characterFactory = GetComponent<CharacterFactory>();
        playerInput = input;
        player = characterFactory.createCharacter(character);
        playerInput.onActionTriggered += Input_onActionTriggered;
        pController = player.GetComponent<PlayerController>();
    }

    public override PlayerController playerController {
        get {
            return pController;
        }
        set { }
    }

    public override int playerIndex {
        get {
            return playerInput.playerIndex;
        }
        set { }
    }

    private void Input_onActionTriggered(CallbackContext obj) {
        //Debug.Log("onactionTriggered:  " + obj.action.name + " | " + obj.action.ReadValueAsObject().ToString());

        //Only use player inputs when game is not paused
        if (!PauseMenu.gameIsPaused) {
            if (obj.action.name == controls.Gameplay.Move.name) {
                pController.onLeftJoystickMovement(obj);
            }

            if (obj.action.name == controls.Gameplay.Shoot.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                pController.onPressButtonWest();
            }

            if (obj.action.name == controls.Gameplay.Shoot.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
            {
                pController.onReleaseButtonWest();
            }

            if (obj.action.name == controls.Gameplay.Jump.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                pController.onPressButtonSouth();
            }

            if (obj.action.name == controls.Gameplay.Jump.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
            {
                pController.onReleaseButtonSouth();
            }

            if (obj.action.name == controls.Gameplay.Swat.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                pController.onPressButtonEast();
            }

            if (obj.action.name == controls.Gameplay.Sprint.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                pController.onPressRightTrigger();
            }

            if (obj.action.name == controls.Gameplay.Sprint.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
            {
                pController.onReleaseRightTrigger();
            }

            if (obj.action.name == controls.Gameplay.R_Bumper.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                pController.onPressRightBumper();
            }

            if (obj.action.name == controls.Gameplay.R_Bumper.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
            {
                pController.onReleaseRightBumper();
            }

            if (obj.action.name == controls.Gameplay.L_Bumper.name && obj.ReadValueAsButton())// obj.ReadValueAsButton when only want button down
            {
                pController.onPressLeftBumper();
            }

            if (obj.action.name == controls.Gameplay.L_Bumper.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
            {
                pController.onReleaseLeftBumper();
            }
        }

        // Pause Button
        if (obj.action.name == controls.Gameplay.StartButton.name && !obj.ReadValueAsButton())// !obj.ReadValueAsButton when only want button up
        {
            PauseMenu.Instance.PauseResumeManager();
        }
    }
}
