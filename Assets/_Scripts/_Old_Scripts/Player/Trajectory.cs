using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    int dotsNumber = 15;
    [SerializeField] GameObject dotsParent;
    [SerializeField] GameObject dotsPrefab;
    float dotsSpacing = 0.05f;

    Transform[] dotsList;
    Vector2 pos;
    float timeStamp;

    private void Start() {
        Hide();
        PrepareDots();
    }

    void PrepareDots() {
        dotsList = new Transform[dotsNumber];

        for (int i = 0; i < dotsNumber; i++) {
            dotsList[i] = Instantiate(dotsPrefab, null).transform;
            dotsList[i].parent = dotsParent.transform;
        }
    }

    public void UpdateDots(Vector3 ballPos, Vector2 forceApplied) {
        timeStamp = dotsSpacing;
        for (int i = 0; i < dotsNumber; i++) {
            pos.x = (ballPos.x + forceApplied.x * timeStamp);
            pos.y = (ballPos.y + forceApplied.y * timeStamp) - (Physics2D.gravity.magnitude * timeStamp * timeStamp) / 2f;

            dotsList[i].position = pos;
            timeStamp += dotsSpacing;
        }
    }

    public void Show() {
        dotsParent.SetActive(true);
    }

    public void Hide() {
        dotsParent.SetActive(false);
    }
}
