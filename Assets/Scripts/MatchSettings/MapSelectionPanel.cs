using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectionPanel : MonoBehaviour
{
    public static MapSelectionPanel Instance { get; private set; }

    public TextMeshProUGUI mapName;
    public Image mapPreview;
    private Map currMap;
    private int currIndex = 0;

    private List<Map> availableMaps;

    public void Awake() {
        if (Instance != null) {
            Destroy(Instance.gameObject);
        } else {
            Instance = this;

        }
        availableMaps = new List<Map>(GameSettingsManager.Instance.unlockedMaps);
        currMap = availableMaps[currIndex];
        changeMap(0); //display first map
    }

    public void changeMap(int direction) {
        
        currIndex += direction;
        if (currIndex > availableMaps.Count - 1)
            currIndex = 0;
        else if (currIndex < 0)
            currIndex = availableMaps.Count - 1;

        currMap = availableMaps[currIndex];
        mapPreview.sprite = currMap.graphic;
        mapName.text = currMap.name;

        setGameMap();
    }

    private void setGameMap() {
        GameSettingsManager.Instance.gameSettings.map = currMap;
    }

    public Map getSelectedMap() {
        return currMap;
    }
}
