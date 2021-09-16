using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState {

    public TaskList taskList;
    public float timeLeft;

    public GameState() {
        taskList = new TaskList();
    }

    public void loadExample() {
        taskList = new TaskList();
        taskList.tasks.Add(new Task(new Vector3(0, 0, 0), new Vector3(10, 0, 0)));
        taskList.tasks.Add(new Task(new Vector3(0, 0, 0), new Vector3(10, 0, 0)));
        taskList.tasks.Add(new Task(new Vector3(0, 0, 0), new Vector3(10, 0, 0)));
        taskList.tasks.Add(new Task(new Vector3(0, 0, 0), new Vector3(10, 0, 0)));
        timeLeft = 5*60 + 2;
    }
}
