using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterRefresher : MonoBehaviour {

    GameObject canvas;

    GameStateController controller;
    public GameObject trafficHolder;
    public GameObject trafficCar;

    public Dictionary<int, GameObject> carsView = new Dictionary<int, GameObject>();
    public Dictionary<int, Renderer> carsRend = new Dictionary<int, Renderer>();

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
	}

    void Refresh() {
        if (!controller) controller = GetComponent<GameStateController>();
        if (!canvas) canvas = GameObject.Find("Canvas");
        canvas.BroadcastMessage("Refresh");

        foreach (var link in controller.playerLinks) {
            link.Refresh();
        }

        if (carsView.Count == 0) {
            instantiateTrafficCars();
		}
        refreshCars();
    }
}
