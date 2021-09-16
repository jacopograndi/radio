using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeLeftUI : MonoBehaviour {

    TMP_Text label;
    GameStateComponent gameStateComp;

    void Start() {
        gameStateComp = FindObjectOfType<GameStateComponent>();
        label = GetComponent<TMP_Text>();
    }

    string FormatSecToMinSec (float sec) {
        string format = "";
        int mm = 0;
        while ((mm + 1) * 60 <= sec) mm++;
        float ss = sec - mm*60;
        if (ss > 59.9f) ss = 59.9f;
        format = mm + ":" + ss.ToString("00.0");
        return format;

    }

    void Refresh() {
        label.text = FormatSecToMinSec(gameStateComp.gameState.timeLeft);
    }
}
