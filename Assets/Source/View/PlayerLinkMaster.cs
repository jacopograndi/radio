using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLinkMaster : PlayerLink {

    GameStateController controller;

    public GameObject uiPanelPrefab;
    [HideInInspector]
    public RawImage viewportTex = null;
    public GameObject linePrefab;

    TMP_Text taskLabel;
    GameObject uiPanel;
    Transform handle;
    LineRenderer line;

    public override void Refresh() {
        if (!controller) controller = FindObjectOfType<GameStateController>();
        if (controller.getLocalPlayer() != this) {
            transform.position = controller.gameState.playerList.getPlayer(nameId).pos;
        }

        if (uiPanel == null) {
            var viewportsHolder = GameObject.Find("Viewports");
            uiPanel = Instantiate(uiPanelPrefab);
            uiPanel.transform.SetParent(viewportsHolder.transform);
            uiPanel.transform.Find("NameLabel").GetComponent<TMP_Text>().text = nameId;
            handle = uiPanel.transform.Find("Handle");
            viewportTex = uiPanel.GetComponentInChildren<RawImage>();
            taskLabel = uiPanel.transform.Find("TaskLabel").GetComponent<TMP_Text>();

            GameObject lobj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            line = lobj.GetComponent<LineRenderer>();
        } 
        else {
            Vector3[] positions = new Vector3[2];
            positions[0] = transform.position;
            positions[1] = Camera.main.ScreenToWorldPoint(handle.position);
            line.SetPositions(positions);
            line.startWidth = 3;
            line.material.color = Color.black;

            var gst = controller.gameState;
            var player = gst.playerList.getPlayer(nameId);
            var task = gst.taskList.fromId(player.acceptedTaskId);
            if (task != null) {
                taskLabel.text = "Doing task " + task.id;
            } else {
                taskLabel.text = "No task";
			}
        }
    }
}
