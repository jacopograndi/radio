using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class GameStateTest {

    GameState TestGameState() {
        GameState gameState = new GameState();
        gameState.timeLeft = 1;
        gameState.acceptTaskRadius = 3;
        gameState.completeTaskRadius = 3;

        PlayerRepr player1 = new PlayerRepr();
        player1.pos = Vector3.zero;
        gameState.addPlayer("Lul", player1);

        return gameState;
    }

    [Test]
    public void IsConstructed() {
        GameState gameState = new GameState();
        Assert.IsNotNull(gameState);
    }

    [Test]
    public void With0Second_IsLost() {
        GameState gameState = new GameState();
        Assert.IsTrue(gameState.isLost());
    }

    [Test]
    public void With1Second_IsNotLost() {
        GameState gameState = new GameState();
        gameState.timeLeft = 1;
        Assert.IsFalse(gameState.isLost());
    }

    [Test]
    public void WithoutTasks_IsWon() {
        GameState gameState = new GameState();
        gameState.timeLeft = 1;
        Assert.IsTrue(gameState.isWon());
    }

    [Test]
    public void WithOnlyACompleteTask_IsWon() {
        GameState gameState = new GameState();
        gameState.timeLeft = 1;
        Task task = new Task();
        task.completed = true;
        gameState.taskList.addTask(task);
        Assert.IsTrue(gameState.isWon());
    }

    [Test]
    public void WithOnlyAnIncompleteTask_IsNotWon() {
        GameState gameState = new GameState();
        gameState.timeLeft = 1;
        Task task = new Task();
        task.completed = false;
        gameState.taskList.addTask(task);
        Assert.IsFalse(gameState.isWon());
    }

    [Test]
    public void WithACompleteAndAnIncompleteTask_IsNotWon() {
        GameState gameState = new GameState();
        gameState.timeLeft = 1;
        Task task1 = new Task();
        task1.completed = true;
        gameState.taskList.addTask(task1);
        Task task2 = new Task();
        task2.completed = false;
        gameState.taskList.addTask(task2);
        Assert.IsFalse(gameState.isWon());
    }

    [Test]
    public void With2CompletedTasks_IsWon() {
        GameState gameState = new GameState();
        gameState.timeLeft = 1;
        Task task1 = new Task();
        task1.completed = true;
        gameState.taskList.addTask(task1);
        Task task2 = new Task();
        task2.completed = true;
        gameState.taskList.addTask(task2);
        Assert.IsTrue(gameState.isWon());
    }

    [Test]
    public void Lost_IsNotWon() {
        GameState gameState = new GameState();
        Assert.IsFalse(gameState.isWon());
    }

    [Test]
    public void AddsPlayer() {
        GameState gameState = new GameState();
        PlayerRepr player = new PlayerRepr();
        player.pos = Vector3.one;
        gameState.addPlayer("Lul", player);
        Assert.AreEqual(1, gameState.playerList.players.Count);
    }

    [Test]
    public void GetsPlayer() {
        GameState gameState = new GameState();
        PlayerRepr player = new PlayerRepr();
        player.pos = Vector3.one;
        gameState.addPlayer("Lul", player);
        Assert.AreEqual(gameState.getPlayer("Lul"), player);
    }

    [Test]
    public void GetsUndefinedPlayer() {
        GameState gameState = new GameState();
        Assert.IsNull(gameState.getPlayer("Lul"));
    }

    [Test]
    public void RefreshesPositionOfAnUndefinedPlayer() {
        GameState gameState = new GameState();
        gameState.refreshPlayerPosition("Lul", Vector3.one);
    }

    [Test]
    public void RefreshPlayerPosition() {
        GameState gameState = TestGameState();
        gameState.refreshPlayerPosition("Lul", Vector3.one);
        Assert.AreEqual(gameState.getPlayer("Lul").pos, Vector3.one);
    }

    [Test]
    public void RefreshPlayerTwoPosition() {
        GameState gameState = TestGameState();

        PlayerRepr player2 = new PlayerRepr();
        player2.pos = Vector3.zero;
        gameState.addPlayer("Omega", player2);
        gameState.refreshPlayerPosition("Omega", Vector3.forward);
        Assert.AreEqual(gameState.getPlayer("Omega").pos, Vector3.forward);
    }

    [Test]
    public void RefreshPlayerTwo_DoestChangePlayerOne() {
        GameState gameState = TestGameState();

        PlayerRepr player2 = new PlayerRepr();
        player2.pos = Vector3.zero;
        gameState.addPlayer("Omega", player2);
        gameState.refreshPlayerPosition("Omega", Vector3.forward);
        Assert.AreEqual(gameState.getPlayer("Lul").pos, Vector3.zero);
    }

    [Test]
    public void PlayerGetNearestTask() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.zero;
        gameState.taskList.addTask(task1);

        Task task = gameState.getNearestStart("Lul");
        Assert.AreEqual(task1, task);
    }

    [Test]
    public void PlayerGetNearestTask_With2Tasks_SecondNearest() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.one;
        gameState.taskList.addTask(task1);

        Task task2 = new Task(); task2.completed = false;
        task2.start = Vector3.zero;
        gameState.taskList.addTask(task2);

        Task task = gameState.getNearestStart("Lul");
        Assert.AreEqual(task2, task);
    }

    [Test]
    public void PlayerGetNearestTask_With2Tasks_FirstNearest() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.zero;
        gameState.taskList.addTask(task1);

        Task task2 = new Task(); task2.completed = false;
        task2.start = Vector3.one;
        gameState.taskList.addTask(task2);

        Task task = gameState.getNearestStart("Lul");
        Assert.AreEqual(task1, task);
    }

    [Test]
    public void PlayerGetNearestTask_With2Tasks_PlayerNotInZero() {
        GameState gameState = TestGameState();
        gameState.refreshPlayerPosition("Lul", Vector3.one);

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.zero;
        gameState.taskList.addTask(task1);

        Task task2 = new Task(); task2.completed = false;
        task2.start = Vector3.one;
        gameState.taskList.addTask(task2);

        Task task = gameState.getNearestStart("Lul");
        Assert.AreEqual(task2, task);
    }

    [Test]
    public void PlayerGetNearest_OnlyUncompletedTasks() {
        GameState gameState = TestGameState();
        gameState.refreshPlayerPosition("Lul", Vector3.one);

        Task task1 = new Task(); task1.completed = true;

        Task task = gameState.getNearestStart("Lul");
        Assert.AreEqual(null, task);
    }

    [Test]
    public void PlayerAcceptTaskInRadius() {
        GameState gameState = TestGameState();
        gameState.refreshPlayerPosition("Lul", Vector3.one * 3.1f);

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.zero;
        gameState.taskList.addTask(task1);

        Task task = gameState.getNearestStart("Lul");
        Assert.AreEqual(null, task);
    }

    [Test]
    public void PlayerAcceptTaskInRadius_TwoTasks() {
        GameState gameState = TestGameState();
        gameState.refreshPlayerPosition("Lul", Vector3.one * 3.1f);

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.zero;
        gameState.taskList.addTask(task1);

        Task task2 = new Task(); task2.completed = false;
        task2.start = Vector3.one * 2;
        gameState.taskList.addTask(task2);

        Task task = gameState.getNearestStart("Lul");
        Assert.AreEqual(task2, task);
    }

    [Test]
    public void PlayerCanAcceptTask() {
        GameState gameState = TestGameState();
        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.zero;
        gameState.taskList.addTask(task1);
        Assert.IsTrue(gameState.canPlayerAcceptTask("Lul"));
    }

    [Test]
    public void PlayerCantAcceptFarTask() {
        GameState gameState = TestGameState();
        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 1000f;
        gameState.taskList.addTask(task1);
        Assert.IsFalse(gameState.canPlayerAcceptTask("Lul"));
    }

    [Test]
    public void PlayerCantAccept2Tasks() {
        GameState gameState = TestGameState();
        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.zero;
        gameState.taskList.addTask(task1);
        gameState.getPlayer("Lul").acceptedTaskId = task1.id;
        Assert.IsFalse(gameState.canPlayerAcceptTask("Lul"));
    }

    [Test]
    public void PlayerAcceptsTaskByMoving() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 20;
        gameState.taskList.addTask(task1);

        gameState.refreshPlayerPosition("Lul", Vector3.forward * 1);
        Assert.AreEqual(gameState.getPlayer("Lul").acceptedTaskId, task1.id);
    }

    [Test]
    public void PlayerAcceptsTaskByMoving_2Tasks() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.zero;
        gameState.taskList.addTask(task1);

        Task task2 = new Task(); task2.completed = false;
        task2.start = Vector3.forward * 4;
        gameState.taskList.addTask(task2);

        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);
        Assert.AreEqual(gameState.getPlayer("Lul").acceptedTaskId, task2.id);
    }

    [Test]
    public void PlayerAcceptsTaskByMoving_AlreadyAccepted() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        gameState.taskList.addTask(task1);

        Task task2 = new Task(); task2.completed = false;
        task2.start = Vector3.forward * 8;
        gameState.taskList.addTask(task2);

        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 8);

        Assert.AreEqual(gameState.getPlayer("Lul").acceptedTaskId, task1.id);
    }

    [Test]
    public void CanPlayerCompleteTask() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        gameState.taskList.addTask(task1);

        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);
        gameState.getPlayer("Lul").pos = Vector3.forward * 12;

        Assert.IsTrue(gameState.canPlayerCompleteTask("Lul"));
    }

    [Test]
    public void CanPlayerCompleteTask_NotAccepted() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        gameState.taskList.addTask(task1);

        Assert.IsFalse(gameState.canPlayerCompleteTask("Lul"));
    }

    [Test]
    public void CanPlayerCompleteTask_InRange() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        gameState.taskList.addTask(task1);

        gameState.refreshPlayerPosition("Lul", Vector3.forward * 1);

        Assert.IsFalse(gameState.canPlayerCompleteTask("Lul"));
    }
    
    [Test]
    public void PlayerCompletesTaskByMoving() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        gameState.taskList.addTask(task1);

        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 12);
        Assert.IsTrue(task1.completed);
        Assert.IsTrue(gameState.isWon());
    }

    [Test]
    public void PlayerResetsCompletedTasks() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        gameState.taskList.addTask(task1);

        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 12);
        Assert.IsTrue(gameState.getPlayer("Lul").acceptedTaskId == -1);
    }

    [Test]
    public void CanPlayerComplete2Tasks() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        gameState.taskList.addTask(task1);

        Task task2 = new Task(); task2.completed = false;
        task2.start = Vector3.forward * 12;
        task2.destination = Vector3.forward * 24;
        gameState.taskList.addTask(task2);

        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 12);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 24);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 32);
        Assert.IsTrue(task1.completed);
        Assert.IsTrue(task2.completed);
        Assert.IsTrue(gameState.isWon());
    }

    [Test]
    public void TaskIdAssignment_IsSequential() {
        GameState gameState = TestGameState();
        Task task1 = new Task();
        gameState.taskList.addTask(task1);
        Task task2 = new Task();
        gameState.taskList.addTask(task2);
        Task task3 = new Task();
        gameState.taskList.addTask(task3);
        Assert.AreEqual(task1, gameState.taskList.fromId(0));
        Assert.AreEqual(task2, gameState.taskList.fromId(1));
        Assert.AreEqual(task3, gameState.taskList.fromId(2));
    }
}
