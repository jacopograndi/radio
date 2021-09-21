using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterRefresher : MonoBehaviour {

    GameObject canvas;

    GameStateController controller;

    void Refresh() {
        if (!controller) controller = GetComponent<GameStateController>();
        if (!canvas) canvas = GameObject.Find("Canvas");
        canvas.BroadcastMessage("Refresh");

        foreach (var link in controller.playerLinks) {
            link.Refresh();
        }
    }
}
