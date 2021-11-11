using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoadGraphObstacle {

    public string name;
    public float time = 0;
    public float min = 0;
    public float max = 0;

    public Vector3 pos;
    public Quaternion rot;
    public float scale;

    public RoadGraphObstacle(string name, float start, float min, float max, Vector3 pos, Quaternion rot, float scale) {
        this.name = name; time = start;
        this.min = min; this.max = max;
        this.pos = pos; this.rot = rot;
        this.scale = scale;
    }
}
