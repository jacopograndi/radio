using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLinkClient : PlayerLink {

    GameStateController controller;
    public TimeLeftUI timeleftUI;

    float speed = 0;

    public override void Refresh() {
        if (!controller) controller = FindObjectOfType<GameStateController>();
        if (timeleftUI) timeleftUI.Refresh();
        if (controller.getLocalPlayer() != this) {
            var player = controller.gameState.playerList.getPlayer(nameId);
            if (controller.lastSync.ContainsKey(nameId) 
                && Time.time > controller.lastSync[nameId]) 
            {
                float extr = Time.time - controller.lastSync[nameId];
                transform.position = player.pos + (player.rot * Vector3.forward) * player.vel * extr;
                transform.rotation = player.rot;
                speed = player.vel;
            } else {
                transform.position = player.pos;
                transform.rotation = player.rot;
                speed = player.vel;
            }
        }
    }
}
