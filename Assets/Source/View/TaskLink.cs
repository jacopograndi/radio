using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskLink : MonoBehaviour {

    public int taskId;
	GameStateController controller;

    public GameObject linePrefab;
    public GameObject arrowTipPrefab;
    GameObject arrowTip;
    LineRenderer line;
    
    Image image;
    Button button;

    public bool selected = false;

    public void Refresh () {
        if (!controller) controller = FindObjectOfType<GameStateController>();
        
        if (!image) image = GetComponent<Image>();
        if (!button) {
            button = GetComponent<Button>();
            button.onClick.AddListener(() => { selected = !selected; });
        }
        if (!line) {
            var obj = Instantiate(linePrefab);
            line = obj.GetComponent<LineRenderer>();
            arrowTip = Instantiate(arrowTipPrefab);
		}

        image.color = Color.white;

        var gst = controller.gameState;
        var task = gst.taskList.fromId(taskId);
        if (task.completed) {
            button.interactable = false;
            selected = false;
        }

        bool playerDoing = false;
        foreach (var pair in gst.playerList.players) {
            if (pair.Value.acceptedTaskId == taskId) playerDoing = true;
        }
        if (task.completed) {
            image.color = new Color(0.9f, 1, 0.9f);
        } else if (playerDoing) {
            image.color = new Color(1, 1, 0.9f);
        } else {
            image.color = new Color(1, 0.9f, 0.9f);
        }

        if (selected) {
            line.enabled = true;
            arrowTip.SetActive(true);

            Vector3[] poss = new Vector3[2];
            poss[0] = task.start;
            poss[1] = task.destination;
            for (int i=0; i<poss.Length; i++) poss[i] += new Vector3(0, 10, 0);
            line.startWidth = 5;
            line.SetPositions(poss);

            arrowTip.transform.position = task.destination;
            arrowTip.transform.localScale = Vector3.one * 20;
            var diff = task.start - task.destination;
            if (diff.sqrMagnitude == 0) diff = Vector3.forward;
            arrowTip.transform.rotation = Quaternion.LookRotation(diff, Vector3.up);

            Color start = Color.red;
            Color end = Color.green;
            if (playerDoing) {
                start = Color.yellow;
            }

            arrowTip.GetComponentInChildren<Renderer>().material.color = end;
            line.startColor = start;
            line.endColor = end;
		} else {
            line.enabled = false;
            arrowTip.SetActive(false);
		}
    }

    public void OnMouseEnter () {
	}

    public void OnMouseExit () {
	}
}
