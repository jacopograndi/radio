using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GenerateMap))]
public class GenerateMapEditor : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GenerateMap gm = (GenerateMap)target;
        if (GUILayout.Button("Generate Map")) {
            gm.Generate();
        }
        if (GUILayout.Button("Replace")) {
            gm.Replace();
        }
        if (GUILayout.Button("Replace Exact")) {
            gm.Replace(true);
        }
        if (GUILayout.Button("Place Roads")) {
            gm.PlaceRoads();
        }
        if (GUILayout.Button("Remove Roads")) {
            gm.RemoveRoads();
        }
        if (GUILayout.Button("Remove Road Duplicates")) {
            gm.RemoveDuplicates();
        }
    }
}