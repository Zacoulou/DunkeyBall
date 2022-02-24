using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameSettingsManager : MonoBehaviour {
    public static GameSettingsManager Instance { get; private set; }
    public GameObject[] panels;
    public List<GameObject> activePanels;
    public Canvas parentCanvas;
    public Button startGameButton;
    public List<Map> allMaps;
    [NonSerialized] public List<Map> unlockedMaps;

    [NonSerialized] public GameSettings gameSettings;

    private void Awake() {
        if (Instance != null)
            Debug.Log("Trying to create another instance!");
        else {
            Instance = this;
        }
        unlockedMaps = GetUnlockedMaps();
        activePanels = new List<GameObject>();
        SetDefaultGameSettings();
        startGameButton.gameObject.SetActive(false);
    }

    private List<Map> GetUnlockedMaps() {
        List<Map> listOfMaps = new List<Map>();

        foreach (var map in allMaps) {
            if (map.unlocked)
                listOfMaps.Add(map);
        }

        return listOfMaps;
    }

    public void SpawnPanels() {
        foreach (var panel in panels) {
            GameObject currPanel = Instantiate(panel, parentCanvas.transform);
            currPanel.transform.SetParent(parentCanvas.transform);
            activePanels.Add(currPanel);
        }
    }

    public void CanvasState(bool state) {
        parentCanvas.enabled = state;
        startGameButton.gameObject.SetActive(state);
    }

    public void BackToPlayerSetup() {

        foreach (var panel in activePanels) {
            Destroy(panel.gameObject);
        }
        PlayerConfigurationManager.Instance.playerMenuIsActive = true;
        CanvasState(false); //disables game settings menus
        PlayerConfigurationManager.Instance.canvasStates(true); //enables player join menu
        PlayerConfigurationManager.Instance.GetComponent<PlayerInputManager>().EnableJoining();
    }

    public void SetDefaultGameSettings() {
        gameSettings = new GameSettings(0, 11, false); //defaults to standard game to 11 on rooftop court
        gameSettings.map = unlockedMaps[0];
    }

    public void StartGame() {
        SceneManager.LoadScene("GameScene");
    }
}




public class GameSettings {
    //rules
    public float endCondition;
    public int gameTypeIndex;
    public bool hastimer;

    //Map
    public Map map;

    public GameSettings(int gameType, float endGame, bool timer) {
        gameTypeIndex = gameType;
        endCondition = endGame;
        hastimer = timer;
    }

}