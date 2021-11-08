using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TrafficState {

	static float laneDist = 3.5f;
	public RoadGraph roads;
	public RailGraph rails;
	public Dictionary<int, RailCar> cars = new Dictionary<int, RailCar>();

	public CarGridIndex carIndex = new CarGridIndex();

	public TrafficState (RoadGraph graph) {
		this.roads = graph;
	}

	public void generateRails () {
		Dictionary<int, int> conns = new Dictionary<int, int>();

		int serialLight = 0;
		int serial = 0;
		rails = new RailGraph();
		foreach (var node in roads.nodes) {
			var intersectionNodes = new Dictionary<RoadGraphNode, List<RailGraphNode>>();
			var nodeDisplace = new Dictionary<int, float>();

			var star = roads.star(node);

			if (star.Count > 2) {
				TrafficLight tl = new TrafficLight(serialLight, node.id);
				tl.timer = Random.Range(0, tl.cycleTime);
				serialLight++;
				rails.lights.Add(node.id, tl);
			}

			foreach (var conn in star) {
				intersectionNodes.Add(conn, new List<RailGraphNode>());

				float ortWidth = laneDist * 2;
				Vector3 dir = (conn.pos - node.pos).normalized;
				Vector3 ort = Vector3.Cross(dir, Vector3.up);

				float offset = laneDist * 2f + 3;
				foreach (var oth in star) {
					if (oth == conn) continue;
					Vector3 othdir = (oth.pos - node.pos).normalized;
					if (Vector3.Cross(othdir, ort).y < 0.01f) {
						var othedge = roads.getEdge(node.id, oth.id);
						if (othedge.lanes == 8) offset = 4 * laneDist + 3;
					}
				}

				var edge = roads.getEdge(node.id, conn.id);
				for (int i = 0; i < edge.lanes; i++) {
					float displace = (i + 0.5f - edge.lanes / 2.0f) * laneDist;

					Vector3 pos = node.pos + dir * offset + ort * displace;
					var railNode = new RailGraphNode(serial, pos, node.id);
					serial++;
					rails.nodes.Add(railNode);
					intersectionNodes[conn].Add(railNode);
					conns.Add(railNode.id, conn.id);
					nodeDisplace.Add(railNode.id, displace);

					if (rails.lights.ContainsKey(node.id) && displace > 0) {
						int parity = 0;
						if (Vector3.Cross(dir, Vector3.right).sqrMagnitude < 0.1f) {
							parity = 1;
						}
						var lightState = new TrafficLight.LightState(parity);
						rails.lights[node.id].rnodeState[railNode.id] = lightState;
					}
				}
			}

			foreach (var conn in star) {
				var starNoLefts = new List<RoadGraphNode>();

				foreach (var othconn in star) {
					if (othconn == conn) continue;
					if (star.Count > 2) {
						// no left turns
						
						Vector3 dir = (conn.pos - node.pos).normalized;
						Vector3 othdir = (othconn.pos - node.pos).normalized;
						if (Vector3.Cross(dir, othdir).y > 0.01f) {
							continue;
						}
					}
					starNoLefts.Add(othconn);
				}

				List<RailGraphNode> oths = new List<RailGraphNode>();
				foreach (var othconn in starNoLefts) {
					if (othconn == conn) continue;
					foreach (var railoth in intersectionNodes[othconn]) {
						if (nodeDisplace[railoth.id] > 0) continue;

						oths.Add(railoth);
					}
				}

				foreach (var railnode in intersectionNodes[conn]) {
					if (nodeDisplace[railnode.id] < 0) continue;

					foreach (var othconn in starNoLefts) {
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
							Vector3 othdir = (othconn.pos - node.pos).normalized;
							if (Mathf.Abs(Vector3.Cross(dir, othdir).y) > 0.1f) {
								Vector3 arc0 = new Vector3(railnode.pos.x, 0, closest.pos.z);
								Vector3 arc1 = new Vector3(closest.pos.x, 0, railnode.pos.z);
								if ((arc0 - node.pos).sqrMagnitude < (arc1 - node.pos).sqrMagnitude) {
									edge.setArc(rails, arc0);
								} else {
									edge.setArc(rails, arc1);
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

	public Vector3 absPos (int i, int j, float rel) {
		var edge = rails.getEdge(i, j);
		Vector3 start = rails.getNode(i).pos;
		Vector3 end = rails.getNode(j).pos;
		if (edge.arc) {
			float amt = rel;
			return edge.bezier(start, edge.arcCenter, end, amt);
		} else {
			float amt = rel;
			return amt * end + (1 - amt) * start;
		}
	}

	public Vector3 absDir (int i, int j, float rel) {
		var edge = rails.getEdge(i, j);
		Vector3 start = rails.getNode(i).pos;
		Vector3 end = rails.getNode(j).pos;
		if (edge.arc) {
			float amt = rel;
			return edge.bezierDerivative(start, edge.arcCenter, end, amt).normalized;
		} else {
			return (start - end).normalized;
		}
	}
	
	public Vector3 absPos (RailCar car) {
		if (car.dirtyPos) {
			car.absPos = absPos(car.startNode, car.endNode, car.relPos);
			car.dirtyPos = false;
		}
		return car.absPos;
	}

	public Vector3 absDir (RailCar car) {
		if (car.dirtyDir) {
			car.absDir = absDir(car.startNode, car.endNode, car.relPos);
			car.dirtyDir = false;
		}
		return absDir(car.startNode, car.endNode, car.relPos);
	}

	public Vector3 absPos (CarMoveState state) {
		if (state.dirtyPos) {
			state.absPos = absPos(state.startNode, state.endNode, state.relPos);
			state.dirtyPos = false;
		}
		return state.absPos;
	}

	public Vector3 absDir (CarMoveState state) {
		if (state.dirtyDir) {
			state.absDir = absDir(state.startNode, state.endNode, state.relPos);
			state.dirtyDir = false;
		}
		return state.absDir;
	}

	public static float maxVelocity = 10f;
	

	public class CarMoveState {
		public float velocity;
		public int startNode;
		public int endNode;
		public float relPos;
		public int seed;
		public int stopLink;

		public bool dirtyPos = true;
		public bool dirtyDir = true;
		public Vector3 absPos;
		public Vector3 absDir;


		public CarMoveState (RailCar car) {
			this.velocity = car.velocity;
			this.startNode = car.startNode;
			this.endNode = car.endNode;
			this.relPos = car.relPos;
			this.seed = car.seed;
			this.dirtyPos = car.dirtyPos;
			this.dirtyDir = car.dirtyDir;
			this.absPos = car.absPos;
			this.absDir = car.absDir;
			this.stopLink = car.stopLink;
		}

		public CarMoveState (CarMoveState state) {
			this.velocity = state.velocity;
			this.startNode = state.startNode;
			this.endNode = state.endNode;
			this.relPos = state.relPos;
			this.seed = state.seed;
			this.dirtyPos = state.dirtyPos;
			this.dirtyDir = state.dirtyDir;
			this.absPos = state.absPos;
			this.absDir = state.absDir;
			this.stopLink = state.stopLink;
		}

		public void fill (CarMoveState car) {
			velocity = car.velocity;
			startNode = car.startNode;
			endNode = car.endNode;
			relPos = car.relPos;
			seed = car.seed;
			dirtyPos = car.dirtyPos;
			dirtyDir = car.dirtyDir;
			absPos = car.absPos;
			absDir = car.absDir;
			stopLink = car.stopLink;
		}

		public void apply (RailCar car) {
			car.velocity = velocity;
			car.startNode = startNode;
			car.endNode = endNode;
			car.relPos = relPos;
			car.seed = seed;
			car.dirtyPos = dirtyPos;
			car.dirtyDir = dirtyDir;
			car.absPos = absPos;
			car.absDir = absDir;
			car.stopLink = stopLink;
		}
	}

	public void navigateGraph (float dist, CarMoveState state) {
		while (dist > 0) {
			var lightColor = TrafficLight.LightColor.green;
			int roadId = rails.getNode(state.endNode).idRoad;
			if (rails.lights.ContainsKey(roadId) && rails.lights[roadId].rnodeState.ContainsKey(state.endNode)) {
				int parity = rails.lights[roadId].rnodeState[state.endNode].parity;
				lightColor = rails.lights[roadId].getLightColor(parity);
			}

			float len = rails.getEdge(state.startNode, state.endNode).length;
			float cur = len * state.relPos;
			if (cur + dist > len) {
				if (lightColor == TrafficLight.LightColor.red) {
					state.relPos = 1;
					state.dirtyPos = true;
					state.dirtyDir = true;
					state.velocity = 0;
					break; // red semaphore
				}
				dist -= len - cur;
				state.relPos = 0;
				state.dirtyPos = true;
				state.dirtyDir = true;
				var star = rails.stars[rails.getNode(state.endNode)];
				if (star.Count > 0) {
					state.startNode = state.endNode;
					int next = state.seed % star.Count;
					state.seed++;
					state.endNode = star[next].id;
				} else { break; } // dead end
			} else {
				state.relPos += dist / len;
				state.dirtyPos = true;
				state.dirtyDir = true;
				dist = 0;
			}
		}
	}

	public CarMoveState getState (RailCar car) {
		return new CarMoveState(car);
	}

	public CarMoveState carMove (float dt, RailCar car) {
		return carStateMove(dt, car, getState(car));
	}

	public CarMoveState carStateMove (float dt, RailCar car, CarMoveState state) {
		state.velocity += car.acceleration * dt;
		if (state.velocity > maxVelocity) state.velocity = maxVelocity;
		float dist = state.velocity * dt;

		navigateGraph(dist, state);
		return state;
	}

	public void serializeCars (StreamSerializer stream) {
		stream.append(cars.Count);
		foreach (var car in cars.Values) {
			car.serialize(stream);
		}
	}

	public void deserializeCars (StreamSerializer stream) {
		int count = stream.getNextInt();
		for (int i=0; i<count; i++) {
			var car = new RailCar();
			car.deserialize(stream);
			cars.Add(car.id, car);
			carIndex.addCar(this, car);
			movedState[car.id] = new CarMoveState[lookahead];
		}
		initCars();
	}

	public void generateCars (float density = 0.5f) {
		int serial = 0;
		cars.Clear();
		var edgeNotArcs = rails.edges.FindAll(x => !x.arc && x.iRoad != x.jRoad);
		
		foreach (var edge in edgeNotArcs) {
			if (Random.Range(0, 1f) > density) continue;
			int num = (int)(edge.length / 10);
			for (int i = 0; i < num; i++) {
				if (Random.Range(0, 1f) > density) continue;
				float amt = (float)i/num;
				var car = new RailCar(edge.i, edge.j, amt, serial); serial++;
				car.acceleration = Random.Range(1, 1.5f) * 5;
				car.seed = Random.Range(0, 1000000);
				cars.Add(car.id, car);
			}
		}
		initCars();
	}

	public void initCars () {
		foreach (var car in cars.Values) {
			carIndex.addCar(this, car);
			movedState[car.id] = new CarMoveState[lookahead];
			for (int j = 0; j < lookahead; j++) {
				movedState[car.id][j] = new CarMoveState(car);
			}
		}
	}

	ConcurrentDictionary<int, CarMoveState[]> movedState = new ConcurrentDictionary<int, CarMoveState[]>();
	ConcurrentDictionary<int, CarMoveState> nextState = new ConcurrentDictionary<int, CarMoveState>();
	
	int lookahead = 2;

	public void stepMoveStates (int carStart, int carEnd) {
		foreach (var car in cars.Values) {
			if (!(car.id >= carStart && car.id < carEnd)) continue;

			nextState[car.id] = getState(car);

			if (car.stopLink != -1) {
				if (cars[car.stopLink].velocity > 0) {
					nextState[car.id].stopLink = -1;
				} else 
					continue;
			}

			var next = getState(car);
			for (int i = 0; i < lookahead; i++) {
				navigateGraph(5, next);
				movedState[car.id][i].fill(next);
			}
		}
	}
		
	public void stepCheckIntersect (float dt, int carStart, int carEnd) {
		foreach (var car in cars.Values) {
			if (!(car.id >= carStart && car.id < carEnd)) continue;

			if (car.stopLink != -1) continue;

			int stopper = carIntersect(car.id, lookahead);
			if (stopper == -1) {
				nextState[car.id] = carMove(dt, car);
			} else {
				
				nextState[car.id].velocity = 0f;
				if (cars[stopper].velocity < 0.1f) {
					nextState[car.id].stopLink = stopper;
				}
			}
		}
	}

	public void step (float dt) {
		var watch = System.Diagnostics.Stopwatch.StartNew();

		foreach (var light in rails.lights.Values) {
			light.step(dt);
		}
		
		int threadNum = 8;
		int carsPerThread = Mathf.CeilToInt((float)cars.Count / threadNum);

		Parallel.For(0, threadNum,
			i => stepMoveStates(carsPerThread * i, carsPerThread * (i + 1)));
		Parallel.For(0, threadNum,
			i => stepCheckIntersect(dt, carsPerThread * i, carsPerThread * (i + 1)));

		int stopped = 0;
		foreach (var car in cars.Values) { 
			Vector3 lastPos = absPos(car);
			nextState[car.id].apply(car);
			carIndex.movedCar(this, lastPos, car);
			if (car.stopLink != -1) stopped++;
		}

		watch.Stop();
		Debug.Log(cars.Count + " cars, Elapsed " + watch.ElapsedMilliseconds + ", (stopped: " + stopped + ")");
	}

	public int carIntersect (int carId, int lookahead) {
		var car = cars[carId];
		var pos = absPos(car);
		var neighbors = carIndex.neighbors(pos);

		bool onStraight = false;
		/*
		if (rails.getNode(car.startNode).idRoad 
			!= rails.getNode(car.endNode).idRoad 
			&& car.relPos > 0.1f && car.relPos < 0.9f) {
			onStraight = true;
		}*/

		var off = 1.5f;
		foreach (var oth in neighbors) {
			if (carId == oth) continue;
			
			var othcar = cars[oth];

			if (onStraight) {
				if (rails.getNode(othcar.startNode).idRoad
					!= rails.getNode(car.startNode).idRoad)
					continue;
				if (rails.getNode(othcar.endNode).idRoad
					!= rails.getNode(car.endNode).idRoad)
					continue;
			}

			var othpos = absPos(othcar);
			if (!circleInCircle(pos, othpos, 8)) continue;

			for (int i=0; i<lookahead; i++) {
				var fpos = absPos(movedState[car.id][i]);
				var fdir = absDir(movedState[car.id][i]);
				var fothpos = absPos(othcar);
				var fothdir = absDir(othcar);
				
				if (circleInCircle(fpos+fdir*off, fothpos+fothdir*off, 2) 
				 || circleInCircle(fpos+fdir*off, fothpos-fothdir*off, 2)) {
					return oth;
				}
				if (circleInCircle(fpos-fdir*off, fothpos+fothdir*off, 2) 
				 || circleInCircle(fpos-fdir*off, fothpos-fothdir*off, 2)) {
					return oth;
				}
			}
		}
		return -1;
	}

	public bool circleInCircle (Vector3 a, Vector3 b, float rad) {
		return (a - b).sqrMagnitude < rad * rad;
	}
}
