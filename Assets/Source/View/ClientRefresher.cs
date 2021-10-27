using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientRefresher : MonoBehaviour {

	GameStateController controller;
    public GameObject trafficHolder;

    public Dictionary<int, GameObject> carsView = new Dictionary<int, GameObject>();

    void instantiateTrafficCars () {
        var traffic = controller.traffic;
        if (traffic == null) return;
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
