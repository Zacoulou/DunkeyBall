using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UsersManager : MonoBehaviour
{
    public Character tempCharacterOption;
    public GameObject tempHoop;

    int MaxPlayers = 4;
    Dictionary<int, GameObject> usersGameObjects = new Dictionary<int, GameObject>();

    public void HandlePlayerJoined(PlayerInput pi) {
        if (usersGameObjects.Count < MaxPlayers && !usersGameObjects.ContainsKey(pi.playerIndex)) {

            Debug.Log("Player " + pi.playerIndex + " Joined!");
            GameObject userObject = pi.gameObject;
            UserInputHandler userInputHandler = userObject.GetComponent<UserInputHandler>();
            userInputHandler.InitializeUserInputHandler(pi, tempCharacterOption);
            usersGameObjects.Add(pi.playerIndex, userObject);

            //TODO: MOVE THIS ASSIGNMENT TO MORE SUITABLE LOCATION
            userInputHandler.playerController.SetHoop(tempHoop.GetComponentInChildren<HoopController>());

            if (usersGameObjects.Count >= MaxPlayers) {
                this.GetComponent<PlayerInputManager>().DisableJoining();
                PopUpManager.Instance.showPopUpMenu("No more players can be added");
            }
        } else {
            PopUpManager.Instance.showPopUpMenu("No more players can be added");
        }
    }

    public void HandlePlayerLeft(PlayerInput pi) { 

    }

}
