using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoadGraph {
    public List<RoadGraphObstacle> obstacles = new List<RoadGraphObstacle>();
    public List<RoadGraphNode> nodes = new List<RoadGraphNode>();
    public List<RoadGraphEdge> edges = new List<RoadGraphEdge>();
    public List<RoadGraphStreet> streets = new List<RoadGraphStreet>();

    public RoadGraphNode fromId (int id) {
        return nodes.Find(x => x.id == id);
    }

    public RoadGraphEdge getEdge (int i, int j) {
        return edges.Find(x => (
            (x.i == i && x.j == j) || 
            (x.i == j && x.j == i))
        );
	}

    public List<RoadGraphNode> star (RoadGraphNode node) {
        var star = new List<RoadGraphNode>();
        foreach (var edge in edges) {
            if (edge.i == node.id) star.Add(fromId(edge.j)); 
            if (edge.j == node.id) star.Add(fromId(edge.i)); 
		}
        return star;
	}
}
