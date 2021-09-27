using System.Collections;
using System.Collections.Generic;
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
            string path = gm.filePath + SceneManager.GetActiveScene().name + ".json";
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(File.ReadAllText(path));
            gm.visualizeGraph(graph);
        }

        mapName = GUILayout.TextField(mapName, 25);
        if (GUILayout.Button("Visualize")) {
            string path = gm.filePath + mapName + ".json";
            RoadGraph graph = JsonUtility.FromJson<RoadGraph>(File.ReadAllText(path));
            gm.visualizeGraph(graph);
        }
        if (GUILayout.Button("Clear Visualization")) {
            gm.clearGraphVisualization();
        }
    }
}
