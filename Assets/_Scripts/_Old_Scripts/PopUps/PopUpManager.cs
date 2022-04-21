using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class PopUpManager : MonoBehaviour
{
    public static PopUpManager Instance { get; private set; }
    public GameObject PopUpPrompt;
    private Timer timer;

    void Awake()
    {
        if (Instance != null) {
            Debug.Log("Trying to create another instance!");
        } else {
            Instance = this;
            timer = GetComponent<Timer>();
            PopUpPrompt = Instantiate(PopUpPrompt, this.transform);
            hidePopUpMenu();
        }
    }

    public void showPopUpMenu(string text) {
        PopUpPrompt.SetActive(true);
        PopUpPrompt.GetComponent<PopUpPrompt>().fade(GetComponent<CanvasGroup>(), 1f, 0f);
        PopUpPrompt.GetComponent<PopUpPrompt>().setPromptDetails(text);
        timer.BeginTimer(3.0f, hidePopUpMenu);
    }

    public void hidePopUpMenu() {
        PopUpPrompt.GetComponent<PopUpPrompt>().fade(GetComponent<CanvasGroup>(), 0f, 2.0f);
    }

    

}
