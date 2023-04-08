using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.UI;
using TMPro;

public class NetPlayerManager : NetworkComponent {
  #region FIELDS
  public string Name { get; set; } = "";
  public bool IsReady { get; set; } = false;

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
      transform.SetParent(playerHolderTransform);
    }
    catch {
      Debug.Log("ERROR: Object with tag \"PlayerHolder\" not found!");
    }
  }

  public override void HandleMessage(string flag, string value) {
    if (flag == "NAME") {
      Name = value;

      if (IsServer) {
        SendUpdate(flag, value);
      }

      if (IsClient) {
        NameText.text = value;
      }
    }

    if (flag == "READY") {
      IsReady = bool.Parse(value);

      if (IsServer) {
        SendUpdate(flag, value);
      }

      if (IsReady) {
        if (IsClient) {
          IsReadyText.text = "Ready";
          IsReadyText.color = new Color32(57, 255, 20, 255);
        }

        if (IsLocalPlayer) {
          NameInput.gameObject.SetActive(false);
          IsReadyButton.gameObject.SetActive(false);
        }
      }
    }

  }

  public override IEnumerator SlowUpdate() {
    while (IsConnected) {
      if (IsServer) {
        if (IsDirty) {
          SendUpdate("NAME", Name);
          SendUpdate("READY", IsReady.ToString());
          IsDirty = false;
        }
      }
      if (IsLocalPlayer) {
        if (Name == "") {
          IsReadyButton.interactable = false;
        }
        else {
          IsReadyButton.interactable = true;
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
  public void SetName(string name) {
    if (IsLocalPlayer) {
      SendCommand("NAME", name);
    }
  }

  public void SetIsReady() {
    if (IsLocalPlayer) {
      if (!IsReady) {
        SendCommand("READY", true.ToString());
      }
    }
  }
  #endregion
}
