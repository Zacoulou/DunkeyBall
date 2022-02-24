using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSetupMenuController : MonoBehaviour {
    private int PlayerIndex;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button readyButton;
    [SerializeField] public TextMeshProUGUI characterName;
    [SerializeField] private Button readyUnreadyButton;

    [SerializeField] private GameObject Team0Toggle;
    private Color team0Color;
    [SerializeField] private GameObject Team1Toggle;
    private Color team1Color;

    private float ignoreInputTime = 0.5f;
    private bool inputEnabled;
    public GameObject customizationPreFab;
    [SerializeField] public Transform customCharacterTransform;

    private int appearanceDetails = 0;
    private bool isAI = false;
    public Image panelImage;
    public Color[] panelColors;

    private void Awake() {
        //Spawn Player customization prefab to visualize appearance changes
        customizationPreFab = Instantiate(customizationPreFab, this.transform);
        Vector3 pos = new Vector3(0, 0.5f, 0);
        customizationPreFab.transform.position = pos;
        customizationPreFab.transform.localScale = new Vector3(100, 100, 1);

        //set Character name Text
        characterName.text = customizationPreFab.GetComponent<SwitchPart>().labels[0];
        team0Color = Team0Toggle.GetComponent<Image>().color;
        team1Color = Team1Toggle.GetComponent<Image>().color;
        Team1Toggle.GetComponent<Image>().color = Color.grey;
    }

    //reselects first button if no button is selected
    private void FixedUpdate() {
        if (this.GetComponentInChildren<EventSystem>().currentSelectedGameObject == null) {
            this.GetComponentInChildren<EventSystem>().SetSelectedGameObject(this.GetComponentInChildren<EventSystem>().firstSelectedGameObject);
        }
    }

    public void SetPlayerIndex(int pi, bool ai = false) {
        PlayerIndex = pi;
        isAI = ai;
        if (!isAI) {
            titleText.SetText("Player " + (pi + 1).ToString());
        } else {
            titleText.SetText("AI");
        }
            

        ignoreInputTime = Time.time + ignoreInputTime;

        //set panel color based on player index
        panelImage.color = panelColors[PlayerIndex];
    }

    public int GetPlayerIndex() {
        return PlayerIndex;
    }

    // Update is called once per frame
    void Update() {
        if (Time.time > ignoreInputTime) {
            inputEnabled = true;
        }
    }

    public void SetTeam(int team) {
        if (!inputEnabled) { return; }

        PlayerConfigurationManager.Instance.SetPlayerTeam(PlayerIndex, team);
    }

    public void ReadyPlayer() {
        if (!inputEnabled) { return; }

        PlayerConfigurationManager.Instance.ReadyPlayer(PlayerIndex, true);

        SubmitCharacterCustomization();
        readyPanel.SetActive(true);
        menuPanel.SetActive(false);

        readyUnreadyButton.Select();
    }


    public void CheckEveryoneReadyUp() {
        PlayerConfigurationManager.Instance.checkAllReadyUp();
    }


    public void AddAIPlayer() {
        if (!inputEnabled) { return; }

        PlayerConfigurationManager.Instance.HandleAIAdded();
    }

    public void PlayerLeave() {
        if (!inputEnabled) { return; }

        PlayerConfigurationManager.Instance.HandlePlayerLeft(PlayerIndex);
    }

    public void UnReady() {
        if (!inputEnabled) { return; }

        if (PlayerConfigurationManager.Instance.playerMenuIsActive) {
            PlayerConfigurationManager.Instance.ReadyPlayer(PlayerIndex, false);
            readyPanel.SetActive(false);
            menuPanel.SetActive(true);
            readyButton.Select();
        }
    }

    public void SelectCharacter(int direction) {
        if (!inputEnabled) { return; }

        int labelsLength = customizationPreFab.GetComponent<SwitchPart>().labels.Length;

        int newVal = appearanceDetails += direction;

        //check if value is out of bounds
        if (newVal >= labelsLength) {
            newVal = 0;
        } else if (newVal < 0) {
            newVal = labelsLength - 1;
        }

        int labelIndex = newVal;

        appearanceDetails = newVal;

        //change text for respective characters name
        characterName.text = customizationPreFab.GetComponent<SwitchPart>().labels[labelIndex];

        //switch out body parts for character
        customizationPreFab.GetComponent<SwitchPart>().switchParts(appearanceDetails);
        SubmitCharacterCustomization();
    }

    public void SubmitCharacterCustomization() {
        PlayerConfigurationManager.Instance.SetPlayerAppearance(PlayerIndex, appearanceDetails);
        PlayerConfigurationManager.Instance.SetPlayerColor(PlayerIndex, panelColors[PlayerIndex]);
    }

    public void ToggleTeam(int teamOption) {
        if (!inputEnabled) { return; }

        if (teamOption == 0) {
            Team0Toggle.GetComponent<Image>().color = team0Color;
            Team1Toggle.GetComponent<Image>().color = Color.grey;
        } else {
            Team0Toggle.GetComponent<Image>().color = Color.grey;
            Team1Toggle.GetComponent<Image>().color = team1Color;
        }

        PlayerConfigurationManager.Instance.SetPlayerTeam(PlayerIndex, teamOption);

    }
}
