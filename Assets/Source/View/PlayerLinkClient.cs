using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLinkClient : PlayerLink {

    GameStateController controller;
    public TimeLeftUI timeleftUI;

    public override void Refresh() {
        if (!controller) controller = FindObjectOfType<GameStateController>();
        if (timeleftUI) timeleftUI.Refresh();
        if (controller.getLocalPlayer() != this) {
            transform.position = controller.gameState.playerList.getPlayer(nameId).pos;
        }
    }
}
