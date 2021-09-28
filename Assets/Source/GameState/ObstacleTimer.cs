using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTimer : IEquatable<ObstacleTimer> {

    public float time = 0;
    public float min = 0;
    public float max = 0;

    public ObstacleTimer (float start, float min, float max) {
        time = start;
        this.min = min; this.max = max;
    }

    public void clampTime () {
        float period = max - min;
        while (!(time < max && time >= min)) {
            if (time < min) { time += period; }
            if (time >= max) { time -= period; }
        }
    }

    public void passTime (float deltaTime) {
        time += deltaTime;
        clampTime();
    }

    public void serialize(StreamSerializer serializer) {
        serializer.append(time);
        serializer.append(min);
        serializer.append(max);
    }
    public void deserialize(StreamSerializer deserializer) {
        time = deserializer.getNextFloat();
        min = deserializer.getNextFloat();
        max = deserializer.getNextFloat();
    }

    public override bool Equals(object obj) {
        return Equals(obj as ObstacleTimer);
    }

    public bool Equals(ObstacleTimer other) {
        return other != null &&
               time == other.time &&
               min == other.min &&
               max == other.max;
    }
}
