using System.Collections;
using UnityEngine;
using NETWORK_ENGINE;

public class NetTransform2D : NetworkComponent {
  
  private Vector3 lastPosition;
  private Vector3 lastRotation;
  private float minThreshold = 0.1f;
  private float maxThreshold = 5.0f;

  #region M_NETWORK_ENGINE
  public override void NetworkedStart() {
    lastPosition = new Vector3();
    lastRotation = new Vector3();
  }

  public override void HandleMessage(string flag, string value) {
    if (flag == "LOC") {
      if (IsClient) {
        lastPosition = Parser.ParseVector3(value);
      }
    }

    if (flag == "ROT") {
      if (IsClient) {
        lastRotation = Parser.ParseVector3(value);
      }
    }
  }

  public override IEnumerator SlowUpdate() {
    while (IsServer) {
      float deltaPosition = (lastPosition - transform.position).magnitude;
      if (deltaPosition > minThreshold) {
        lastPosition = transform.position;
        SendUpdate("LOC", lastPosition.ToString("F2"));
      }

      float deltaRotation = (lastRotation - transform.rotation.eulerAngles).magnitude;
      if (deltaRotation > minThreshold) {
        lastRotation = transform.rotation.eulerAngles;
        SendUpdate("ROT", lastRotation.ToString("F2"));
      }

      if (IsDirty) {
        SendUpdate("LOC", lastPosition.ToString("F2"));
        SendUpdate("ROT", lastRotation.ToString("F2"));
        IsDirty = false;
      }
      yield return new WaitForSeconds(0.1f);
    }
  }
  #endregion

  #region M_UNITY
  void Start() { }

  void Update() {
    if (IsClient) {
      float deltaPosition = (transform.position - lastPosition).magnitude;
      if (deltaPosition > maxThreshold) {
        transform.position = lastPosition;
        transform.rotation = Quaternion.Euler(lastRotation);
      }
      else {
        transform.position = Vector3.Lerp(transform.position, lastPosition, 3.5f * Time.deltaTime);
        transform.rotation = Quaternion.Euler(Vector3.Lerp(transform.rotation.eulerAngles, lastRotation, 120 * Time.deltaTime));
      }
    }
  }
  #endregion
}