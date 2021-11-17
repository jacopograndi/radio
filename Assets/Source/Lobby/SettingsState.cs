using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsState {

    public FullScreenMode fullscreenMode = FullScreenMode.Windowed;
    public int resolutionX = 640;
    public int resolutionY = 480;
    public int refreshRate = 60;
    public int quality = 0;
    public float masterAudio = 1;

    public void setFullscreenMode (FullScreenMode mode) {
        fullscreenMode = mode;
        Screen.SetResolution(resolutionX, resolutionY, mode, refreshRate);
        save();
	}
    public void setResolution (int w, int h, int refresh) {
        resolutionX = w;
        resolutionY = h;
        refreshRate = refresh;
        Screen.SetResolution(resolutionX, resolutionY, fullscreenMode, refresh);
        save();
	}
    public void setQuality (string level) {
		for (int i = 0; i < QualitySettings.names.Length; i++) {
			if (QualitySettings.names[i] == level) {
				quality = i;
				break;
			}
		}

		QualitySettings.SetQualityLevel(quality, true);
        save();
	}

    public void setMasterAudio (float vol) {
        masterAudio = vol;
        save();
	}

    public void load () {
        fullscreenMode = (FullScreenMode)PlayerPrefs.GetInt("fullscreenMode", (int)FullScreenMode.Windowed);
        resolutionX = PlayerPrefs.GetInt("resolutionX", Screen.currentResolution.width);
        resolutionY = PlayerPrefs.GetInt("resolutionY", Screen.currentResolution.height);
        refreshRate = PlayerPrefs.GetInt("refreshRate", Screen.currentResolution.refreshRate);
        quality = PlayerPrefs.GetInt("quality", QualitySettings.GetQualityLevel());
        masterAudio = PlayerPrefs.GetFloat("masterAudio", 1);
	}

    public void save () {
        PlayerPrefs.SetInt("fullscreenMode", (int)fullscreenMode);
        PlayerPrefs.SetInt("resolutionX", resolutionX);
        PlayerPrefs.SetInt("resolutionY", resolutionY);
        PlayerPrefs.SetInt("quality", quality);
        PlayerPrefs.SetInt("refreshRate", refreshRate);
        PlayerPrefs.SetFloat("masterAudio", masterAudio);
	}
}
