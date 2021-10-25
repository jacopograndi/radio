using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailGraphEdge {
	public int i, j;
	public int iRoad, jRoad;
	public bool arc;
	public Vector3 arcCenter;
	public float length;

	public RailGraphEdge (int i, int j, int iRoad, int jRoad) {
		this.i = i; this.j = j;
		this.iRoad = iRoad; this.jRoad = jRoad; 
	}
	public void setArc (Vector3 arc) {
		this.arc = true;
		arcCenter = arc;
	}
}
