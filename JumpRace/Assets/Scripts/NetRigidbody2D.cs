using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class NetRigidbody2D : NetworkComponent {
  #region M_NETWORK_ENGINE
  public override void NetworkedStart() {

  }

  public override void HandleMessage(string flag, string value) {

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
}