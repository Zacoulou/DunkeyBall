using UnityEngine;

public class PulseTween : MonoBehaviour {
    private Timer timer;

    private void Awake() {
        timer = GetComponent<Timer>();
        //hide point indicators
        this.gameObject.SetActive(false);
    }
    public void StartAnimation(float duration) {
        LeanTween.scale(this.gameObject, new Vector3(0f, 0f, 0f), 0f);

        LeanTween.scale(this.gameObject, new Vector3(1f, 1f, 1f), 0.5f).setOnComplete(RotateRight);
        timer.BeginTimer(duration, SetStateOff);
    }

    void RotateRight() {
        LeanTween.rotate(this.gameObject, new Vector3(0f, 0f, -2f), 0.3f).setOnComplete(RotateLeft);
    }

    void RotateLeft() {
        LeanTween.rotate(this.gameObject, new Vector3(0f, 0f, 2f), 0.3f).setOnComplete(RotateRight);
    }

    void SetStateOff() {
        LeanTween.cancel(this.gameObject);
        this.gameObject.SetActive(false);
    }
}
