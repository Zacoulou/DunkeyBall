using DentedPixel;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    public Image colorIndicator;
    public Image dunkMeterGauge;
    public Image dunkMeterGaugeFrame;
    public ParticleSystem particleEffect;

    private float dunkMeterValue = 0f;
    public bool dunkMeterFull = false;
    private int playerSortingOrder = 0;
    private bool returnParticleEffectToActive = false;

    private void Start() {
        ResetDunkMeter();
        particleEffect.Stop();
    }

    public void UpdateUILocation(float xLocation, int sortingOrder, bool playerJumping)
    {
        this.gameObject.transform.position = new Vector2(xLocation, 1.5f);
        playerSortingOrder = sortingOrder;

        if (playerJumping && particleEffect.isPlaying) {
            particleEffect.Stop();
            returnParticleEffectToActive = true;
        }else if (returnParticleEffectToActive && !playerJumping && !particleEffect.isPlaying) {
            particleEffect.Play();
            returnParticleEffectToActive = false;
        }
    }

    public void SetColor(Color color) {
        colorIndicator.color = color;
    }

    public void ResetDunkMeter() {
        dunkMeterValue = 0f;
        dunkMeterGauge.fillAmount = dunkMeterValue;
    }

    //Increments dunk meter value based on given amount.
    public void ChangeDunkMeterValue(float amount) {
        if (!dunkMeterFull ) { // || amount < 0

            dunkMeterValue += amount;

            //Cap value from 0 to 1
            if (dunkMeterValue >= 1f) {
                dunkMeterValue = 1f;
                DunkMeterFull();
            }
            if (dunkMeterValue < 0)
                dunkMeterValue = 0;

            dunkMeterGauge.fillAmount = dunkMeterValue;
        }
    }

    public float GetDunkMeterValue() {
        return dunkMeterValue;
    }

    private void DunkMeterFull() {
        dunkMeterFull = true;
        particleEffect.Play();
        particleEffect.GetComponent<Renderer>().sortingOrder = playerSortingOrder + 3;
        StartPulse();
    }

    private void EndOnFireStreak() {
        dunkMeterFull = false;
        particleEffect.Stop();
    }

    private void StartPulse() {
        LeanTween.scale(dunkMeterGauge.gameObject, new Vector3(1.25f, 1.25f, 1f), 0.5f).setOnComplete(EndPulse);
        dunkMeterGauge.color = Color.green;
    }

    private void EndPulse() {
        LeanTween.scale(dunkMeterGauge.gameObject, new Vector3(1f, 1f, 1f), 0.5f).setOnComplete(ResetGaugeColor);
    }

    private void ResetGaugeColor() {
        dunkMeterGauge.color = Color.white;
    }
}
