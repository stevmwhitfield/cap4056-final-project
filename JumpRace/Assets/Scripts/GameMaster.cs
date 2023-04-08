using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class GameMaster : NetworkComponent {
  #region FIELDS
  public bool isGameReady = false;
  public bool isGameRunning = false;
  public bool isGameOver = false;
  #endregion

  #region M_NETWORK_ENGINE
  public override void NetworkedStart() {
    //Canvas scoreCanvas = GameObject.FindGameObjectWithTag("ScoreHolder").transform.parent.GetComponent<Canvas>();
    //Debug.Log("NetworkedStart(): ScoreCanvas: " + scoreCanvas.name);
    //scoreCanvas.enabled = false;
  }

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

      if (IsClient) {
        if (isGameOver) {
          // Display score screen
          Canvas scoreCanvas = GameObject.FindGameObjectWithTag("ScoreHolder").transform.parent.GetComponent<Canvas>();
          scoreCanvas.enabled = true;
        }
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    if (IsServer) {
      // Determine if all players are ready to start
      while (!isGameReady) {
        CheckArePlayersReady();
        yield return new WaitForSeconds(1.0f);
      }

      // Spawn the level, players, etc.
      if (isGameReady) {
        Debug.Log("SlowUpdate(): Game is preparing");
        PrepareGame();
      }

      // Start the game
      Debug.Log("SlowUpdate(): Start game timer");
      StartCoroutine(GameTimer());
      isGameRunning = true;
      while (isGameRunning) {
        Debug.Log("SlowUpdate(): Game is running");
        yield return new WaitForSeconds(0.1f);
      }

      // End the game
      if (isGameOver) {
        Debug.Log("SlowUpdate(): Game is over");
        EndGame();
      }
    }
  }
  #endregion

  #region M_UNITY
  void Start() {

  }

  void Update() {

  }
  #endregion

  private IEnumerator GameTimer() {
    yield return new WaitForSecondsRealtime(5f);
    isGameRunning = false;
    isGameOver = true;
  }

  private void CheckArePlayersReady() {
    if (MyCore.Connections.Count > 1) {
      isGameReady = true;

      NetPlayerManager[] netPlayerManagers = GameObject.FindObjectsOfType<NetPlayerManager>();
      foreach (NetPlayerManager npm in netPlayerManagers) {
        if (!npm.IsReady) {
          isGameReady = false;
          break;
        }
      }
    }
  }

  private void PrepareGame() {
    // Disable lobby screen
    Canvas lobbyCanvas = GameObject.FindGameObjectWithTag("PlayerHolder").transform.parent.GetComponent<Canvas>();
    lobbyCanvas.enabled = false;

    // Spawn level
    int levelType = 1;
    GameObject level = MyCore.NetCreateObject(levelType, Owner);
    GameObject root = GameObject.FindGameObjectWithTag("Root");
    level.transform.SetParent(root.transform);

    // Spawn players
    int i = 0;
    GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
    NetPlayerManager[] netPlayerManagers = GameObject.FindObjectsOfType<NetPlayerManager>();
    foreach (NetPlayerManager npm in netPlayerManagers) {
      // Create players and set spawn position
      int playerType = 0;
      GameObject player = MyCore.NetCreateObject(playerType, npm.Owner);
      player.transform.position = spawnPoints[i].transform.position;
      i += 1;
    }

    SendUpdate("READY", isGameReady.ToString());
  }

  private void EndGame() {
    Debug.Log("EndGame(): Start EndGame()");
    // Destroy players
    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    foreach (GameObject player in players) {
      int playerNetId = player.GetComponent<NetworkID>().NetId;
      Debug.Log("EndGame(): Player Net ID: " + playerNetId);
      MyCore.NetDestroyObject(playerNetId);
    }

    // Destroy level
    GameObject level = GameObject.FindGameObjectWithTag("Level");
    Debug.Log("EndGame(): Level: " + level.name);
    MyCore.NetDestroyObject(level.GetComponent<NetworkID>().NetId);

    SendUpdate("GAMEOVER", true.ToString());
  }
}