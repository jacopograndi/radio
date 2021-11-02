using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class TrafficTest {

    [Test]
    public void TrafficStateExists () {
        foreach (TextAsset textAsset in Resources.LoadAll("Maps/Roads")) {
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(textAsset.text);
            var traffic = new TrafficState(graph);
        }
    }
    
    [Test]
    public void LoadRailGraph () { 
        foreach (TextAsset textAsset in Resources.LoadAll("Maps/Roads")) {
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(textAsset.text);
            var traffic = new TrafficState(graph);
            
            var railText = Resources.Load("Maps/Rails/"+textAsset.name) as TextAsset;
            var rgu = JsonUtility.FromJson<RailGraphUnindexed>(railText.text);

            traffic.rails = new RailGraph(rgu);
        }
    }
    
    [Test]
    public void SerializeCars () { 
        foreach (TextAsset textAsset in Resources.LoadAll("Maps/Roads")) {
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(textAsset.text);
            
            var railText = Resources.Load("Maps/Rails/"+textAsset.name) as TextAsset;
            var rgu = JsonUtility.FromJson<RailGraphUnindexed>(railText.text);
            
            var traffic = new TrafficState(graph);
            traffic.rails = new RailGraph(rgu);
            traffic.generateCars();

            StreamSerializer serializer = new StreamSerializer();
            traffic.serializeCars(serializer);
            var raw = serializer.getBytes();

            Debug.Log(raw.Length);
            
            var newtraffic = new TrafficState(graph);
            newtraffic.rails = new RailGraph(rgu);
            StreamSerializer deserializer = new StreamSerializer(raw);
            newtraffic.deserializeCars(deserializer);

            foreach (var car in traffic.cars) {
                Assert.True(newtraffic.cars.ContainsKey(car.Key));
			}
        }
    }
}
