using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGridIndex {

	public class CarBucket {
		public HashSet<int> cars = new HashSet<int>();

		public void add (RailCar car) { cars.Add(car.id); }
		public void remove (RailCar car) { cars.Remove(car.id); }
		public List<int> list () {
			var list = new List<int>();
			foreach (var v in cars) list.Add(v);
			return list; 
		}
	}

	public Dictionary<(int, int), CarBucket> gridIndex = new Dictionary<(int, int), CarBucket>();

	public static int gridSize = 8;
	public (int, int) indexPos (Vector3 pos) {
		int x = Mathf.CeilToInt(pos.x / gridSize);
		int z = Mathf.CeilToInt(pos.z / gridSize);
		return (x, z);
	}

	public void addCar (TrafficState traffic, RailCar car) {
		var key = indexPos(traffic.absPos(car));
		if (!gridIndex.ContainsKey(key)) {
			gridIndex.Add(key, new CarBucket());
		}
		gridIndex[key].add(car);
	}

	public void movedCar (TrafficState traffic, Vector3 lastPos, RailCar car) {
		var oldkey = indexPos(lastPos);
		var newkey = indexPos(traffic.absPos(car.startNode, car.endNode, car.relPos));
		if (oldkey == newkey) return;

		if (!gridIndex.ContainsKey(newkey)) {
			gridIndex.Add(newkey, new CarBucket());
		}
		gridIndex[newkey].add(car);
		gridIndex[oldkey].remove(car);
	}

	public static (int, int)[] dirs = new (int, int)[]
		{ ( 1, 0 ), ( 0, 1 ), ( -1, 0 ), ( 0, -1 ) };

	public List<(int, int)> gridNeighbor ((int, int) key) {
		var list = new List<(int, int)>();
		foreach (var dir in dirs) {
			list.Add((key.Item1 + dir.Item1, key.Item2 + dir.Item2));
		}
		return list;
	}

	public List<int> neighbors (Vector3 pos) {
		var neighs = new List<int>();
		var key = indexPos(pos);
		if (gridIndex.ContainsKey(key)) 
			neighs.AddRange(gridIndex[key].list());

		foreach (var n in gridNeighbor(key)) {
			if (gridIndex.ContainsKey(n)) 
				neighs.AddRange(gridIndex[n].list());
		}
		return neighs;
	}
}
