using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleTimerList : IEquatable<ObstacleTimerList> {

    public Dictionary<string, ObstacleTimer> timers = new Dictionary<string, ObstacleTimer>();


    public void addTimer(string name, ObstacleTimer timer) {
        timers.Add(name, timer);
    }

    public ObstacleTimer getTimer(string name) {
        ObstacleTimer timer = null;
        timers.TryGetValue(name, out timer);
        return timer;
    }

    public void passTime(float deltaTime) {
        foreach (var pair in timers) {
            pair.Value.passTime(deltaTime);
        }
    }

    public void serialize(StreamSerializer serializer) {
        serializer.append(timers.Count);
        foreach (var pair in timers) {
            serializer.append(pair.Key);
            pair.Value.serialize(serializer);
        }
    }

    public void deserialize(StreamSerializer deserializer) {
        timers.Clear();
        int count = deserializer.getNextInt();
        for (int i = 0; i < count; i++) {
            string name = deserializer.getNextString();
            ObstacleTimer timer = new ObstacleTimer(0, 0, 0);
            timer.deserialize(deserializer);
            timers[name] = timer;
        }
    }

    public override bool Equals(object obj) {
        return Equals(obj as ObstacleTimerList);
    }

    public bool Equals(ObstacleTimerList other) {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (timers.Count != other.timers.Count) return false;
        foreach (var pair in timers) {
            ObstacleTimer timer = null;
            other.timers.TryGetValue(pair.Key, out timer);
            if (timer == null) return false;
            if (!timer.Equals(timers[pair.Key])) return false;
        }
        return true;
    }
}
