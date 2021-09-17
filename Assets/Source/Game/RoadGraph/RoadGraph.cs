using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoadGraph {
    public List<RoadGraphNode> nodes = new List<RoadGraphNode>();
    public List<RoadGraphEdge> edges = new List<RoadGraphEdge>();

    public RoadGraphNode fromId (int id) {
        return nodes.Find(x => x.id == id);
    }
}
