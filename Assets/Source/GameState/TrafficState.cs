using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficState {

	static float laneDist = 5f;
	public RoadGraph roads;
	public RailGraph rails;
	public List<RailCar> cars = new List<RailCar>();

	public TrafficState (RoadGraph graph) {
		this.roads = graph;
	}

	public void generateRails () {
		Dictionary<int, int> conns = new Dictionary<int, int>();

		int serial = 0;
		rails = new RailGraph();
		foreach (var node in roads.nodes) {
			var intersectionNodes = new Dictionary<RoadGraphNode, List<RailGraphNode>>();
			var nodeDisplace = new Dictionary<int, float>();

			var star = roads.star(node);
			foreach (var conn in star) {
				intersectionNodes.Add(conn, new List<RailGraphNode>());

				float ortWidth = laneDist * 2;
				Vector3 dir = (conn.pos - node.pos).normalized;
				Vector3 ort = Vector3.Cross(dir, Vector3.up);

				float offset = laneDist * 2f;
				foreach (var oth in star) {
					if (oth == conn) continue;
					Vector3 othdir = (oth.pos - node.pos).normalized;
					if (Vector3.Cross(othdir, ort).y < 0.01f) {
						var othedge = roads.getEdge(node.id, oth.id);
						if (othedge.lanes == 8) offset = 4 * laneDist;
					}
				}

				var edge = roads.getEdge(node.id, conn.id);
				for (int i = 0; i < edge.lanes; i++) {
					float displace = (i + 0.5f - edge.lanes / 2.0f) * laneDist;

					Vector3 pos = node.pos + dir * offset + ort * displace;
					var railNode = new RailGraphNode(serial, pos, node.id);
					rails.nodes.Add(railNode);
					intersectionNodes[conn].Add(railNode);
					conns.Add(serial, conn.id);
					nodeDisplace.Add(serial, displace);
					serial++;
				}
			}

			foreach (var conn in star) {
				List<RailGraphNode> oths = new List<RailGraphNode>();
				foreach (var othconn in star) {
					if (othconn == conn) continue;
					foreach (var railoth in intersectionNodes[othconn]) {
						if (nodeDisplace[railoth.id] > 0) continue;
						oths.Add(railoth);
					}
				}

				foreach (var railnode in intersectionNodes[conn]) {
					if (nodeDisplace[railnode.id] < 0) continue;

					foreach (var othconn in star) {
						if (othconn == conn) continue;

						RailGraphNode closest = null;
						var mindist = float.PositiveInfinity;
						foreach (var oth in oths) {
							var dist = -nodeDisplace[oth.id];
							if (dist < mindist) {
								mindist = dist;
								closest = oth;
							}
						}
						if (closest != null) {
							var edge = new RailGraphEdge(railnode.id, closest.id, node.id, node.id);

							Vector3 dir = (conn.pos - node.pos).normalized;
							Vector3 ort = Vector3.Cross(dir, Vector3.up);
							Vector3 othdir = (othconn.pos - node.pos).normalized;
							if (Vector3.Cross(othdir, ort).y < 0.01f) {
								Vector3 arc0 = new Vector3(railnode.pos.x, 0, closest.pos.z);
								Vector3 arc1 = new Vector3(closest.pos.x, 0, railnode.pos.z);
								if ((arc0 - node.pos).sqrMagnitude < (arc1 - node.pos).sqrMagnitude) {
									edge.setArc(arc0);
								} else {
									edge.setArc(arc1);
								}
							}

							rails.edges.Add(edge);
							if (oths.Count > 1) oths.Remove(closest);
						}
					}
				}
			}
		}

		foreach (var edge in roads.edges) {
			List<RailGraphNode> groupStart = rails.nodes.FindAll(x => x.idRoad == edge.i);
			List<RailGraphNode> groupEnd = rails.nodes.FindAll(x => x.idRoad == edge.j);
			foreach (var starter in groupStart) {
				RailGraphNode closest = null;
				var mindist = float.PositiveInfinity;
				foreach (var ender in groupEnd) {
					if (conns[starter.id] != ender.idRoad) continue;
					var dist = Vector3.SqrMagnitude(ender.pos - starter.pos);
					if (dist < mindist) {
						mindist = dist;
						closest = ender;
					}
				}
				if (closest != null) {
					Vector3 roadStart = roads.fromId(starter.idRoad).pos;
					Vector3 roadDest = roads.fromId(closest.idRoad).pos;
					Vector3 diffRoad = roadStart - roadDest;
					Vector3 diffRail = starter.pos - roadStart;
					int i = starter.id, j = closest.id;
					if (Vector3.Cross(diffRail, diffRoad).y < 0) {
						i = closest.id; j = starter.id;
					}

					rails.edges.Add(new RailGraphEdge(i, j, edge.i, edge.j));
					groupEnd.Remove(closest);
				}
			}
		}

		rails.precalc();
	}

	public bool carIntersect (RailCar car) {
		foreach (var oth in cars) {
			if (car == oth) continue;

			float absdist = (carPos(oth) - carPos(car)).sqrMagnitude;
			if (absdist < 3*3) return true;
			/*
			if (car.endNode != oth.endNode) continue;
			if (car.startNode != oth.startNode) continue;

			float dist = Mathf.Abs(oth.relPos - car.relPos);
			if (dist < 0.01f) return true;*/
		}
		return false;
	}

	public Vector3 carPos (RailCar car) {
		Vector3 start = rails.getNode(car.startNode).pos;
		Vector3 end = rails.getNode(car.endNode).pos;
		float amt = car.relPos;
		var pos = amt * end + (1 - amt) * start;
		return pos;
	}

	public void generateCars () {
		int serial = 0;
		cars.Clear();
		var edgeNotArcs = rails.edges.FindAll(x => !x.arc && x.iRoad != x.jRoad);
		
		foreach (var edge in edgeNotArcs) {
			for (int i = 0; i < 5; i++) {
				if (Random.Range(0, 1f) > 0.5f) continue;
				float amt = i/5f;
				var car = new RailCar(edge.i, edge.j, amt, serial); serial++;
				cars.Add(car);
			}
		}
	}

	public void step (float dt) {
		foreach (var car in cars) {
			car.move(dt, rails);
		}
	}
}
