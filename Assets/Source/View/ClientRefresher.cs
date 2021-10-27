using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientRefresher : MonoBehaviour {

	GameStateController controller;
    public GameObject trafficHolder;

    public Dictionary<int, GameObject> carsView = new Dictionary<int, GameObject>();

    void instantiateTrafficCars () {
        var traffic = controller.traffic;
        CarLoader carLoader = FindObjectOfType<CarLoader>();
        carLoader.Load();
        if (!trafficHolder) trafficHolder = new GameObject("TrafficHolder");
        foreach (var car in traffic.cars.Values) {
            Random.InitState(car.seed);
            int carGraphic = Random.Range(0, carLoader.carGraphics.Count);
            var obj = Instantiate(carLoader.carGraphics[carGraphic]);
            obj.transform.SetParent(trafficHolder.transform);
            obj.transform.position = traffic.absPos(car);
            obj.transform.rotation = Quaternion.identity;
            obj.name = "car." + car.id.ToString();
            carsView.Add(car.id, obj);
		}
    }

    void refreshCars () {
        var traffic = controller.traffic;
        foreach (var car in traffic.cars.Values) {
            var pos = traffic.absPos(car);
            carsView[car.id].transform.position = pos;
            Vector3 dir = traffic.absDir(car);
            if (dir.sqrMagnitude == 0) dir = Vector3.right;
            carsView[car.id].transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
		}
	}

	void Refresh () {
		if (!controller) controller = GetComponent<GameStateController>();
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
