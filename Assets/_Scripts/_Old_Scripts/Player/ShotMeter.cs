using UnityEngine;
using UnityEngine.UI;

public class ShotMeter : MonoBehaviour {
    public Image shotMeter;
    public GameObject shotMeterObject;
    private bool isShooting = false;
    private float shotMeterValue = 0f;
    public bool shotMeterFull = false;
    private bool rising = true;

    private readonly float meterIncrement = 0.04f; // value between 0 - 1
    private PlayerMovement attachedPlayer;

    private void Start() {
        ResetShotMeter();
        shotMeterObject.SetActive(false);
        attachedPlayer = this.GetComponentInParent<PlayerMovement>();
    }

    public void ResetShotMeter() {
        shotMeterValue = 0f;
        shotMeter.fillAmount = shotMeterValue;
        rising = true;
    }

    public void StartShotMeter() {
        shotMeterObject.SetActive(true);
        isShooting = true;
    }

    public float ReleaseShot() {
        float value = shotMeterValue;
        EndShotMeter();
        return value;
    }

    public void FixedUpdate() {
        if (isShooting) {
            ChangeShotMeterValue(meterIncrement); //Time.timeScale
        }
    }

    public void ChangeShotMeterValue(float amount) {

        if (rising)
            shotMeterValue += amount;
        else
            shotMeterValue -= amount;


        if (shotMeterValue >= 1f) {
            rising = false;
            shotMeterValue = 1f;
                
        }
        if (shotMeterValue <= 0) {
            shotMeterValue = 0;
            attachedPlayer.OnReleaseShot();
        }

        shotMeter.fillAmount = shotMeterValue;
        
    }


    private void EndShotMeter() {
        isShooting = false;
        ResetShotMeter();
        shotMeterObject.SetActive(false);
    }

}

