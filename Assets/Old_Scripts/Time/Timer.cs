using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    //public static Timer Instance;
    private float elapsedTime = 0.0f;
    private bool timerGoing = false;
    public delegate void OnTimerEvent();
    private OnTimerEvent CallBackFunction;

    //private void Awake() {
    //    if (Instance != null) {
    //        Debug.Log("Trying to create another instance!");
    //    } else {
    //        Instance = this;
    //    }
    //}

    public void BeginTimer(float duration, OnTimerEvent NewCallBackFunction) { 
        timerGoing = true;
        elapsedTime = 0.0f;
        CallBackFunction = NewCallBackFunction;
        StartCoroutine(UpdateTimer(duration));
    }

    IEnumerator UpdateTimer(float duration) {
        while (timerGoing) {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= duration) {
                timerGoing = false;
                CallBackFunction();
            }
            
            yield return null;
        }
        
    }

}
