using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoadGraphMaker : MonoBehaviour {

    public string filePathRoads = "Assets/Resources/Maps/Roads/";
    public string filePathRails = "Assets/Resources/Maps/Rails/";
    float connectionToleranceSqr = 0.1f;

    public GameObject visualizerNodePrefab;
    public GameObject visualizerEdgePrefab;
    public GameObject visualizerStreetPrefab;
    public GameObject visualizerBridgePrefab;

    public GameObject visualizerLine;
    public GameObject visualizerCarPrefab;

    public static float blockDistance4Lane = 62.5f;
    public static float blockDistance8Lane = 70;

    public GameObject visualizerHolder;
    GameObject visualizerTrafficHolder;
    GameObject signsHolder;

    public GameObject trafficLightPrefab;
    public GameObject signPolePrefab;
    public GameObject signPrefab;

    public TrafficState traffic;

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
            scale.x = edge.lanes == 8 ? scale.x*2 : scale.x;
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

    public void visualizeTrafficGraph (RoadGraph graph) {
        visualizerTrafficHolder = new GameObject("TrafficVisualizer");

        traffic = new TrafficState(graph);
        traffic.generateRails();
        traffic.generateCars();

        string json = JsonUtility.ToJson(new RailGraphUnindexed(traffic.rails), false);
        saveToDisk(filePathRails, json);

        foreach (var node in traffic.rails.nodes) {
            var obj = Instantiate(visualizerNodePrefab);
            obj.transform.SetParent(visualizerTrafficHolder.transform);
            obj.transform.position = node.pos;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one * 1;
            
            int roadId = traffic.rails.getNode(node.id).idRoad;
            if (traffic.rails.lights.ContainsKey(roadId) && traffic.rails.lights[roadId].rnodeState.ContainsKey(node.id)) {
                var lobj = Instantiate(visualizerNodePrefab);
                lobj.transform.SetParent(visualizerTrafficHolder.transform);
                lobj.transform.position = node.pos + Vector3.up * 5;
                lobj.transform.rotation = Quaternion.identity;
                lobj.transform.localScale = Vector3.one * 1;
                ligths[node.id] = lobj;
            }
		}

        foreach (var edge in traffic.rails.edges) {
            var obj = Instantiate(visualizerLine);
            obj.transform.SetParent(visualizerTrafficHolder.transform);
            obj.transform.position = new Vector3();
            obj.transform.rotation = Quaternion.identity;
            var line = obj.GetComponent<LineRenderer>();
            var start = traffic.rails.getNode(edge.i).pos;
            var dest = traffic.rails.getNode(edge.j).pos;
            if (edge.arc) {
                line.positionCount = edge.arcPoints.Length;
                line.SetPositions(edge.arcPoints);
            } else {
                Vector3[] ps = new Vector3[2] { start, dest };
                line.SetPositions(ps);
            }
            line.startWidth = 1;
            line.endWidth = 0;
		}

        cars.Clear();

        foreach (var car in traffic.cars.Values) {
            var obj = Instantiate(visualizerCarPrefab);
            obj.transform.SetParent(visualizerTrafficHolder.transform);
            obj.transform.position = traffic.absPos(car);
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one * 1;
            obj.name = "car." + car.id.ToString();
            cars.Add(car.id, obj);
		}

        { // grid         
            for (int x = -10; x < 10; x++) {
                var obj = Instantiate(visualizerLine);
                obj.transform.SetParent(visualizerTrafficHolder.transform);
                obj.transform.position = new Vector3();
                obj.transform.rotation = Quaternion.identity;
                var line = obj.GetComponent<LineRenderer>();
                var start = new Vector3(x*32, 0, -10*32);
                var dest = new Vector3(x*32, 0, 10*32);
                Vector3[] ps = new Vector3[2] { start, dest };
                line.SetPositions(ps);
            }
            for (int y = -10; y < 10; y++) {
                var obj = Instantiate(visualizerLine);
                obj.transform.SetParent(visualizerTrafficHolder.transform);
                obj.transform.position = new Vector3();
                obj.transform.rotation = Quaternion.identity;
                var line = obj.GetComponent<LineRenderer>();
                var start = new Vector3(-10*32, 0, y*32);
                var dest = new Vector3(10*32, 0, y*32);
                Vector3[] ps = new Vector3[2] { start, dest };
                line.SetPositions(ps);
            }
        }
	}

    public bool trafficPreview = false;
    Dictionary<int, GameObject> cars = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject> ligths = new Dictionary<int, GameObject>();

    public Material red;
    public Material green;
    public Material yellow;
    
    public void stepTraffic (float dt) {
        traffic.step(dt);
        foreach (var car in traffic.cars.Values) {
            var pos = traffic.absPos(car);
            cars[car.id].transform.position = pos;
            Vector3 dir = traffic.absDir(car);
            if (dir.sqrMagnitude == 0) dir = Vector3.right;
            cars[car.id].transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            var index = traffic.carIndex.indexPos(pos);
		}
        foreach (var l in traffic.rails.lights.Values) {
            foreach (var node in l.rnodeState.Keys) {
                int roadId = traffic.rails.getNode(node).idRoad;
                int parity = l.rnodeState[node].parity;
                var col = traffic.rails.lights[roadId].getLightColor(parity);
                if (col == TrafficLight.LightColor.red) {
                    ligths[node].GetComponent<Renderer>().material = red;
                }
                if (col == TrafficLight.LightColor.yellow) {
                    ligths[node].GetComponent<Renderer>().material = yellow;
                }
                if (col == TrafficLight.LightColor.green) {
                    ligths[node].GetComponent<Renderer>().material = green;
                }
			}
		}
    }

    public void clearGraphVisualization() {
        if (visualizerHolder != null) DestroyImmediate(visualizerHolder);
        if (visualizerTrafficHolder != null) DestroyImmediate(visualizerTrafficHolder);
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
                        graph.edges.Add(new RoadGraphEdge(roadNode.id, oth.id, oth.lanes));
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
                bridge.nameId, bridge.timer, bridge.timerMin, bridge.timerMax, 
                bridge.transform.position, bridge.transform.rotation, bridge.size)
            );
        }

        string json = JsonUtility.ToJson(graph, false);
        saveToDisk(filePathRoads, json);
    }

    public void saveToDisk(string path, string raw) {
        string fp = path + SceneManager.GetActiveScene().name + ".json";
        File.WriteAllText(fp, raw);
    }

    public void placeSigns (RoadGraph graph) {
        signsHolder = GameObject.Find("signsHolder");
        if (!signsHolder) signsHolder = new GameObject("signsHolder");

        float off = 11;
        foreach (var node in graph.nodes) {
            var star = graph.star(node);
            var corners = new List<(RoadGraphNode, RoadGraphNode)>();
            foreach (var conn in star) {
                Vector3 dir = (node.pos - conn.pos).normalized;
                foreach (var oth in star) {
                    if (oth == conn) continue;
                    Vector3 othdir = (node.pos - oth.pos).normalized;
                    if (Vector3.Cross(dir, othdir).y > 0.01f) {
                        corners.Add((conn, oth));
					}
				}
			}
            if (star.Count > 2) {
                foreach (var (conn, oth) in corners) {
                    Vector3 dir = -(node.pos - conn.pos).normalized;
                    Vector3 othdir = -(node.pos - oth.pos).normalized;
                    
                    var edge = graph.getEdge(node.id, conn.id);
                    var othedge = graph.getEdge(node.id, oth.id);
                    Vector3 pos = node.pos;
                    float width = 1;
                    if (edge.lanes == 8) width = 1.5f;
                    float othwidth = 1;
                    if (othedge.lanes == 8) othwidth = 1.5f;
                    pos += dir * off * othwidth;
                    pos += othdir * off * width;

                    var obj = Instantiate(signPolePrefab);
                    obj.transform.SetParent(signsHolder.transform);
                    obj.transform.position = pos;
                    obj.transform.rotation = Quaternion.identity;

                    RoadGraphStreet street = graph.getStreetFromEdge(node.id, conn.id);
                    RoadGraphStreet othstreet = graph.getStreetFromEdge(node.id, oth.id);

                    Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
                    var sign0 = Instantiate(signPrefab);
                    sign0.transform.SetParent(obj.transform);
                    sign0.transform.position = pos + new Vector3(0, 2.1f, 0);
                    sign0.transform.rotation = rot;
                    sign0.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = othstreet.name;
                    
                    Quaternion othrot = Quaternion.LookRotation(othdir, Vector3.up);
                    var sign1 = Instantiate(signPrefab);
                    sign1.transform.SetParent(obj.transform);
                    sign1.transform.position = pos + new Vector3(0, 2.4f, 0);
                    sign1.transform.rotation = othrot;
                    sign1.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = street.name;
                    
				}
			}
		}
	}
}
