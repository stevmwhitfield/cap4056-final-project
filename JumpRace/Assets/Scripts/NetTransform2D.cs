using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class NetTransform2D : NetworkComponent {
  
  private Vector2 lastPosition;
  private Vector2 lastRotation;
  private Vector2 lastScale;
  private float minThreshold = 0.1f;
  private float maxThreshold = 5.0f;

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