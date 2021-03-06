using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailCar {
	public int id;
	public int startNode;
	public int endNode;
	public float relPos;
	public float velocity = 0;
	public float acceleration = 0;

	public bool dirtyPos = true;
	public bool dirtyDir = true;
	public Vector3 absPos = Vector3.zero;
	public Vector3 absDir = Vector3.zero;

	public int seed = 0;
	public int stopLink = -1;

	public override bool Equals(object obj) {
        return Equals(obj as RailCar);
    }

    public bool Equals(RailCar other) {
		return other != null && id == other.id;
    }

	public RailCar () {
	}

	public RailCar (int startNode, int endNode, float relPos, int id) {
		this.startNode = startNode;
		this.endNode = endNode;
		this.relPos = relPos;
		this.id = id;
	}

	public void serialize (StreamSerializer stream) {
		stream.append(id);
		stream.append(startNode);
		stream.append(endNode);
		stream.append(relPos);
		stream.append(velocity);
		stream.append(acceleration);
		stream.append(seed);
		stream.append(stopLink);
	}

	public void deserialize (StreamSerializer stream) {
		id = stream.getNextInt();
		startNode = stream.getNextInt();
		endNode = stream.getNextInt();
		relPos = stream.getNextFloat();
		velocity = stream.getNextFloat();
		acceleration = stream.getNextFloat();
		seed = stream.getNextInt();
		stopLink = stream.getNextInt();
	}
}
