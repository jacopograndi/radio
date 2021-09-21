using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLink : MonoBehaviour {

    public string nameId;

    GameStateController controller;
    public TimeLeftUI timeleftUI;

    public void Refresh() {
        if (!controller) controller = FindObjectOfType<GameStateController>();
        if (timeleftUI) timeleftUI.Refresh();
        if (controller.getLocalPlayer() != this) {
            transform.position = controller.gameState.playerList.getPlayer(nameId).pos;
        }
    }
}
