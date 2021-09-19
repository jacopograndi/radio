using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task : IEquatable<Task> {

    public int id;
    public string description = "";
    public Vector3 start;
    public Vector3 destination;
    public float timerCompletion;
    public float timerRelease;
    public float timerProcessing;
    public bool completed;

    public Task() {
        completed = false;
    }

    public Task (Vector3 start, Vector3 dest) {
        this.start = start; destination = dest;
        completed = false;
    }

    public void serialize(StreamSerializer serializer) {
        serializer.append(id);
        serializer.append(start);
        serializer.append(destination);
        serializer.append(completed);
    }

    public void deserialize(StreamSerializer deserializer) {
        id = deserializer.getNextInt();
        start = deserializer.getNextVector3();
        destination = deserializer.getNextVector3();
        completed = deserializer.getNextBool();
    }

    public override bool Equals(object obj) {
        return Equals(obj as Task);
    }

    public bool Equals(Task other) {
        return other != null &&
               id == other.id &&
               description == other.description &&
               start.Equals(other.start) &&
               destination.Equals(other.destination) &&
               timerCompletion == other.timerCompletion &&
               timerRelease == other.timerRelease &&
               timerProcessing == other.timerProcessing &&
               completed == other.completed;
    }
}
