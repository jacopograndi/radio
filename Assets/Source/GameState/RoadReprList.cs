using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadReprList : IEquatable<RoadReprList> {
    public List<RoadRepr> roads = new List<RoadRepr>();

    public void serialize(StreamSerializer serializer) {
        serializer.append(roads.Count);
        foreach (RoadRepr road in roads) { 
            road.serialize(serializer); 
        }
    }

    public void deserialize(StreamSerializer deserializer) {
        roads.Clear();
        int count = deserializer.getNextInt();
        for (int i = 0; i < count; i++) {
            RoadRepr road = new RoadRepr();
            road.deserialize(deserializer);
            roads.Add(road);
        }
    }

    public override bool Equals(object obj) {
        return Equals(obj as RoadReprList);
    }

    public bool Equals(RoadReprList other) {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (roads.Count != other.roads.Count) return false;
        foreach (RoadRepr road in roads) {
            if (!other.roads.Contains(road)) return false;
        }
        return true;
    }
}
