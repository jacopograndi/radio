using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientRefresher : MonoBehaviour {

    GameObject canvas;

	GameStateController controller;
    public GameObject trafficHolder;

    public Dictionary<int, GameObject> carsView = new Dictionary<int, GameObject>();

    public GameObject trafficLightsHolder;
    public GameObject trafficLightPrefab;
    public GameObject trafficLightPole4Prefab;
    public GameObject trafficLightPole8Prefab;
    public Material black;
    public Material red;
    public Material yellow;
    public Material green;

    public Dictionary<int, Renderer> lightsRend = new Dictionary<int, Renderer>();

    void instantiateTrafficCars () {
        var traffic = controller.traffic;
        if (traffic == null) return;
        if (traffic.cars.Count == 0) return;
        CarLoader carLoader = FindObjectOfType<CarLoader>();
        carLoader.Load();
        if (!trafficHolder) trafficHolder = new GameObject("TrafficHolder");
        foreach (var car in traffic.cars.Values) {
            Random.InitState(car.seed);
            int carGraphic = Random.Range(0, carLoader.carGraphics.Count);
            var obj = Instantiate(carLoader.carGraphics[carGraphic]);
            obj.transform.SetParent(trafficHolder.transform);
            var pos = traffic.absPos(car);
            var dir = traffic.absDir(car);
            obj.transform.position = traffic.absPos(car);
            obj.transform.rotation = Quaternion.identity;
            obj.name = "car." + car.id.ToString();
            carsView.Add(car.id, obj);
		}

        HashSet<Vector3> polePos = new HashSet<Vector3>();
        
        if (!trafficLightsHolder) trafficLightsHolder = new GameObject("TrafficLightsHolder");
        foreach (var node in traffic.rails.nodes) {
            var railnode = traffic.rails.getNode(node.id);
            int roadId = railnode.idRoad;
            var roadnode = controller.graph.fromId(roadId);
            if (traffic.rails.lights.ContainsKey(roadId) 
                && traffic.rails.lights[roadId].rnodeState.ContainsKey(node.id)) 
            {
                var star = traffic.rails.backwardStar(railnode);
                if (star.Count == 0) 
                    continue;

                var lobj = Instantiate(trafficLightPrefab);
                lobj.transform.SetParent(trafficLightsHolder.transform);
                lobj.transform.position = node.pos + Vector3.up * 6;

                var dir = traffic.absDir(star[0].id, node.id, 1);

                lobj.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                lobj.transform.localScale = Vector3.one * 1;
                lightsRend[node.id] = lobj.GetComponentInChildren<Renderer>();
                
                var edge = controller.graph.getEdge(roadId, star[0].idRoad);
                Vector3 ort = Vector3.Cross(dir, Vector3.up);
                float dist = edge.lanes == 4 ? 8 : 16;
                float off = Vector3.Dot(node.pos - roadnode.pos, dir);
                var pos = roadnode.pos;
                pos += dir * off;
                pos += ort * dist;
                if (!polePos.Contains(pos)) {
                    polePos.Add(pos);
                    var prefab = edge.lanes == 4 ? trafficLightPole4Prefab : trafficLightPole8Prefab;
                    var poleobj = Instantiate(prefab);
                    poleobj.transform.SetParent(trafficLightsHolder.transform);
                    poleobj.transform.position = pos;
                    poleobj.transform.rotation = Quaternion.LookRotation(ort, Vector3.up);
                    poleobj.transform.localScale = Vector3.one * 1;
                }
            }
		}
    }

    void refreshCars () {
        Vector3 ppos = controller.getLocalPlayer().transform.position;

        var traffic = controller.traffic;
        if (traffic == null) return;
        foreach (var car in traffic.cars.Values) {
            var pos = traffic.absPos(car);
            if ((ppos - pos).sqrMagnitude < 150 * 150) {
                if (!carsView[car.id].activeSelf) carsView[car.id].SetActive(true);
            } else {
                if (carsView[car.id].activeSelf) carsView[car.id].SetActive(false);
                continue;
			}

            float delta = Time.time - controller.carLastTime;
            float amt = delta / 0.1f;

            var lastCar = controller.carsLast[car.id];
            Vector3 betweenPos = Vector3.Lerp(lastCar.Item1, pos, amt);

            var lastDir = lastCar.Item2;
            if (lastDir.sqrMagnitude == 0) lastDir = Vector3.right;
            var lastRot = Quaternion.LookRotation(lastDir, Vector3.up);

            var dir = traffic.absDir(car);
            if (dir.sqrMagnitude == 0) dir = Vector3.right;
            var rot = Quaternion.LookRotation(dir, Vector3.up);

            var betweenRot = Quaternion.Slerp(lastRot, rot, amt);

            carsView[car.id].transform.position = betweenPos;
            carsView[car.id].transform.rotation = betweenRot;
		}
        
        foreach (var l in traffic.rails.lights.Values) {
            foreach (var node in l.rnodeState.Keys) {
                if (!lightsRend.ContainsKey(node)) 
                    continue;

                int roadId = traffic.rails.getNode(node).idRoad;
                int parity = l.rnodeState[node].parity;
                var col = traffic.rails.lights[roadId].getLightColor(parity);
                if (col == TrafficLight.LightColor.red) {
                    lightsRend[node].materials[1].color = Color.red;
                    lightsRend[node].materials[2].color = Color.black;
                    lightsRend[node].materials[3].color = Color.black;
                }
                if (col == TrafficLight.LightColor.yellow) {
                    lightsRend[node].materials[1].color = Color.black;
                    lightsRend[node].materials[2].color = Color.yellow;
                    lightsRend[node].materials[3].color = Color.black;
                }
                if (col == TrafficLight.LightColor.green) {
                    lightsRend[node].materials[1].color = Color.black;
                    lightsRend[node].materials[2].color = Color.black;
                    lightsRend[node].materials[3].color = Color.green;
                }
			}
		}
	}

	void Refresh () {
		if (!controller) controller = GetComponent<GameStateController>();
        if (!canvas) canvas = GameObject.Find("clientGUI");
        canvas.BroadcastMessage("Refresh");

		foreach (var link in controller.taskLinks) {
			link.Refresh();
		}
		foreach (var link in controller.playerLinks) {
			link.Refresh();
		}
		foreach (var link in controller.bridgeLinks) {
			link.Refresh();
		}

        if (carsView.Count == 0) {
            instantiateTrafficCars();
		}
        refreshCars();
	}
}
