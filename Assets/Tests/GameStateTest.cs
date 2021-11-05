using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class GameStateTest {

    public GameState TestGameState() {
        GameState gameState = new GameState();
        gameState.timeLeft = 1;

        PlayerRepr player1 = new PlayerRepr();
        player1.pos = Vector3.zero;
        gameState.playerList.addPlayer("Lul", player1);

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
        gameState.playerList.addPlayer("Lul", player);
        Assert.AreEqual(1, gameState.playerList.players.Count);
    }

    [Test]
    public void GetsPlayer() {
        GameState gameState = new GameState();
        PlayerRepr player = new PlayerRepr();
        player.pos = Vector3.one;
        gameState.playerList.addPlayer("Lul", player);
        Assert.AreEqual(gameState.playerList.getPlayer("Lul"), player);
    }

    [Test]
    public void GetsUndefinedPlayer() {
        GameState gameState = new GameState();
        Assert.IsNull(gameState.playerList.getPlayer("Lul"));
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
        Assert.AreEqual(gameState.playerList.getPlayer("Lul").pos, Vector3.one);
    }

    [Test]
    public void RefreshPlayerTwoPosition() {
        GameState gameState = TestGameState();

        PlayerRepr player2 = new PlayerRepr();
        player2.pos = Vector3.zero;
        gameState.playerList.addPlayer("Omega", player2);
        gameState.refreshPlayerPosition("Omega", Vector3.forward);
        Assert.AreEqual(gameState.playerList.getPlayer("Omega").pos, Vector3.forward);
    }

    [Test]
    public void RefreshPlayerTwo_DoestChangePlayerOne() {
        GameState gameState = TestGameState();

        PlayerRepr player2 = new PlayerRepr();
        player2.pos = Vector3.zero;
        gameState.playerList.addPlayer("Omega", player2);
        gameState.refreshPlayerPosition("Omega", Vector3.forward);
        Assert.AreEqual(gameState.playerList.getPlayer("Lul").pos, Vector3.zero);
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
        gameState.playerList.getPlayer("Lul").acceptedTaskId = task1.id;
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
        Assert.AreEqual(gameState.playerList.getPlayer("Lul").acceptedTaskId, task1.id);
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
        Assert.AreEqual(gameState.playerList.getPlayer("Lul").acceptedTaskId, task2.id);
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

        Assert.AreEqual(gameState.playerList.getPlayer("Lul").acceptedTaskId, task1.id);
    }

    [Test]
    public void CanPlayerCompleteTask() {
        GameState gameState = TestGameState();

        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        gameState.taskList.addTask(task1);

        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);
        gameState.playerList.getPlayer("Lul").pos = Vector3.forward * 12;

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
        Assert.IsTrue(gameState.playerList.getPlayer("Lul").acceptedTaskId == -1);
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

    [Test]
    public void ObstacleTimers() {
        GameState gameState = TestGameState();
        ObstacleTimer timer = new ObstacleTimer(0, 0, 10);
        gameState.timerList.addTimer("bridge1", timer);
        gameState.passTime(1f);
        Assert.AreEqual(1.0f, gameState.timerList.getTimer("bridge1").time);
    }

    [Test]
    public void ObstacleTimers_InBounds() {
        GameState gameState = TestGameState();
        ObstacleTimer timer = new ObstacleTimer(0, 0, 10);
        gameState.timerList.addTimer("bridge1", timer);
        gameState.passTime(12f);
        Assert.AreEqual(2.0f, gameState.timerList.getTimer("bridge1").time);
        gameState.passTime(120f);
        Assert.AreEqual(2.0f, gameState.timerList.getTimer("bridge1").time);
        gameState.passTime(-9f);
        Assert.AreEqual(3.0f, gameState.timerList.getTimer("bridge1").time);
    }

    [Test]
    public void ObstacleTimers_MultipleTimers() {
        GameState gameState = TestGameState();
        gameState.timerList.addTimer("bridge1", new ObstacleTimer(0, 0, 10));
        gameState.timerList.addTimer("bridge2", new ObstacleTimer(0, 0, 4));
        gameState.passTime(11f);
        Assert.AreEqual(1.0f, gameState.timerList.getTimer("bridge1").time);
        Assert.AreEqual(3.0f, gameState.timerList.getTimer("bridge2").time);
    }

    [Test]
    public void TaskItemSerialize () {
        Task task1 = new Task();
        task1.itemId = 1;
        StreamSerializer stream = new StreamSerializer();
        task1.serialize(stream);

        Task task2 = new Task();
        StreamSerializer stream2 = new StreamSerializer(stream.getBytes());
        task2.deserialize(stream2);
        Assert.AreEqual(task1.itemId, task2.itemId);
	}
	/*
    [Test]
    public void CreateItems () {
        string fp = "Assets/Resources/items.json";
        Items items = new Items();
        Item it1 = new Item();
        it1.id = 0;
        it1.name = "test1";
        it1.fragility = 5;
        it1.weight = 1;
        items.items.Add(it1);
        Item it2 = new Item();
        it2.id = 1;
        it2.name = "test1";
        it2.fragility = 4;
        it2.weight = 2;
        items.items.Add(it2);
        File.WriteAllText(fp, JsonUtility.ToJson(items));
    }*/

	[Test]
	public void ItemsJson () {
		JsonUtility.FromJson<Items>(Resources.Load<TextAsset>("items").text);
	}

	[Test]
    public void PlayerBonkNoTask () {
        var gameState = TestGameState();

        int oldlives = gameState.playerList.getPlayer("Lul").lives;
        gameState.playerBonk("Lul");
        Assert.AreEqual(oldlives, gameState.playerList.getPlayer("Lul").lives);
	}
    
	[Test]
    public void PlayerBonk () {
        var gameState = TestGameState();
        
        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        task1.itemId = 0;
        gameState.taskList.addTask(task1);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);

        int oldlives = gameState.playerList.getPlayer("Lul").lives;
        gameState.playerBonk("Lul");
        Assert.AreEqual(oldlives-1, gameState.playerList.getPlayer("Lul").lives);
	}
    
	[Test]
    public void PlayerBonkCooldown () {
        var gameState = TestGameState();
        
        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        task1.itemId = 0;
        gameState.taskList.addTask(task1);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);

        int oldlives = gameState.playerList.getPlayer("Lul").lives;
        gameState.passTime(20);
        gameState.playerBonk("Lul");
        gameState.passTime(1.9f);
        gameState.playerBonk("Lul");
        gameState.playerBonk("Lul");
        gameState.playerBonk("Lul");
        Assert.AreEqual(oldlives-1, gameState.playerList.getPlayer("Lul").lives);
	}
    
	[Test]
    public void PlayerAcceptAssignFragility () {
        var gameState = TestGameState();
        
        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        task1.itemId = 0;
        gameState.taskList.addTask(task1);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);

        var fragility = gameState.items.items.Find(x => x.id == task1.itemId).fragility;
        Assert.AreEqual(fragility, gameState.playerList.getPlayer("Lul").lives);
	}

	[Test]
    public void PlayerBonkFailsTask () {
        var gameState = TestGameState();
        
        Task task1 = new Task(); task1.completed = false;
        task1.start = Vector3.forward * 4;
        task1.destination = Vector3.forward * 12;
        task1.itemId = 0;
        gameState.taskList.addTask(task1);
        gameState.refreshPlayerPosition("Lul", Vector3.forward * 4);

        int oldlives = gameState.playerList.getPlayer("Lul").lives;
        for (int i = 0; i < oldlives + 1; i++) {
            Assert.IsTrue(gameState.playerList.getPlayer("Lul").acceptedTaskId != -1);
            gameState.playerBonk("Lul");
            gameState.passTime(2.1f);
        }
        Assert.IsTrue(gameState.playerList.getPlayer("Lul").acceptedTaskId == -1);
	}
}
