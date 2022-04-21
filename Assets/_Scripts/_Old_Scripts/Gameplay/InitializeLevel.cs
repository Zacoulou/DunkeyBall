using System;
using System.Collections.Generic;
using UnityEngine;

public class InitializeLevel : MonoBehaviour {
    [SerializeField] private Transform[] team0Spawns;
    [SerializeField] private Transform[] team1Spawns;

    [SerializeField]private GameObject playerPreFab;
    [NonSerialized] public GameObject mapInPlay;
    private MultipleTargetCamera MovingCam;

    public List<GameObject> playerList;

    private PlayerConfiguration[] team0Players;
    private PlayerConfiguration[] team1Players;
    

    // Start is called before the first frame update
    void Start() {
        SpawnMap();
        MovingCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MultipleTargetCamera>();
        playerList = new List<GameObject>();

        team0Players = PlayerConfigurationManager.Instance.team0Players.ToArray();
        team1Players = PlayerConfigurationManager.Instance.team1Players.ToArray();

        SpawnTeam(team0Players, team0Spawns, 0);
        SpawnTeam(team1Players, team1Spawns, 1);

        GameManager gm = GameObject.FindObjectOfType(typeof(GameManager)) as GameManager; ;
        gm.StartGame();
    }

    private void SpawnMap() {
        GameObject cam = GameObject.FindGameObjectWithTag("MapTransform");
        mapInPlay = Instantiate(GameSettingsManager.Instance.gameSettings.map.map, cam.transform);

        SetMapGravity();
    }

    private void SetMapGravity() {
        float gravity = GameSettingsManager.Instance.gameSettings.map.gravity;
        Physics2D.gravity = new Vector3(0f, gravity * 3, 0f);
    }

    private void SpawnTeam(PlayerConfiguration[] teamPlayers, Transform[] teamSpawns, int team) {
        List<GameObject> teamates = new List<GameObject>();

        for (int i = 0; i < teamPlayers.Length; i++) {
            teamates.Add(SpawnPlayer(teamPlayers[i], teamSpawns[i], team));
        }
        
        //For all AI players assign teamates
        if(teamates.Count >= 2) {
            if (teamates[0].GetComponent<AI_Controller>().isAI)
                teamates[0].GetComponent<AI_Controller>().SetTeamate(teamates[1]);
            if (teamates[1].GetComponent<AI_Controller>().isAI)
                teamates[1].GetComponent<AI_Controller>().SetTeamate(teamates[0]);
        } 
    }

    private GameObject SpawnPlayer(PlayerConfiguration playerConfig, Transform spawnLocation, int team) {
        GameObject player = Instantiate(playerPreFab, spawnLocation.position, spawnLocation.rotation, gameObject.transform);
        if (playerConfig.isAI) {
            player.GetComponent<PlayerInputHandler>().enabled = false;
            player.GetComponent<AI_Controller>().enabled = true;
            player.GetComponent<AI_Controller>().InitializePlayer(team, playerConfig);
        } else {
            player.GetComponent<PlayerInputHandler>().enabled = true;
            player.GetComponent<AI_Controller>().enabled = false;
            player.GetComponent<PlayerInputHandler>().InitializePlayer(playerConfig);
        }
        player.GetComponent<PlayerMovement>().spawnPoint = spawnLocation;
        playerList.Add(player);

        //Add players to camera movement manager
        MovingCam.AddTarget(player.transform);

        return player;
    }





}
