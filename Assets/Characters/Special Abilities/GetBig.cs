using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetBig : SpecialAbility {

    string abilityDescription = "Grows in size to stomp opponents";
    
    public override string description {
        get {
            return abilityDescription;
        }
    }

    public GetBig() {

    }

    public override void Activate() {
        Debug.Log("Activating Ability");
    }
}
