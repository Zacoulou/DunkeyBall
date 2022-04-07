using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFactory: MonoBehaviour
{
    public GameObject playerPrefab;

    private GameObject player;
    Character.CharacterNames characterNames;
    public Dictionary<Character.CharacterNames, Character> characterDetailsTable;

    public void createCharacter(Character.CharacterNames characterName, Character.SpecialAbilities nonDefaultSpecialAbility) {
        Character character = characterDetailsTable[characterName];
        //TODO:  creates PlayerController, and uses given Character to set the PlayerController's details
    }

    public GameObject createCharacter(Character character) {
        this.transform.position = new Vector3(0f, 2f, 0f);
        player = Instantiate(playerPrefab, this.transform) as GameObject;
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.InitializePlayerController(character);
        
        return player;
    }
}
