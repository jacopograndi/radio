using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskListUI : MonoBehaviour {

    public GameObject taskUI;

    public GameStateController gameStateComp;

    void Clear() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }

    void Fill() {
        if (gameStateComp.gameState == null) return;
        
        Items items = JsonUtility.FromJson<Items>(Resources.Load<TextAsset>("items").text);
        foreach (Task task in gameStateComp.gameState.taskList.tasks) {
            GameObject taskUIobj = Instantiate(taskUI, transform);
            var tmptext = taskUIobj.transform.Find("TaskLabel").GetComponent<TMP_Text>();
            tmptext.text = "Deliver a " + items.items.Find(x => x.id == task.itemId).name;
            taskUIobj.GetComponent<TaskLink>().taskId = task.id;
        }
    } 

    public void Refresh() {
        if (!gameStateComp) gameStateComp = FindObjectOfType<GameStateController>();
        if (transform.childCount == 0) {
            Clear();
            Fill();
        }
    }
}
