using UnityEngine;

public class PointIndicatorTween : MonoBehaviour {
    private float xTrans = 0f;
    private float yTrans = 0f;

    private void Awake() {
        //hide point indicators
        this.gameObject.SetActive(false);
    }
    public void StartAnimation(float xTranslation, float yTranslation, float duration) {
        xTrans = xTranslation;
        yTrans = yTranslation;

        //reset positions
        this.transform.position = this.GetComponentInParent<Transform>().position;
        LeanTween.scale(this.gameObject, new Vector3(0f, 0f, 0f), 0f);

        LeanTween.rotate(this.gameObject, new Vector3(0f, 0f, 10f), 0.1f).setOnComplete(RotateRight);

        LeanTween.move(this.gameObject, new Vector2(this.transform.position.x + xTranslation, this.transform.position.y + yTranslation), duration);
        LeanTween.scale(this.gameObject, new Vector3(1f, 1f, 1f), duration).setOnComplete(FadeOut);
    }

    void RotateRight() {
        LeanTween.rotate(this.gameObject, new Vector3(0f, 0f, -10f), 0.1f).setOnComplete(RotateLeft);
    }

    void RotateLeft() {
        LeanTween.rotate(this.gameObject, new Vector3(0f, 0f, 10f), 0.1f).setOnComplete(RotateRight);
    }

    void FadeOut() {
        LeanTween.scale(this.gameObject, new Vector3(0f, 0f, 0f), 0.5f).setOnComplete(SetStateOff);
    }
    void SetStateOff() {
        LeanTween.move(this.gameObject, new Vector2(this.transform.position.x - xTrans, this.transform.position.y - yTrans), 0f);
        LeanTween.cancel(this.gameObject);
        this.gameObject.SetActive(false);
    }
}
