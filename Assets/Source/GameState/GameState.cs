using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameState : IEquatable<GameState> {

    public float timeLeft;

    public static float acceptTaskRadius = 3;
    public static float completeTaskRadius = 3;
    public static float minDistThresholdSqr = 0;//500f * 500f;

    public TaskList taskList;
    public PlayerReprList playerList;
    public ObstacleTimerList timerList;

    public GameState() {
        taskList = new TaskList();
        playerList = new PlayerReprList();
        timerList = new ObstacleTimerList();
        timeLeft = 0;
    }

    public byte[] serialize() {
        StreamSerializer serializer = new StreamSerializer();
        serializer.append(timeLeft);
        taskList.serialize(serializer);
        playerList.serialize(serializer);
        timerList.serialize(serializer);
        return serializer.getBytes();
    }

    public void deserialize(byte[] raw) {
        StreamSerializer deserializer = new StreamSerializer(raw);
        timeLeft = deserializer.getNextFloat();
        taskList.deserialize(deserializer);
        playerList.deserialize(deserializer);
        timerList.deserialize(deserializer);
    }

    public void loadExample() {
        timeLeft = 5 * 60 + 2;
        taskList = new TaskList();
        taskList.addTask(new Task(new Vector3(0, 0, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 1, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 2, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 3, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 4, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 5, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 6, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 7, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 8, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 9, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 10, 0), new Vector3(10, 0, 0)));
        taskList.addTask(new Task(new Vector3(0, 11, 0), new Vector3(10, 0, 0)));
        playerList = new PlayerReprList();
        PlayerRepr player1 = new PlayerRepr();
        player1.pos = Vector3.forward * 10;
        playerList.addPlayer("Lul", player1);
        PlayerRepr player2 = new PlayerRepr();
        player2.pos = Vector3.zero;
        playerList.addPlayer("Omega", player2);
    }

    public void passTime (float deltaTime) {
        timeLeft -= deltaTime;
        timerList.passTime(deltaTime);
    }

    public void refreshPlayerPosition(string name, Vector3 pos) {
        refreshPlayerPosition(playerList.getPlayer(name), pos);
    }
    public void refreshPlayerPosition(PlayerRepr player, Vector3 pos) {
        if (player == null) return;
        player.pos = pos;
        if (canPlayerCompleteTask(player)) {
            taskList.fromId(player.acceptedTaskId).completed = true;
            player.acceptedTaskId = -1;
        }
        if (canPlayerAcceptTask(player)) player.acceptedTaskId = getNearestStart(player).id;
    }

    public bool isLost() {
        return timeLeft <= 0;
    }

    public bool isWon() {
        if (isLost()) return false;
        int completedCount = 0;
        foreach (Task task in taskList.tasks) {
            if (task.completed) completedCount++;
        }
        return taskList.tasks.Count == completedCount;
    }


    public Task getNearestStart(string name) {
        return getNearestStart(playerList.getPlayer(name));
    }
    public Task getNearestStart(PlayerRepr player) {
        if (player == null) return null;

        float acceptTaskRadiusSqr = acceptTaskRadius * acceptTaskRadius;
        Task near = null;
        float mindist = float.PositiveInfinity;
        foreach (Task task in taskList.tasks) {
            if (task.completed) continue;
            float distance = Vector3.SqrMagnitude(task.start - player.pos);
            if (distance <= acceptTaskRadiusSqr && distance < mindist) {
                mindist = distance;
                near = task;
            }
        }
        return near;
    }

    public bool canPlayerAcceptTask(string name) {
        return canPlayerAcceptTask(playerList.getPlayer(name));
    }
    public bool canPlayerAcceptTask(PlayerRepr player) {
        if (player.acceptedTaskId != -1) return false;
        return getNearestStart(player) != null;
    }

    public bool canPlayerCompleteTask(string name) {
        return canPlayerCompleteTask(playerList.getPlayer(name));
    }
    public bool canPlayerCompleteTask(PlayerRepr player) {
        if (player.acceptedTaskId == -1) return false;
        float distance = Vector3.SqrMagnitude(
            taskList.fromId(player.acceptedTaskId).destination - player.pos
        );
        return distance <= completeTaskRadius * completeTaskRadius;
    }

    public Task generateTask (RoadGraph graph) {
        Task task = new Task();
        int startIndex = UnityEngine.Random.Range(0, graph.nodes.Count);
        task.start = graph.nodes[startIndex].pos;
        List<RoadGraphNode> farNodes = new List<RoadGraphNode>();
        foreach (var node in graph.nodes) {
            if (Vector3.SqrMagnitude(node.pos - task.start) >= minDistThresholdSqr) {
                farNodes.Add(node);
            }
        }
        int destIndex = UnityEngine.Random.Range(0, farNodes.Count);
        task.destination = farNodes[destIndex].pos;
        return task;
    }

    
    public void generateTasks (RoadGraph graph, int tasknum) {
        for (int i = 0; i < tasknum; i++) {
            Task task = generateTask(graph);
            taskList.addTask(task);
        }
    }

    public override bool Equals(object obj) {
        return Equals(obj as GameState);
    }

    public bool Equals(GameState other) {
        return other != null &&
               timeLeft == other.timeLeft &&
               EqualityComparer<TaskList>.Default.Equals(taskList, other.taskList) &&
               EqualityComparer<PlayerReprList>.Default.Equals(playerList, other.playerList) &&
               EqualityComparer<ObstacleTimerList>.Default.Equals(timerList, other.timerList);
    }
}
