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
}
