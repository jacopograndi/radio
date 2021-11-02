using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(RoadGraphMaker))]
public class RoadGraphMakerEditor : Editor {

    string mapName = "";

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        RoadGraphMaker gm = (RoadGraphMaker)target;
        if (GUILayout.Button("Generate Graph")) {
            gm.generateGraph();
        }
        if (GUILayout.Button("Generate and Visualize")) {
            gm.clearGraphVisualization();
            gm.generateGraph();
            string path = gm.filePathRoads + SceneManager.GetActiveScene().name + ".json";
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(File.ReadAllText(path));
            gm.visualizeGraph(graph);
		}

		mapName = GUILayout.TextField(mapName, 25);
        if (mapName.Length == 0) mapName = SceneManager.GetActiveScene().name;
        
		if (GUILayout.Button("Visualize")) {
            string path = gm.filePathRoads + mapName + ".json";
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(File.ReadAllText(path));
            gm.visualizeGraph(graph);
        }
        if (GUILayout.Button("Place Signs")) {
            string path = gm.filePathRoads + mapName + ".json";
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(File.ReadAllText(path));
            gm.placeSigns(graph);
        }
        if (GUILayout.Button("Clear Visualization")) {
            gm.clearGraphVisualization();
        }
        if (GUILayout.Button("Traffic Visualize")) {
            string path = gm.filePathRoads + mapName + ".json";
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(File.ReadAllText(path));
            gm.visualizeTrafficGraph(graph);
        }
        if (GUILayout.Button("Traffic step")) {
            gm.stepTraffic(0.1f);
        }
        if (!gm.trafficPreview) {
            if (GUILayout.Button("Traffic start")) {
                gm.trafficPreview = true;
            }
        } else { 
            if (GUILayout.Button("Traffic stop")) {
                gm.trafficPreview = false;
            }
        }
    }
    void OnEnable() { EditorApplication.update += Update; }
    void OnDisable() { EditorApplication.update -= Update; }
 
    void Update() {
        RoadGraphMaker gm = (RoadGraphMaker)target;
        if (gm.traffic == null) gm.trafficPreview = false;
        if (gm.trafficPreview) gm.stepTraffic(0.1f);
    }
}
