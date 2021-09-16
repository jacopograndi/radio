using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TaskListUI : MonoBehaviour {

    public GameObject taskUI;

    public GameStateComponent gameStateComp;

    void Start() {
        gameStateComp = FindObjectOfType<GameStateComponent>();
    }

    void Clear() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }

    void Fill() {
        foreach (Task task in gameStateComp.gameState.taskList.tasks) {
            GameObject taskUIobj = Instantiate(taskUI, transform);
            var tmptext = taskUIobj.transform.Find("TaskLabel").GetComponent<TMP_Text>();
            tmptext.text = "new task";
        }
    } 

    public void Refresh() {
        Clear();
        Fill();
    }
}
