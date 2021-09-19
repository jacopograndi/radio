using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RoadGraphTest {

    [Test]
    public void File_Exists() {
        TextAsset textAsset = Resources.Load("Generated/RoadGraph") as TextAsset;
        Assert.NotNull(textAsset.text);
    }

    [Test]
    public void File_ContainsGraph() {
        TextAsset textAsset = Resources.Load("Generated/RoadGraph") as TextAsset;
        Assert.NotNull(JsonUtility.FromJson<RoadGraph>(textAsset.text));
    }

    [Test]
    public void GenerateTask() {
        TextAsset textAsset = Resources.Load("Generated/RoadGraph") as TextAsset;
        RoadGraph graph = JsonUtility.FromJson<RoadGraph>(textAsset.text);

        GameState gameState = new GameState();

        // testing 1000x to account for stocasticity in generateTask
        for (int i = 0; i < 1000; i++) {
            Task task = gameState.generateTask(graph);
            float distanceStartEnd = Vector3.SqrMagnitude(task.destination - task.start);
            Assert.GreaterOrEqual(distanceStartEnd, GameState.minDistThresholdSqr);
        }
    }

    [Test]
    public void Generate20Tasks() {
        TextAsset textAsset = Resources.Load("Generated/RoadGraph") as TextAsset;
        RoadGraph graph = JsonUtility.FromJson<RoadGraph>(textAsset.text);

        GameState gameState = new GameState();
        gameState.generateTasks(graph, 20);
        Assert.AreEqual(20, gameState.taskList.tasks.Count);
    }
}
