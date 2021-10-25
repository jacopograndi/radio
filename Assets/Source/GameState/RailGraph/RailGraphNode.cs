using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailGraphNode {
	public int id;
	public int idRoad;
	public Vector3 pos;

	public RailGraphNode (int id, Vector3 pos, int idRoad) {
		this.id = id; this.pos = pos;
		this.idRoad = idRoad;
	}
}
