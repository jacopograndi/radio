using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailCar {
	public int id;
	public int startNode;
	public int endNode;
	public float relPos;
	public float velocity = 0;
	Vector3 absPos = Vector3.zero;

	public RailCar () {

	}

	public RailCar (int startNode, int endNode, float relPos, int id) {
		this.startNode = startNode;
		this.endNode = endNode;
		this.relPos = relPos;
		this.id = id;
	}

	public Vector3 getAbsPos (RailGraph graph) {
		if (absPos == Vector3.zero) {
			Vector3 start = graph.getNode(startNode).pos;
			Vector3 end = graph.getNode(endNode).pos;
			float amt = relPos;
			absPos = amt * end + (1 - amt) * start;
		}
		return absPos;
	}

	public void move (float dt, RailGraph graph) {
		relPos += dt / graph.getEdge(startNode, endNode).length * 5;
		if (relPos > 1) {
			relPos -= 1;
			var star = graph.stars[graph.getNode(endNode)];
			if (star.Count > 0) {
				startNode = endNode;
				endNode = star[Random.Range(0, star.Count)].id;
			} else {
				absPos = Vector3.zero;
				Debug.Log("dead end, " + getAbsPos(graph));
			}
		}
		absPos = Vector3.zero;
	}
}
