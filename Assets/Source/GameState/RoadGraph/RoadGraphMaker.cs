using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoadGraphMaker : MonoBehaviour {

    public string filePath = "Assets/Resources/Maps/";
    float connectionToleranceSqr = 0.1f;

    public GameObject visualizerNodePrefab;
    public GameObject visualizerEdgePrefab;
    public GameObject visualizerStreetPrefab;

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
        

        foreach (RoadGraphStreet street in graph.streets) {
            var objj = Instantiate(visualizerStreetPrefab) as GameObject;
            objj.transform.SetParent(visualizerHolder.transform);
            RoadGraphNode startj = graph.fromId(street.edges[0].i);
            objj.transform.position = startj.pos;
            objj.GetComponentInChildren<TextMesh>().text = street.name;
            /*
            foreach (RoadGraphEdge edge in street.edges) {
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
            }*/

            bool avenue = false;

            var diff = (graph.fromId(street.edges[0].i).pos 
                - graph.fromId(street.edges[0].j).pos).normalized;
            if (Mathf.Abs(diff.x) > 0.9f) {
                avenue = true;
            } 
            
            if (avenue) objj.transform.rotation = Quaternion.Euler(0, 0, 0);
			else objj.transform.rotation = Quaternion.Euler(0, 90, 0);

            Vector3 minPos = graph.fromId(street.edges[0].i).pos;
            float max = 0, min = float.PositiveInfinity;
            foreach (RoadGraphEdge edge in street.edges) {
                Vector3 start = graph.fromId(edge.i).pos;
                Vector3 end = graph.fromId(edge.j).pos;

                if (avenue) { 
                    max = Mathf.Max(max, start.x);
                    if (start.x < min) {
                        min = start.x;
                        minPos = start;
                    }
                } else { 
                    max = Mathf.Max(max, start.z); 
                    if (start.z < min) {
                        min = start.z;
                        minPos = start;
                    }
                }
            }

            if (avenue) objj.transform.position = minPos - new Vector3(80, 0, 0); //+ new Vector3(1, 0, 0) * (max + 10);
            else objj.transform.position = minPos - new Vector3(0, 0, 80); //+ new Vector3(0, 0, 1) * (max + 10);
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
                    var diff = oth.transform.position - probe;
                    float dist = Vector3.SqrMagnitude(diff);

                    if (dist < connectionToleranceSqr) {
                        graph.edges.Add(new RoadGraphEdge(roadNode.id, oth.id));
                    }
                }
            }
        }


        List<RoadGraphEdge> horizontals = new List<RoadGraphEdge>();
        List<RoadGraphEdge> verticals = new List<RoadGraphEdge>();
        foreach (var edge in graph.edges) {
            var diff = (graph.fromId(edge.i).pos - graph.fromId(edge.j).pos).normalized;
            if (Mathf.Abs(diff.x) > 0.9f) {
                horizontals.Add(edge);
            }
            if (Mathf.Abs(diff.z) > 0.9f) {
                verticals.Add(edge);
            }
        }

        List<List<RoadGraphEdge>> streetHor = new List<List<RoadGraphEdge>>();
        foreach (var edge in horizontals) {
            bool flag = false;
            foreach (var street in streetHor) {
                var found = street.Find(x =>                
                    Mathf.Abs(graph.fromId(x.j).pos.z - graph.fromId(edge.j).pos.z) < 1
                );
                if (found != null) {
                    flag = true;
                    street.Add(edge);
                    break;
				}
            }
            if (!flag) {
                var newStreet = new List<RoadGraphEdge>();
                newStreet.Add(edge);
                streetHor.Add(newStreet);
			}
        }
        List<List<RoadGraphEdge>> streetVer = new List<List<RoadGraphEdge>>();
        foreach (var edge in verticals) {
            bool flag = false;
            foreach (var street in streetVer) {
                var found = street.Find(x =>
                    Mathf.Abs(graph.fromId(x.j).pos.x - graph.fromId(edge.j).pos.x) < 1
                );
                if (found != null) {
                    flag = true;
                    street.Add(edge);
                    break;
				}
            }
            if (!flag) {
                var newStreet = new List<RoadGraphEdge>();
                newStreet.Add(edge);
                streetVer.Add(newStreet);
			}
        }

        streetHor = streetHor.OrderBy(x => graph.fromId(x[0].i).pos.z).ToList();
        streetVer = streetVer.OrderBy(x => graph.fromId(x[0].i).pos.x).ToList();

        for (int i=0; i<streetHor.Count; i++) {
            string roadName = "Avenue " + (i+1);
            graph.streets.Add(new RoadGraphStreet(roadName, streetHor[i]));
        }
        for (int i=0; i<streetVer.Count; i++) {
            string roadName = "Street " + (i+1);
            graph.streets.Add(new RoadGraphStreet(roadName, streetVer[i]));
        }


        var bridges = FindObjectsOfType<BridgeLink>();
        foreach (var bridge in bridges) {
            graph.obstacles.Add(new RoadGraphObstacle(
                bridge.nameId, bridge.timer, bridge.timerMin, bridge.timerMax)
            );
        }

        string json = JsonUtility.ToJson(graph, false);
        saveToDisk(json);
    }

    public void saveToDisk(string raw) {
        string path = filePath + SceneManager.GetActiveScene().name + ".json";
        File.WriteAllText(path, raw);
    }
}
