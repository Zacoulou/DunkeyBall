using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class SpawnPlayerSetupMenu : MonoBehaviour {
    public GameObject playerSetupMenuPrefab;
    public PlayerInput input;
    public GameObject characterCustomizationPreFab;
    [NonSerialized] public GameObject menu;

    private void Awake() {
        var rootMenu = GameObject.Find("PlayerSetupCanvas");
        if (rootMenu != null) {
            menu = Instantiate(playerSetupMenuPrefab, rootMenu.transform);
            
            input.uiInputModule = menu.GetComponentInChildren<InputSystemUIInputModule>();
            menu.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(input.playerIndex);
        }
    }
}
