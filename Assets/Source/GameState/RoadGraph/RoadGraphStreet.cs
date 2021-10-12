using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoadGraphStreet {
    public List<RoadGraphEdge> edges = new List<RoadGraphEdge>();
    public string name;

    public RoadGraphStreet(string name, List<RoadGraphEdge> edges) {
        this.name = name;
        this.edges = edges;
    }
}
