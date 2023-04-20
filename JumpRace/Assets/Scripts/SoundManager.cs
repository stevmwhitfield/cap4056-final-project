using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour {
  [SerializeField] private Slider volumeSlider;

  private void Start() {
    if (!PlayerPrefs.HasKey("Volume")) {
      PlayerPrefs.SetFloat("Volume", 1f);
      LoadVolume();
    }
    else {
      LoadVolume();
    }
  }

  public void OnVolumeChange() {
    AudioListener.volume = volumeSlider.value;
    SaveVolume();
  }

  private void SaveVolume() {
    PlayerPrefs.SetFloat("Volume", volumeSlider.value);
  }

  private void LoadVolume() {
    volumeSlider.value = PlayerPrefs.GetFloat("Volume");
  }
}
