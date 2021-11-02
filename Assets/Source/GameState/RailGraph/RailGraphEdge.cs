using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RailGraphEdge {
	public int i, j;
	public int iRoad, jRoad;
	public float length;
	
	public bool arc;
	public Vector3 arcCenter;
	public Vector3[] arcPoints;
	public float[] arcSegmentPercent;

	public RailGraphEdge (int i, int j, int iRoad, int jRoad) {
		this.i = i; this.j = j;
		this.iRoad = iRoad; this.jRoad = jRoad; 
	}

	public void setArc (RailGraph graph, Vector3 arc) {
		this.arc = true;
		arcCenter = arc;
		arcPoints = bezierArray(10, graph.getNode(i).pos, arc, graph.getNode(j).pos);
	}

    public Vector3[] bezierArray (int n, Vector3 a, Vector3 b, Vector3 c) {
        Vector3[] points = new Vector3[n];
        for (int i = 0; i < n-1; i++) {
            float t = (float)i / n;
            points[i] = bezier(a, b, c, t);
        }
        points[n-1] = c;
        return points;
	}
	

    public Vector3 bezier (Vector3 a, Vector3 b, Vector3 c, float t) {
		return (1-t)*(1-t)*a + 2*(1-t)*t*b + t*t*c;
	}

    public Vector3 bezierDerivative (Vector3 a, Vector3 b, Vector3 c, float t) {
		return 2*(1-t)*(a-b) + 2*t*(b-c);
	}
}
