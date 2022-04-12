using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Map", menuName = "Map")]
public class Map : ScriptableObject
{
    public new string name;
    public GameObject map;
    public Sprite graphic;
    public bool unlocked;

    public float gravity;
    public GameObject hoop;

    //sounds of court?
    //music?
}
