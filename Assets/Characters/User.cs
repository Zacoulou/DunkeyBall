using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Represents the human playing the game. Bridges gap between person, control scheme, and in game thing they are controlling
public abstract class User : MonoBehaviour {
    public abstract PlayerController playerController { get; set; }
    public abstract int playerIndex { get; set; }
}
