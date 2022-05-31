using UnityEngine;


public class SwitchPart : MonoBehaviour {
    [SerializeField] BodyParts[] bodyParts;
    [SerializeField] public string[] labels;

    public void switchParts(int bodyPartIndices) //int bodyPartIndex
    {
        for (int i = 0; i < bodyParts.Length; i++) {
            bodyParts[i].SwitchPart(labels, bodyPartIndices);
        }
    }
}

[System.Serializable]
public class BodyParts {
    [SerializeField] UnityEngine.U2D.Animation.SpriteResolver[] spriteResolver;

    public UnityEngine.U2D.Animation.SpriteResolver[] SpriteResolver { get => spriteResolver; }

    //method that are going to be triggered by the button, and it will switch the sprites of each resolver list.
    public void SwitchPart(string[] labels, int id) {

        foreach (var item in spriteResolver) {
            //Debug.Log(item.GetCategory() + "|" + item.name + "|" + labels[id] + ":  " + id);
            item.SetCategoryAndLabel(item.GetCategory(), labels[id]);
        }
    }
}

