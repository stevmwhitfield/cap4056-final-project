using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class GameMaster : NetworkComponent {
  #region FIELDS
  public float secondsPerGame = 5f;
  public bool isGameReady = false;
  public bool isGameRunning = false;
  public bool isGameOver = false;
  #endregion

  #region M_NETWORK_ENGINE
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
        CheckArePlayersReady();
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

  #region M_UNITY
  void Start() { }

  void Update() { }
  #endregion

  #region M_HELPERS
  private IEnumerator GameTimer() {
    yield return new WaitForSecondsRealtime(secondsPerGame);
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
      GameObject player = MyCore.NetCreateObject(playerType, npm.Owner, spawnPoints[i].transform.position);
      player.GetComponent<NetJumperController>().Name = npm.Name;
      i += 1;
    }

    SendUpdate("READY", isGameReady.ToString());
  }

  private void EndGame() {
    // Destroy players
    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    foreach (GameObject player in players) {
      int playerNetId = player.GetComponent<NetworkID>().NetId;
      MyCore.NetDestroyObject(playerNetId);
    }

    // Disconnect players - DOES NOT WORK
    GameObject[] netPlayerManagers = GameObject.FindGameObjectsWithTag("NPM");
    foreach (GameObject npm in netPlayerManagers) {
      int npmNetId = npm.GetComponent<NetworkID>().NetId;
      MyCore.NetDestroyObject(npmNetId);
      MyCore.Disconnect(npmNetId);
    }

    // Destroy level
    GameObject level = GameObject.FindGameObjectWithTag("Level");
    MyCore.NetDestroyObject(level.GetComponent<NetworkID>().NetId);

    SendUpdate("GAMEOVER", true.ToString());
    StartCoroutine(ResetGame());
  }

  private IEnumerator ResetGame() {
    yield return new WaitForSeconds(5f);
    SendUpdate("RESET", "1");
  }
  #endregion
}
