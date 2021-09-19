using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientRefresher : MonoBehaviour {

    GameStateController controller;

    void Refresh () {
        if (!controller) controller = GetComponent<GameStateController>();
        foreach (var link in controller.taskLinks) {
            link.Refresh();
        }
        foreach (var link in controller.playerLinks) {
            link.Refresh();
        }
    }
}
