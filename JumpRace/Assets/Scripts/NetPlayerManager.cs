using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using TMPro;

public class NetPlayerManager : NetworkComponent {
  #region FIELDS
  public string username;
  public bool isReady;

  public TMP_InputField NameInput;
  public Button IsReadyButton;
  public TMP_Text NameText;
  public TMP_Text IsReadyText;
  public Image PlaceholderSprite;
  #endregion

  #region M_NETWORK_ENGINE
  public override void NetworkedStart() {
    if (!IsLocalPlayer) {
      NameInput.gameObject.SetActive(false);
      IsReadyButton.gameObject.SetActive(false);
    }

    try {
      Transform playerHolderTransform = GameObject.FindGameObjectWithTag("PlayerHolder").transform;
      this.transform.SetParent(playerHolderTransform);
    }
    catch {
      Debug.Log("ERROR: Object with tag \"PlayerHolder\" not found!");
    }
  }

  public override void HandleMessage(string flag, string value) {
    if (flag == "NAME") {
      this.username = value;

      if (IsServer) {
        SendUpdate(flag, value);
      }

      if (IsClient) {
        NameText.text = value;
      }
    }

    if (flag == "READY") {
      Debug.Log("FLAG: READY");
      this.isReady = true;

      if (IsServer) {
        SendUpdate(flag, value);
      }

      if (isReady) {
        if (IsClient) {
          IsReadyText.text = "Ready";
          IsReadyText.color = new Color32(57, 255, 20, 255);
        }

        if (IsLocalPlayer) {
          NameInput.interactable = false;
          IsReadyButton.interactable = false;
        }
      }
    }

  }

  public override IEnumerator SlowUpdate() {
    while (IsConnected) {
      if (IsServer) {
        if (IsDirty) {
          SendUpdate("NAME", username);
          SendUpdate("READY", isReady.ToString());
          IsDirty = false;
        }
      }
      if (IsLocalPlayer) {
        if (this.username == "") {
          this.IsReadyButton.interactable = false;
        }
        else {
          this.IsReadyButton.interactable = true;
        }
      }
      yield return new WaitForSeconds(0.1f);
    }
  }
  #endregion

  #region M_UNITY
  void Start() {

  }

  void Update() {

  }
  #endregion

  #region M_UI
  public void SetUsername(string username) {
    if (IsLocalPlayer) {
      SendCommand("NAME", username);
    }
  }

  public void SetIsReady() {
    if (IsLocalPlayer) {
      SendCommand("READY", true.ToString());
    }
  }
  #endregion
}
