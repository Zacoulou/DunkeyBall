using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Characters/Character")]
public class Character : ScriptableObject
{
    [Serializable]
    public struct AppearanceDetails {
        public Sprite head;
        public Sprite torso;
        public Sprite armL;
        public Sprite armR;
        public Sprite tail;
        public Sprite shoeL;
        public Sprite shoeR;
    }

    [Serializable]
    public struct MovementStats {
        public float movementSpeed;
        public float jumpForce;
        public float sprintMultiplier;
    }

    [Serializable]
    public struct OtherStats {
        public double shootingAccuracy; 
    }

    public AppearanceDetails appearanceDetails;
    public MovementStats movementStats;
    public OtherStats otherStats;

    [NonSerialized] public SpecialAbility specialAbility;
    [SerializeField] private SpecialAbilities abilityName;

    //Enum of all characterNames
    public enum CharacterNames {
        JUMBO,
        POOTY
    };

    //Enum of all possible special abilities
    public enum SpecialAbilities {
        GETBIG,
        JETPACK
    };

    //Initializes Special Ability based on selected ability
    private void Awake() {
        switch (abilityName) {
            case SpecialAbilities.GETBIG:
                specialAbility = new GetBig();
                break;
            case SpecialAbilities.JETPACK:
                //specialAbility = new Jetpack;
                break;
            default:
                Debug.Log(abilityName + " is not a valid ability");
                break;
        }
    }

}
