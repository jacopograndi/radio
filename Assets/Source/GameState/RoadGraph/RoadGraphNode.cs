using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoadGraphNode {

    public int id;
    public Vector3 pos;

    public RoadGraphNode(int id, Vector3 pos) {
        this.id = id;
        this.pos = pos;
    }
}
