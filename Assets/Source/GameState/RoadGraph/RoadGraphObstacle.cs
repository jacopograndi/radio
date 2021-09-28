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

    public RoadGraphObstacle(string name, float start, float min, float max) {
        this.name = name; time = start;
        this.min = min; this.max = max;
    }
}
