using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadGraphMaker))]
public class RoadGraphMakerEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        RoadGraphMaker gm = (RoadGraphMaker)target;
        if (GUILayout.Button("Generate Graph")) {
            gm.generateGraph();
        }
        if (GUILayout.Button("Generate and Visualize")) {
            gm.visualizeGraph();
        }
        if (GUILayout.Button("Clear Visualization")) {
            gm.clearGraphVisualization();
        }
    }
}
