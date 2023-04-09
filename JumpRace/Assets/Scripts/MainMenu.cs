using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	#region FIELDS
	[SerializeField] private Canvas mainCanvas;
	[SerializeField] private Canvas settingsCanvas;
	#endregion

	#region M_MENU_BUTTON_CALLBACKS
	public void OnStart() {
		SceneManager.LoadScene(1);
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

	public void OnQuit() {
		Application.Quit(0);
	}
  #endregion

  private void SetActiveMenu(int menu) {
    switch (menu) {
      case 0:
        mainCanvas.enabled = true;
        settingsCanvas.enabled = false;
        break;
      case 1:
        mainCanvas.enabled = false;
        settingsCanvas.enabled = true;
        break;
    }
  }
}
