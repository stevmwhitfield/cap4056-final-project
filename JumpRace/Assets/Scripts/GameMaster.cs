using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using TMPro;

public class GameMaster : NetworkComponent {
  #region FIELDS

  public bool isGameReady = false;
  public bool isGameRunning = false;
  public bool isGameOver = false;

  private readonly Dictionary<string, int> prefabTypes = new Dictionary<string, int>();
  private Dictionary<string, int> playerScores = new Dictionary<string, int>();
  private float secondsPerGame = 300f;
  private int minimumPlayers = 1;

  #endregion

  #region UNITY

  void Start() {
    prefabTypes.Add("Player", 1);
    prefabTypes.Add("EnemyRunner", 2);
    prefabTypes.Add("EnemyFlyer", 3);
    prefabTypes.Add("EnemyGunner", 4);
    prefabTypes.Add("Level_1", 5);
    prefabTypes.Add("Coin", 7);
    prefabTypes.Add("Score", 8);
  }

  void Update() { }

  #endregion

  #region NETWORK_ENGINE

  public override void NetworkedStart() { }

  public override void HandleMessage(string flag, string value) {
    if (flag == "READY") {
      isGameReady = bool.Parse(value);

      if (IsClient) {
        if (isGameReady) {
          // Hide lobby screen
          Canvas lobbyCanvas = GameObject.FindGameObjectWithTag("PlayerHolder").transform.parent.GetComponent<Canvas>();
          lobbyCanvas.enabled = false;
        }
      }
    }

    if (flag == "GAMEOVER") {
      isGameOver = bool.Parse(value);

      if (IsServer) {
        if (isGameOver) {
          Debug.Log("Game over");
          //CalculatePlayerScores();
        }
      }

      if (IsClient) {
        if (isGameOver) {
          // Display score screen
          Canvas scoreCanvas = GameObject.FindGameObjectWithTag("ScoreHolder").transform.parent.GetComponent<Canvas>();
          scoreCanvas.enabled = true;
        }
      }
    }

    if (flag == "RESET") {
      isGameReady = false;
      isGameRunning = false;
      isGameOver = false;

      if (IsClient) {
        // Hide score screen
        Canvas scoreCanvas = GameObject.FindGameObjectWithTag("ScoreHolder").transform.parent.GetComponent<Canvas>();
        scoreCanvas.enabled = false;

        // Display LAN screen
        GameObject lanMenu = GameObject.FindGameObjectWithTag("NetworkManager").transform.GetChild(0).GetChild(0).gameObject;
        lanMenu.SetActive(true);
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    if (IsServer) {
      // Determine if all players are ready to start
      while (!isGameReady) {
        isGameReady = ArePlayersReady();
        yield return new WaitForSeconds(1.0f);
      }

      // Spawn the level, players, etc.
      if (isGameReady) {
        PrepareGame();
      }

      // Start the game
      StartCoroutine(GameTimer());
      isGameRunning = true;
      while (isGameRunning) {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) {
          isGameRunning = false;
          isGameOver = true;
        }
        yield return new WaitForSeconds(0.1f);
      }

      // End the game
      if (isGameOver) {
        EndGame();
      }
    }
  }

  #endregion

  #region GAME_CYCLE

  private IEnumerator GameTimer() {
    yield return new WaitForSecondsRealtime(secondsPerGame);
    isGameRunning = false;
    isGameOver = true;
  }

  private bool ArePlayersReady() {
    bool isReady = false;

    if (MyCore.Connections.Count >= minimumPlayers) {
      int numPlayersReady = 0;
      NetPlayerManager[] netPlayerManagers = GameObject.FindObjectsOfType<NetPlayerManager>();

      if (netPlayerManagers.Length >= minimumPlayers) {
        foreach (NetPlayerManager npm in netPlayerManagers) {
          if (npm.IsReady) {
            numPlayersReady++;
          }
        }

        if (numPlayersReady == netPlayerManagers.Length) {
          isReady = true;
        }
      }
    }
    return isReady;
  }

  private void PrepareGame() {
    // Disable lobby screen
    Canvas lobbyCanvas = GameObject.FindGameObjectWithTag("PlayerHolder").transform.parent.GetComponent<Canvas>();
    lobbyCanvas.enabled = false;

    // Spawn level
    GameObject level = MyCore.NetCreateObject(prefabTypes["Level_1"], Owner);
    GameObject root = GameObject.FindGameObjectWithTag("Root");
    level.transform.SetParent(root.transform);

    // Spawn enemies
    //GameObject[] runnerSpawners = GameObject.FindGameObjectsWithTag("RunnerSpawner");
    //GameObject[] flyerSpawners = GameObject.FindGameObjectsWithTag("FlyerSpawner");
    //GameObject[] gunnerSpawners = GameObject.FindGameObjectsWithTag("GunnerSpawner");

    //SpawnPrefab(runnerSpawners, prefabTypes["EnemyRunner"]);
    //SpawnPrefab(flyerSpawners, prefabTypes["EnemyFlyer"]);
    //SpawnPrefab(gunnerSpawners, prefabTypes["EnemyGunner"]);

    // Spawn items
    GameObject[] coinSpawners = GameObject.FindGameObjectsWithTag("CoinSpawner");

    SpawnPrefab(coinSpawners, prefabTypes["Coin"]);

    // Spawn players
    SpawnPlayers();

    SendUpdate("READY", isGameReady.ToString());
  }

  private void EndGame() {
    // Destroy players, enemies, and items
    DestroyPrefabInstances("Player");
    DestroyPrefabInstances("Enemy");
    DestroyPrefabInstances("Item");

    // Destroy level
    GameObject level = GameObject.FindGameObjectWithTag("Level");
    MyCore.NetDestroyObject(level.GetComponent<NetworkID>().NetId);

    GetPlayerScores();
    foreach (KeyValuePair<string, int> playerScore in playerScores) {
      GameObject scorePanel = MyCore.NetCreateObject(prefabTypes["Score"], Owner);

      scorePanel.GetComponent<Score>().PlayerName = playerScore.Key;
      scorePanel.GetComponent<Score>().PlayerScore = playerScore.Value;
    }

    SendUpdate("GAMEOVER", true.ToString());
    //StartCoroutine(ResetGame());
  }

  private IEnumerator ResetGame() {
    yield return new WaitForSeconds(5f);
    SendUpdate("RESET", "1");
  }

  #endregion

  private void SpawnPrefab(GameObject[] spawners, int prefabType) {
    int i = 0;
    foreach (GameObject spawner in spawners) {
      GameObject obj = MyCore.NetCreateObject(prefabType, Owner, spawners[i].transform.position);
      i += 1;
    }
  }

  private void DestroyPrefabInstances(string tag) {
    GameObject[] instances = GameObject.FindGameObjectsWithTag(tag);
    foreach (GameObject instance in instances) {
      int netId = instance.GetComponent<NetworkID>().NetId;
      MyCore.NetDestroyObject(netId);
    }
  }

  private void SpawnPlayers() {
    int i = 0;
    GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
    NetPlayerManager[] netPlayerManagers = GameObject.FindObjectsOfType<NetPlayerManager>();
    foreach (NetPlayerManager npm in netPlayerManagers) {
      GameObject player = MyCore.NetCreateObject(prefabTypes["Player"], npm.Owner, spawnPoints[i].transform.position);
      player.GetComponent<NetJumperController>().Name = npm.Name;
      StartCoroutine(npm.DecreaseTimeToGoal());
      i += 1;
    }
  }

  private void GetPlayerScores() {
    NetPlayerManager[] netPlayerManagers = GameObject.FindObjectsOfType<NetPlayerManager>();

    foreach (NetPlayerManager npm in netPlayerManagers) {
      playerScores.Add(npm.Name, npm.Score);
    }
  }
}
