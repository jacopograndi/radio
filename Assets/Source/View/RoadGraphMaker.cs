using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoadGraphMaker : MonoBehaviour {

    public string filePath = "Assets/Resources/Maps/";
    float connectionToleranceSqr = 0.1f;

    public GameObject visualizerNodePrefab;
    public GameObject visualizerEdgePrefab;

    public static float blockDistance4Lane = 62.5f;
    public static float blockDistance8Lane = 70;

    GameObject visualizerHolder;

    public void visualizeGraph (RoadGraph graph) {
        visualizerHolder = new GameObject("RoadGraphVisualizer");

        foreach (RoadGraphNode node in graph.nodes) {
            var obj = Instantiate(visualizerNodePrefab) as GameObject;
            obj.transform.SetParent(visualizerHolder.transform);
            obj.transform.position = node.pos;
        }

        foreach (RoadGraphEdge edge in graph.edges) {
            var obj = Instantiate(visualizerEdgePrefab) as GameObject;
            obj.transform.SetParent(visualizerHolder.transform);
            RoadGraphNode start = graph.fromId(edge.i);
            RoadGraphNode end = graph.fromId(edge.j);
            obj.transform.position = start.pos;
            obj.transform.rotation = Quaternion.LookRotation(
                end.pos - start.pos
            );
            Vector3 scale = obj.transform.localScale;
            scale.z = Vector3.Magnitude(end.pos - start.pos);
            obj.transform.localScale = scale;
        }
    }

    public void clearGraphVisualization() {
        if (visualizerHolder == null) return;
        DestroyImmediate(visualizerHolder);
    }

    public void generateGraph () {
        var dirs = new Vector3[] {
            Vector3.right, Vector3.back, Vector3.left, Vector3.forward
        };

        RoadGraph graph = new RoadGraph();
        RoadNode[] roadNodes = FindObjectsOfType<RoadNode>();
        foreach (RoadNode roadNode in roadNodes) {
            graph.nodes.Add(new RoadGraphNode(roadNode.id, roadNode.transform.position));

            if (!roadNode.name.Contains("Intersection")) continue;

            List<Vector3> probes = new List<Vector3>();
            foreach (Vector3 dir in dirs) {
                probes.Add(roadNode.transform.position
                    + blockDistance4Lane * dir);
                probes.Add(roadNode.transform.position
                    + blockDistance8Lane * dir);
            }

            foreach (RoadNode oth in roadNodes) {
                if (oth.id == roadNode.id) continue;

                foreach (Vector3 probe in probes) {
                    float dist = Vector3.SqrMagnitude(oth.transform.position - probe);
                    if (dist < connectionToleranceSqr) {
                        graph.edges.Add(new RoadGraphEdge(roadNode.id, oth.id));
                    }
                }
            }
        }

        string json = JsonUtility.ToJson(graph, false);
        saveToDisk(json);
    }

    public void saveToDisk(string raw) {
        string path = filePath + SceneManager.GetActiveScene().name + ".json";
        File.WriteAllText(path, raw);
    }
}
