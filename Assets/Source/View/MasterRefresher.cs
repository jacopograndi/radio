using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterRefresher : MonoBehaviour {

    GameObject canvas;

    GameStateController controller;
    public GameObject trafficHolder;
    public GameObject trafficCar;

    public GameObject trafficLightsHolder;
    public GameObject trafficLightPrefab;
    public Material red;
    public Material yellow;
    public Material green;

    public Dictionary<int, GameObject> carsView = new Dictionary<int, GameObject>();
    public Dictionary<int, Renderer> carsRend = new Dictionary<int, Renderer>();
    public Dictionary<int, GameObject> lights = new Dictionary<int, GameObject>();

    void instantiateTrafficCars () {
        var traffic = controller.traffic;
        if (traffic == null) return;
        if (!trafficHolder) trafficHolder = new GameObject("TrafficHolder");
        foreach (var car in traffic.cars.Values) {
            var obj = Instantiate(trafficCar);
            obj.transform.SetParent(trafficHolder.transform);
            obj.transform.position = traffic.absPos(car);
            obj.transform.rotation = Quaternion.identity;
            obj.name = "car." + car.id.ToString();
            carsView.Add(car.id, obj);
            carsRend.Add(car.id, obj.GetComponentInChildren<Renderer>());
		}
        
        if (!trafficLightsHolder) trafficLightsHolder = new GameObject("TrafficLightsHolder");
        foreach (var node in traffic.rails.nodes) {
            int roadId = traffic.rails.getNode(node.id).idRoad;
            if (traffic.rails.lights.ContainsKey(roadId) && traffic.rails.lights[roadId].rnodeState.ContainsKey(node.id)) {
                var lobj = Instantiate(trafficLightPrefab);
                lobj.transform.SetParent(trafficLightsHolder.transform);
                lobj.transform.position = node.pos + Vector3.up * 5;
                lobj.transform.rotation = Quaternion.identity;
                lobj.transform.localScale = Vector3.one * 1;
                lights[node.id] = lobj;
            }
		}
    }

    void refreshCars () {
        var traffic = controller.traffic;
        if (traffic == null) return;
        foreach (var car in traffic.cars.Values) {
            var pos = traffic.absPos(car);
            carsView[car.id].transform.position = pos;
            Vector3 dir = traffic.absDir(car);
            if (dir.sqrMagnitude == 0) dir = Vector3.right;
            carsView[car.id].transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            float amt = car.velocity / TrafficState.maxVelocity;
            carsRend[car.id].material.color = Color.Lerp(Color.red, Color.green, amt);
		}
        
        foreach (var l in traffic.rails.lights.Values) {
            foreach (var node in l.rnodeState.Keys) {
                int roadId = traffic.rails.getNode(node).idRoad;
                int parity = l.rnodeState[node].parity;
                var col = traffic.rails.lights[roadId].getLightColor(parity);
                if (col == TrafficLight.LightColor.red) {
                    lights[node].GetComponent<Renderer>().material = red;
                }
                if (col == TrafficLight.LightColor.yellow) {
                    lights[node].GetComponent<Renderer>().material = yellow;
                }
                if (col == TrafficLight.LightColor.green) {
                    lights[node].GetComponent<Renderer>().material = green;
                }
			}
		}
	}

    void Refresh() {
        if (!controller) controller = GetComponent<GameStateController>();
        if (!canvas) canvas = GameObject.Find("masterGUI");
        canvas.BroadcastMessage("Refresh");

        if (controller.permanent.config.gps == 1) {
            foreach (var link in controller.playerLinks) {
                link.Refresh();
            }
        }

        if (carsView.Count == 0) {
            instantiateTrafficCars();
		}
        refreshCars();
    }
}
