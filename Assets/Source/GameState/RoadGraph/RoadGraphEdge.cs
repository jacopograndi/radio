using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoadGraphEdge {
    public int i, j;
    public RoadGraphEdge(int i, int j) { 
        this.i = i; this.j = j; 
    }
}
