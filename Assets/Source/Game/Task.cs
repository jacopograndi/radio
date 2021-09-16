using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task {

    public string description = "";
    public Vector3 start;
    public Vector3 destination;
    public float timerCompletion;
    public float timerRelease;
    public float timerProcessing;

    public Task (Vector3 start, Vector3 dest) {
        this.start = start; destination = dest;
    }
}
