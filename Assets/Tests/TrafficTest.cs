using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class TrafficTest {

    [Test]
    public void TrafficStateExists () {
        foreach (TextAsset textAsset in Resources.LoadAll("Maps")) {
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(textAsset.text);
            var traffic = new TrafficState(graph);
        }
    }

    public void GenerateRails () { 
        foreach (TextAsset textAsset in Resources.LoadAll("Maps")) {
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(textAsset.text);
            var traffic = new TrafficState(graph);
            traffic.generateRails();
        }
    }
}
