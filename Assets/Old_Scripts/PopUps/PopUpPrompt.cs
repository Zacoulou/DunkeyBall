using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpPrompt : MonoBehaviour
{
    public TextMeshProUGUI promptText;
    public Button okButton;
    public RectTransform panelRectTransform;
    public static PopUpPrompt Instance { get; private set; }
    private Button previousSelectedButton;

    void Awake() {
        if (Instance != null) {
            Debug.Log("Trying to create another instance!");
        } else {
            Instance = this;
        }
    }

    public void setPromptDetails(string text) {
        LeanTween.cancelAll(); //cancels all animations to allow for new ones to override previous ones
        LeanTween.alpha(this.gameObject, 1f, 0f); //makes object completely opaque and visible
        promptText.text = text;
        panelRectTransform.sizeDelta = new Vector2(text.Length * 25 , 75); //sizes panel based on text length
    }

    public void fade(CanvasGroup canvasGroup, float alpha, float duration) {
        LeanTween.alphaCanvas(canvasGroup, alpha, duration);
    }

    public void setNotActive() {
        this.gameObject.SetActive(false);
    }
}
