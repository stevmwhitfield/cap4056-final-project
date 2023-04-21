using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
  #region FIELDS

  [SerializeField] private Canvas mainCanvas;
  [SerializeField] private Canvas settingsCanvas;
  [SerializeField] private Canvas creditsCanvas;

  #endregion

  #region UNITY

  void Start() {
    string[] args = System.Environment.GetCommandLineArgs();
    foreach (string arg in args) {
      if (arg.StartsWith("PORT_") || arg.Contains("MASTER")) {
        // Load WAN scene
        SceneManager.LoadScene(2);
      }
    }
  }

  #endregion

  #region MENU_BUTTON_CALLBACKS

  public void OnStartLAN() {
    SceneManager.LoadScene(1);
  }

  public void OnStartWAN() {
    SceneManager.LoadScene(2);
  }

  public void OnReturnToMainMenu() {
    SceneManager.LoadScene(0);
  }

  public void OnReturn() {
    SetActiveMenu(0);
  }

  public void OnSettings() {
    SetActiveMenu(1);
  }

  public void OnCredits() {
    SetActiveMenu(2);
  }

  public void OnQuit() {
    Application.Quit(0);
  }

  #endregion

  private void SetActiveMenu(int menu) {
    switch (menu) {
      case 0:
        mainCanvas.enabled = true;
        settingsCanvas.enabled = false;
        creditsCanvas.enabled = false;
        break;
      case 1:
        mainCanvas.enabled = false;
        settingsCanvas.enabled = true;
        creditsCanvas.enabled = false;
        break;
      case 2:
        mainCanvas.enabled = false;
        settingsCanvas.enabled = false;
        creditsCanvas.enabled = true;
        break;
    }
  }
}
