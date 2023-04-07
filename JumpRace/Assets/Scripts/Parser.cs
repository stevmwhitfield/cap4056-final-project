using System.Numerics;

public class Parser {
  public static Vector2 ParseVector2(string str) {
    Vector2 result = new Vector2();
    string[] args = str.Trim('(').Trim(')').Split(',');
    result.X = float.Parse(args[0]);
    result.Y = float.Parse(args[1]);
    return result;
  }
}
