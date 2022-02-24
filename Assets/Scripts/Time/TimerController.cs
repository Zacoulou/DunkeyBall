using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimerController : MonoBehaviour {
    public static TimerController instance;

    public TextMeshProUGUI timer;

    private TimeSpan timePlaying;

    private bool timerGoing = false;

    private float elapsedTime;

    private bool play = true;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        timer.text = "00:00";
    }

    public void beginTimer(float startTime, float endtime) {
        bool countDown = true;
        timerGoing = true;
        elapsedTime = startTime;

        if (startTime < endtime)
            countDown = false;

        StartCoroutine(UpdateTimer(startTime, endtime, countDown));
    }

    public void endTimer() {
        timerGoing = false;
    }

    public void pauseResumeTimer(bool playing) {
        play = playing;
    }

    private IEnumerator UpdateTimer(float startTime, float endTime, bool countDown) {
        while (timerGoing) {
            if (play) {
                if (countDown) {
                    elapsedTime -= Time.deltaTime;
                    if (elapsedTime <= endTime) {
                        endTimer();
                    }
                } else {
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime >= endTime) {
                        endTimer();
                    }
                }
            }

            timePlaying = TimeSpan.FromSeconds(elapsedTime);
            string timePlayingString;

            if (elapsedTime > 3f)
                timePlayingString = timePlaying.ToString("mm':'ss");
            else
                timePlayingString = timePlaying.ToString("mm':'ss'.'f");
            timer.text = timePlayingString;

            yield return null;
        }
    }

    public float getElapsedTime() {
        return elapsedTime;
    }
}
