using UnityEngine;

public class Parser {
  public static Vector2 ParseVector2(string str) {
    Vector2 result = new Vector2();
    string[] args = str.Trim('(').Trim(')').Split(',');
    result.x = float.Parse(args[0]);
    result.y = float.Parse(args[1]);
    Debug.Log($"PARSER: x:{result.x}, y:{result.y}");
    return result;
  }

  public static Vector3 ParseVector3(string str) {
    Vector3 result = new Vector3();
    string[] args = str.Trim('(').Trim(')').Split(',');
    result.x = float.Parse(args[0]);
    result.y = float.Parse(args[1]);
    result.z = float.Parse(args[2]);
    return result;
  }
}
