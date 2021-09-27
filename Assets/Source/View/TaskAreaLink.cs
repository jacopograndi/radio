using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskAreaLink : MonoBehaviour {

    public int taskId;
    public int order;

    GameStateController controller;

    public void Refresh () {
        if (!controller) controller = FindObjectOfType<GameStateController>();
        if (!gameObject) return;

        var graphics = gameObject.transform.GetChild(0).gameObject;

        bool active = true;
        Task task = controller.gameState.taskList.fromId(taskId);
        string localPlayerName = controller.getLocalPlayer().nameId;
        PlayerRepr player = controller.gameState.playerList.getPlayer(localPlayerName);

        if (task.completed) 
            active = false;

        if (player != null
         && player.acceptedTaskId != -1 
         && !(player.acceptedTaskId == taskId && order == 1))
            active = false;
        
        if (active != graphics.activeSelf)
            graphics.SetActive(active);
    }
}
