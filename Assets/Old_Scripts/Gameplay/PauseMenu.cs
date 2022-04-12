using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;
    [SerializeField] private Button firstButtonSelected;

    private void Awake() {
        if (Instance != null)
            Debug.Log("Trying to create another instance of PauseMenu!");
        else {
            Instance = this;
            Resume();
        }
    }

    public void PauseResumeManager() {
        if (gameIsPaused)
            Resume();
        else
            Pause();
    }

    public void Resume() {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButtonSelected.gameObject);
    }

    public void QuitGame() {
        Debug.Log("QUITTING");
        Application.Quit();
    }

    public void RestartGame() {
        Debug.Log("Restarting");
        Time.timeScale = 1f;

        //List<PlayerConfiguration> pConfs = new List<PlayerConfiguration>();
        //foreach (var item in PlayerConfigurationManager.Instance.GetPlayerConfigs()) {
        //    PlayerInput pInput = new PlayerInput();
        //    pConfs.Add(new PlayerConfiguration(item.Input));
        //}
        //Debug.Log("player 0 is  " + pConfs[0].Input.playerIndex);

        Destroy(PlayerConfigurationManager.Instance.gameObject);
        SceneManager.LoadScene("PlayerSetup");

        //Debug.Log("player 0 is now  " + pConfs[0].Input.playerIndex);

        //for (int i = 0; i < pConfs.Count; i++) {
        //    PlayerConfigurationManager.Instance.HandlePlayerJoined(pConfs[i].Input);
        //    Debug.Log("adding  " + pConfs[i].Input.playerIndex);
        //}
        //Debug.Log("Added Everyone");
    }
}
