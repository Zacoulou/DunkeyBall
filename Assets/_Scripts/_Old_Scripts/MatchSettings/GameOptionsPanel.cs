using System;
using TMPro;
using UnityEngine;

public class GameOptionsPanel : MonoBehaviour {
    public static GameOptionsPanel Instance { get; private set; }

    private string[] gameTypes = { "Score", "Timed" }; //All Game Types: First to certain points, timed match...

    [SerializeField] public GameObject gameTypeSelection;
    [SerializeField] public TextMeshProUGUI gameTypeText;
    private int gameTypeIndex = 0;

    [SerializeField] public GameObject[] gameTypeMenus;
    private GameObject gameTypeMenu;

    [NonSerialized] public float endConditionScore = 11f;
    [NonSerialized] public float endConditionTime = 300f;

    [NonSerialized] public GameSettings gameSettings;

    private void Awake() {
        if (Instance != null) {
            Destroy(Instance.gameObject);
        } else {
            Instance = this;
        }

        gameTypeText.text = gameTypes[gameTypeIndex];
        setGameTypeMenu(0);
    }

    private void setGameTypeMenu(int gameTypeIndex) {
        if (gameTypeMenu)
            gameTypeMenu.SetActive(false);

        gameTypeMenu = gameTypeMenus[gameTypeIndex];
        gameTypeMenu.SetActive(true);

        //linkVerticalButtons(gameTypeSelection.GetComponentsInChildren<Button>(), gameTypeMenu.GetComponentsInChildren<Button>());
    }

    //private void linkVerticalButtons(Button[] top, Button[] bottom) {
    //    for (int i = 0; i < top.Length; i++) {
    //        //Debug.Log("linking:  " + top[i].name + "  +  " + bottom[i].name);
    //        Navigation nav1 = top[i].navigation;
    //        nav1.selectOnDown = bottom[i];
    //        top[i].navigation = nav1;

    //        Navigation nav2 = bottom[i].navigation;
    //        nav1.selectOnUp = top[i];
    //        bottom[i].navigation = nav2;
    //    }
    //}

    public void cycleGameType(int direction) {
        gameTypeIndex += direction;
        if (gameTypeIndex >= gameTypes.Length)
            gameTypeIndex = 0;
        else if (gameTypeIndex < 0)
            gameTypeIndex = gameTypes.Length - 1;

        setGameTypeMenu(gameTypeIndex);
        gameTypeText.text = gameTypes[gameTypeIndex];
    }

    public void changeScore(int direction) {
        endConditionScore += direction;
        if (endConditionScore > 21)
            endConditionScore = 2;
        else if (endConditionScore < 2)
            endConditionScore = 21;

        findTextElementByTag("EndConditionText").text = endConditionScore.ToString();

        setGameOptions(gameTypeIndex, endConditionScore, false);
    }

    public void changeGameLength(int direction) {
        endConditionTime += direction;

        if (endConditionTime > 600)
            endConditionTime = 30;
        else if (endConditionTime < 30)
            endConditionTime = 600;

        findTextElementByTag("EndConditionText").text = TimeSpan.FromSeconds(endConditionTime).ToString("mm':'ss");

        setGameOptions(gameTypeIndex, endConditionTime, true);
    }

    private TextMeshProUGUI findTextElementByTag(string tag) {
        TextMeshProUGUI[] endConditionTexts = gameTypeMenu.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var item in endConditionTexts) {
            if (item.tag == tag)
                return item;
        }
        return null;
    }

    private void setGameOptions(int gameType, float endConditionValue, bool hasTimer) {
        GameSettingsManager.Instance.gameSettings.gameTypeIndex = gameType;
        GameSettingsManager.Instance.gameSettings.endCondition = endConditionValue;
        GameSettingsManager.Instance.gameSettings.hastimer = hasTimer;
    }
}

