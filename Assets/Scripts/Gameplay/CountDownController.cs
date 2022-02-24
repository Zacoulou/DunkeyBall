using System.Collections;
using TMPro;
using UnityEngine;

public class CountDownController : MonoBehaviour {

    public static CountDownController instance;
    public TextMeshProUGUI countDownDisplay;


    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    public void StartCountDown(string[] textStatements) {
        countDownDisplay.gameObject.SetActive(true);
        StartCoroutine(CountDownToStart(textStatements.Length, textStatements));
    }

    IEnumerator CountDownToStart(int countDownTime, string[] textStatements) {
        while (countDownTime > 0) {
            //countDownDisplay.text = countDownTime.ToString();
            //countDownDisplay.text = "Ready";
            countDownDisplay.text = textStatements[textStatements.Length - countDownTime];

            yield return new WaitForSeconds(1f);

            countDownTime--;
        }

        countDownDisplay.text = "GO!";

        yield return new WaitForSeconds(1f);

        countDownDisplay.gameObject.SetActive(false);
    }
}
