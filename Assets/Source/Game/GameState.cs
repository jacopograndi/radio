using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameState : IEquatable<GameState> {

    public float timeLeft;

    public float acceptTaskRadius = 3;
    public float completeTaskRadius = 3;

    public TaskList taskList;
    public PlayerReprList playerList;
    public RoadReprList roadList;

    public GameState() {
        taskList = new TaskList();
        playerList = new PlayerReprList();
        roadList = new RoadReprList();
        timeLeft = 0;
    }

    public byte[] serialize() {
        StreamSerializer serializer = new StreamSerializer();
        serializer.append(timeLeft);
        taskList.serialize(serializer);
        playerList.serialize(serializer);
        roadList.serialize(serializer);
        return serializer.getBytes();
    }

    public void deserialize(byte[] raw) {
        StreamSerializer deserializer = new StreamSerializer(raw);
        timeLeft = deserializer.getNextFloat();
        taskList.deserialize(deserializer);
        playerList.deserialize(deserializer);
        roadList.deserialize(deserializer);
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
        addPlayer("Lul", player1);
        PlayerRepr player2 = new PlayerRepr();
        player2.pos = Vector3.zero;
        addPlayer("Omega", player2);
        roadList = new RoadReprList();
        RoadRepr road1 = new RoadRepr();
        road1.id = 0;
        road1.traffic = 0.5f;
        roadList.roads.Add(road1);
        RoadRepr road2 = new RoadRepr();
        road2.id = 1;
        road2.traffic = 0.7f;
        roadList.roads.Add(road2);
    }

    public void addPlayer(string name, PlayerRepr player) {
        playerList.players.Add(name, player);
    }

    public PlayerRepr getPlayer(string name) {
        PlayerRepr player = null;
        playerList.players.TryGetValue(name, out player);
        return player;
    }

    public void refreshPlayerPosition(string name, Vector3 pos) {
        refreshPlayerPosition(getPlayer(name), pos);
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
        return getNearestStart(getPlayer(name));
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
        return canPlayerAcceptTask(getPlayer(name));
    }
    public bool canPlayerAcceptTask(PlayerRepr player) {
        if (player.acceptedTaskId != -1) return false;
        return getNearestStart(player) != null;
    }

    public bool canPlayerCompleteTask(string name) {
        return canPlayerCompleteTask(getPlayer(name));
    }
    public bool canPlayerCompleteTask(PlayerRepr player) {
        if (player.acceptedTaskId == -1) return false;
        float distance = Vector3.SqrMagnitude(
            taskList.fromId(player.acceptedTaskId).destination - player.pos
        );
        return distance <= completeTaskRadius * completeTaskRadius;
    }

    public override bool Equals(object obj) {
        return Equals(obj as GameState);
    }

    public bool Equals(GameState other) {
        return other != null &&
               timeLeft == other.timeLeft &&
               acceptTaskRadius == other.acceptTaskRadius &&
               completeTaskRadius == other.completeTaskRadius &&
               EqualityComparer<TaskList>.Default.Equals(taskList, other.taskList) &&
               EqualityComparer<PlayerReprList>.Default.Equals(playerList, other.playerList) &&
               EqualityComparer<RoadReprList>.Default.Equals(roadList, other.roadList);
    }
}
