using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using TMPro;

public class NetPlayerManager : NetworkComponent {
  #region FIELDS
  private string username;
  private string isReady;

  public InputField NameInput;
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

      if (IsClient && !IsLocalPlayer) {
        NameText.text = value;
      }
    }

    if (flag == "READY") {
      this.isReady = value;

      if (IsServer) {
        SendUpdate(flag, value);
      }

      if (IsClient && !IsLocalPlayer) {
        IsReadyText.text = "Ready";
        IsReadyText.color = new Color32(57, 255, 20, 255);
        IsReadyButton.interactable = false;
      }
    }

  }

  public override IEnumerator SlowUpdate() {
    yield return new WaitForSeconds(0.1f);
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

  public void SetIsReady(bool isReady) {
    if (IsLocalPlayer) {
      SendCommand("READY", isReady.ToString());
    }
  }
  #endregion
}