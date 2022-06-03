using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearanceController : MonoBehaviour
{
    Character.AppearanceDetails appearanceDetails;          //Class of all character sprites

    [SerializeField] private SpriteRenderer head;
    [SerializeField] private SpriteRenderer torso;
    [SerializeField] private SpriteRenderer armL;
    [SerializeField] private SpriteRenderer armR;
    [SerializeField] private SpriteRenderer tail;

    [SerializeField] private SpriteRenderer shoeL;
    [SerializeField] private SpriteRenderer shoeR;


    [SerializeField] private SpriteRenderer ball;

    public void SetAppearanceValues(Character.AppearanceDetails appearance) {
        appearanceDetails = appearance;
        SetAppearance();
    }

    private void SetAppearance() {
        head.sprite = appearanceDetails.head;
        torso.sprite = appearanceDetails.torso;
        armL.sprite = appearanceDetails.armL;
        armR.sprite = appearanceDetails.armR;
        tail.sprite = appearanceDetails.tail;
        shoeL.sprite = appearanceDetails.shoeL;
        shoeR.sprite = appearanceDetails.shoeR;
    }

    public void SetBallVisible(bool state) {
        ball.enabled = state;
    }

    public bool GetHasTail() {
        bool hasTail = false;

        if (tail.sprite) {
            hasTail = true;
        }

        return hasTail;
    }
}
