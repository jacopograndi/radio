using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateMap : MonoBehaviour {

    public GameObject blockPrefab;
    public GameObject blockSkyscraperPrefab;
    public GameObject road4Lane;
    public GameObject road8Lane;
    public GameObject road4Intersection;
    public GameObject road8Intersection;
    public GameObject road4To8Intersection;

    public static float blockDistance4Lane = 62.5f;
    public static float blockDistance8Lane = 70;
    
    public int sizex = 3;
    public int sizey = 3;
    public int sizeBlock = 100;
    public int sizeRoad = 25;

    public void Generate() {
        for (int x = 0; x < sizex; x++) {
            for (int y = 0; y < sizey; y++) {
                Vector3 pos = new Vector3(
                    x * (sizeBlock + sizeRoad),
                    0,
                    y * (sizeBlock + sizeRoad)
                );
                GameObject obj = PrefabUtility.InstantiatePrefab(blockPrefab) as GameObject;
                obj.transform.position = pos;
                obj.transform.rotation = Quaternion.identity;
                obj.transform.SetParent(transform);
            }
        }
    }

    public void Replace(bool exact = false) {
        var children = new List<Transform>();
        foreach (Transform child in transform) {
            children.Add(child);
        }

        foreach (Transform child in children) {
            var prefab = blockPrefab;
            if (child.name.Contains("Skyscraper")) { prefab = blockSkyscraperPrefab; }
            GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            obj.transform.position = child.transform.position;
            obj.transform.rotation = child.transform.rotation;
            obj.transform.SetParent(transform);
            if (exact) {
                obj.transform.rotation = child.transform.rotation;
            } else {
                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
            }
            DestroyImmediate(child.gameObject);
        }
    }

    public void PlaceRoads() {
        Transform roadHolder = GameObject.Find("Roads").transform;

        var children = new List<Transform>();
        foreach (Transform child in transform) {
            children.Add(child);
        }

        int serialID = 0;

        var dirs = new Vector3[] {
            Vector3.right, Vector3.back, Vector3.left, Vector3.forward
        };

        var dirsDiag = new Vector3[4,2] {
            { Vector3.right, Vector3.forward },
            { Vector3.right, Vector3.back },
            { Vector3.left, Vector3.forward },
            { Vector3.left, Vector3.back }
        };

        foreach (Transform child in children) {
            var roadtype = new Dictionary<Vector3, int>();

            foreach (var dir in dirs) {
                float mindist = float.PositiveInfinity;
                foreach (Transform oth in children) {
                    if (child == oth) continue;
                    mindist = Mathf.Min(mindist, Vector3.SqrMagnitude(oth.position - child.position - dir * 125));
                }
                GameObject prefab = road4Lane;
                Vector3 pos = child.position;
                if (mindist > 10 && mindist < 300) { 
                    prefab = road8Lane;
                    pos += dir * blockDistance8Lane;
                    roadtype[dir] = 1;
                } else  {
                    pos += dir * blockDistance4Lane;
                    roadtype[dir] = 0;
                }
                Quaternion rot = Quaternion.Euler(0, 0, 0);
                if (dir == Vector3.right || dir == Vector3.left) {
                    rot = Quaternion.Euler(0, 90, 0);
                }
                GameObject lane = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                lane.transform.position = pos;
                lane.transform.rotation = rot;
                lane.transform.SetParent(roadHolder);
                lane.GetComponent<RoadNode>().id = ++serialID;
            }

            for (int i=0; i<4; i++) {
                GameObject prefab = road4Intersection;
                int roadtypeSum = roadtype[dirsDiag[i, 0]] + roadtype[dirsDiag[i, 1]];
                if (roadtypeSum == 2) {
                    prefab = road8Intersection;
                }

                Quaternion rot = Quaternion.Euler(0, 0, 0);
                Vector3 pos = child.position;
                if (roadtype[dirsDiag[i, 0]] == 1) { 
                    pos += dirsDiag[i, 0] * blockDistance8Lane;
                    if (roadtype[dirsDiag[i, 1]] == 0) {
                        prefab = road4To8Intersection;
                        rot = Quaternion.Euler(0, 90, 0);
                    }
                }
                else {
                    pos += dirsDiag[i, 0] * blockDistance4Lane;
                }

                if (roadtype[dirsDiag[i, 1]] == 1) {
                    pos += dirsDiag[i, 1] * blockDistance8Lane;
                    if (roadtype[dirsDiag[i, 0]] == 0) {
                        prefab = road4To8Intersection;
                    }
                } else {
                    pos += dirsDiag[i, 1] * blockDistance4Lane;
                }

                GameObject lane = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                lane.transform.position = pos;
                lane.transform.rotation = rot;
                lane.transform.SetParent(roadHolder);
                lane.GetComponent<RoadNode>().id = ++serialID;
            }
        }

        RemoveDuplicates();
    }

    public void RemoveRoads() {
        var roads = FindObjectsOfType<RoadNode>();
        foreach (RoadNode road in roads) {
            DestroyImmediate(road.gameObject);
        }
    }

    public void RemoveDuplicates() {
        var roads = FindObjectsOfType<RoadNode>();
        bool found = false;
        foreach(RoadNode road in roads) {
            foreach (RoadNode oth in roads) {
                if (road != oth && Vector3.SqrMagnitude(road.transform.position - oth.transform.position) < 1) {
                    DestroyImmediate(oth.gameObject);
                    found = true;
                }
            }
            if (found) break;
        }
        if (found) { RemoveDuplicates(); }
    }
}
