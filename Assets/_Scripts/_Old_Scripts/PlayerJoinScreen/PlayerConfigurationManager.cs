using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerConfigurationManager : MonoBehaviour {
    private GameObject[] TentativePlayerConfigurationGameObjects;
    private List<GameObject> PlayerConfigurationGameObjects = new List<GameObject>();
    private List<PlayerConfiguration> playerConfigs = null;
    [NonSerialized] public List<PlayerConfiguration> team0Players = null;
    [NonSerialized] public List<PlayerConfiguration> team1Players = null;
    public Canvas MainLayout;
    public Canvas PressAnyButton;
    [NonSerialized] public bool playerMenuIsActive = true;
    [SerializeField] private int MaxPlayers = 4;

    public GameObject AIConfigurationPrefab;
    private GameObject AIConfigurationInstance;

    public static PlayerConfigurationManager Instance { get; private set; }
    private GameObject[] customizableCharacters = null;
    private GameObject[] aiPanels;

    private void Awake() {
        if (Instance != null) {
            Debug.Log("Trying to create another instance!");
        } else {
            Instance = this;
            DontDestroyOnLoad(Instance);
            playerConfigs = new List<PlayerConfiguration>();
            team0Players = new List<PlayerConfiguration>();
            team1Players = new List<PlayerConfiguration>();
        }
    }

    public void SetPlayerTeam(int index, int team) {
        findMatchingGameObjectOfPlayerConfigs(index).PlayerTeam = team;
    }

    public void SetPlayerAppearance(int index, int appearanceDetails) {
        findMatchingGameObjectOfPlayerConfigs(index).appearanceIndices = appearanceDetails;
    }

    public void SetPlayerColor(int index, Color color) {
        findMatchingGameObjectOfPlayerConfigs(index).playerColor = color;
    }

    public void ReadyPlayer(int index, bool state) {
        findMatchingGameObjectOfPlayerConfigs(index).IsReady = state;
        checkAllReadyUp();
    }

    public bool checkAllReadyUp() {
        bool allReady = false;

        if (playerMenuIsActive && playerConfigs.Count >= 1 && playerConfigs.All(p => p.IsReady == true)) {
            if (checkFairTeamDistribution()) {
                this.GetComponent<PlayerInputManager>().DisableJoining();

                GameSettingsManager.Instance.CanvasState(true); //enables game settings menus
                GameSettingsManager.Instance.SpawnPanels();
                playerMenuIsActive = false;
                customizableCharacters = GameObject.FindGameObjectsWithTag("CustomizablePlayer");
                aiPanels = GameObject.FindGameObjectsWithTag("AIPanel");
                canvasStates(false); //Disables player join menu

                allReady = true;
            }
        }
        return allReady;
    }

    public void canvasStates(bool state) {
        this.enabled = state;
        MainLayout.enabled = state;
        PressAnyButton.enabled = state;

        //Sets state of all AI panels
        foreach (var panel in aiPanels) {
            if (panel)
                panel.SetActive(state);
        }

        //Sets state of all customizable player previews
        foreach (var character in customizableCharacters) {
            if (character)
                character.SetActive(state);
        }
    }


    public void HandleAIAdded() {
        int aiIndex = 0;
        List<int> availableSlots = new List<int>{0, 1, 2, 3};
        for (int i = 0; i < playerConfigs.Count; i++) {
            if (availableSlots.Contains(playerConfigs[i].PlayerIndex)) {
                availableSlots.Remove(playerConfigs[i].PlayerIndex);
            }
        }
        if (availableSlots.Count > 0)
            aiIndex = availableSlots.Max();

        var rootMenu = GameObject.Find("PlayerSetupCanvas");
        if (rootMenu != null) {
            if (playerConfigs.Count < MaxPlayers && !playerConfigs.Any(p => p.PlayerIndex == aiIndex)) {
                Debug.Log("AI " + aiIndex + " Joined!");
                AIConfigurationInstance = Instantiate(AIConfigurationPrefab, rootMenu.transform);
                AIConfigurationInstance.GetComponent<PlayerSetupMenuController>().SetPlayerIndex(aiIndex, true);

                PlayerConfigurationGameObjects.Add(findMatchingGameObjectOfPlayerConfigurationList(aiIndex));
                playerConfigs.Add(new PlayerConfiguration(aiIndex, true));

                ReadyPlayer(aiIndex, true); //Ready up AI
                AIConfigurationInstance.GetComponent<PlayerSetupMenuController>().SubmitCharacterCustomization(); //set AI color

                if (playerConfigs.Count >= MaxPlayers) {
                    this.GetComponent<PlayerInputManager>().DisableJoining();
                    PopUpManager.Instance.showPopUpMenu("No more players can be added");
                }

            } else {
                Debug.Log("aiIndex: " + aiIndex);
                PopUpManager.Instance.showPopUpMenu("No more players can be added");
            }
        }
    }


    public void HandlePlayerJoined(PlayerInput pi) {
        if (playerConfigs.Count < MaxPlayers && !playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex)) {
            
            Debug.Log("Player " + pi.playerIndex + " Joined!");
            PlayerConfigurationGameObjects.Add(findMatchingGameObjectOfPlayerConfigurationList(pi.playerIndex));

            pi.transform.SetParent(transform);
            playerConfigs.Add(new PlayerConfiguration(pi));

            if (playerConfigs.Count >= MaxPlayers) {
                this.GetComponent<PlayerInputManager>().DisableJoining();
                PopUpManager.Instance.showPopUpMenu("No more players can be added");
            }
        } else {
            PopUpManager.Instance.showPopUpMenu("No more players can be added");
        }
    }

    //Handles player leaving (PlayerLeftEvent in Player Input Manager Component)
    public void HandlePlayerLeft(PlayerInput pi) {
        Debug.Log(pi.playerIndex + " Disconnected");
    }

    //Handles Players leaving from manually leaving 
    public void HandlePlayerLeft(int playerIndex) {
        Debug.Log("Player " + playerIndex + " Left");
        playerConfigs.Remove(findMatchingGameObjectOfPlayerConfigs(playerIndex));

        GameObject playerConfig = findMatchingGameObjectOfPlayerConfigurationList(playerIndex);
        if (playerConfig) {
            PlayerConfigurationGameObjects.Remove(playerConfig);
            Destroy(playerConfig.GetComponent<SpawnPlayerSetupMenu>().menu);
            Destroy(playerConfig);
        }

        GameObject aiPanel = findAIPanelGameObject(playerIndex);
        if (aiPanel) {
            Destroy(aiPanel);
        }

        if (playerConfigs.Count < MaxPlayers)
            this.GetComponent<PlayerInputManager>().EnableJoining();

        checkAllReadyUp();
    }


    public List<PlayerConfiguration> GetPlayerConfigs() {
        return playerConfigs;
    }

    private GameObject findMatchingGameObjectOfPlayerConfigurationList(int playerIndex) {
        GameObject pConfig = null;

        TentativePlayerConfigurationGameObjects = GameObject.FindGameObjectsWithTag("PlayerConfiguration");

        for (int i = 0; i < TentativePlayerConfigurationGameObjects.Length; i++) {
            if (TentativePlayerConfigurationGameObjects[i].GetComponent<SpawnPlayerSetupMenu>().input.playerIndex == playerIndex)
                pConfig = TentativePlayerConfigurationGameObjects[i];
        }

        return pConfig;
    }

    private GameObject findAIPanelGameObject(int aiIndex) {
        GameObject panel = null;

        GameObject[] panels = GameObject.FindGameObjectsWithTag("AIPanel");

        for (int i = 0; i < panels.Length; i++) {
            if (panels[i].GetComponent<PlayerSetupMenuController>().GetPlayerIndex() == aiIndex)
                panel = panels[i];
        }

        return panel;
    }

    private PlayerConfiguration findMatchingGameObjectOfPlayerConfigs(int playerIndex) {
        PlayerConfiguration pConfig = null;

        for (int i = 0; i < playerConfigs.Count; i++) {
            if (playerConfigs[i].PlayerIndex == playerIndex)
                pConfig = playerConfigs[i];
        }
        return pConfig;
    }

    private bool checkFairTeamDistribution() {
        bool teamsMeetConditions = false;
        assignPlayersToTeamArrays();

        if (team0Players.Count > 0 && team1Players.Count > 0) { //Check that there is at least 1 player per team
            if (team0Players.Count <= 2 && team1Players.Count <= 2) { //continues to check if there are no more than 2 players per team
                teamsMeetConditions = true; // teams are playable
                if (playerConfigs.Count % 2 == 0) { //checks for even number of players
                    if (team0Players.Count > team1Players.Count) {
                        PopUpManager.Instance.showPopUpMenu("Team 1 has more players. Move players to team 2?");
                    } else if (team0Players.Count < team1Players.Count) {
                        PopUpManager.Instance.showPopUpMenu("Team 2 has more players. move players to team 1?");
                    }
                } else { //Odd number of players
                    if (team0Players.Count > team1Players.Count)
                        PopUpManager.Instance.showPopUpMenu("Team 1 has more players. Add AI to Team 2?");
                    else
                        PopUpManager.Instance.showPopUpMenu("Team 2 has more players. Add AI to Team 1?");
                }
            } else {
                PopUpManager.Instance.showPopUpMenu("There can be no more than 2 players per team!");
            }
        } else {
            PopUpManager.Instance.showPopUpMenu("At least 1 player on each team is needed!");
        }

        return teamsMeetConditions;
    }

    private void assignPlayersToTeamArrays() {
        team0Players.Clear();
        team1Players.Clear();

        for (int i = 0; i < playerConfigs.Count; i++) {
            if (playerConfigs[i].PlayerTeam == 0)
                team0Players.Add(playerConfigs[i]);
            else
                team1Players.Add(playerConfigs[i]);
        }
    }
}

public class PlayerConfiguration {
    public PlayerInput Input { get; set; }
    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }
    public int PlayerTeam { get; set; }
    public int appearanceIndices { get; set; }

    public Color playerColor;

    public bool isAI = false;

    public PlayerConfiguration(PlayerInput pi) {
        PlayerIndex = pi.playerIndex;
        Input = pi;
    }

    public PlayerConfiguration(int index, bool ai) {
        PlayerIndex = index;
        isAI = ai;
    }
}
