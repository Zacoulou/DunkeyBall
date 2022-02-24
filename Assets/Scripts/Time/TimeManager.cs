using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private float slowDownLength = 1f;
    private bool isInSlowMotion = false;

    public static TimeManager Instance;

    private void Awake() {
        if (Instance != null) {
            Debug.Log("Trying to create another instance!");
        } else {
            Instance = this;
        }
    }

    private void FixedUpdate() {
        if (isInSlowMotion && Time.timeScale < 1f) {
            float increment = Mathf.Abs(Time.unscaledDeltaTime / slowDownLength);
            Time.timeScale += increment;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        } else {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            isInSlowMotion = false;
        }

        //Debug.Log(Time.timeScale + "     " + Time.fixedDeltaTime);
    }

    public void StartSlowMotion(float duration, float sloMoFactor) {
        slowDownLength = duration;
        isInSlowMotion = true;
        Time.timeScale = sloMoFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}
