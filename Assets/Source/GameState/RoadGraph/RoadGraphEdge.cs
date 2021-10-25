using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoadGraphEdge {
    public int i, j;
    public int lanes;

    public RoadGraphEdge(int i, int j, int lanes) { 
        this.i = i; this.j = j;
        this.lanes = lanes;
    }
}
