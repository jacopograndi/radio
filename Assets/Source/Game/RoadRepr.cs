using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadRepr : IEquatable<RoadRepr> {
    public int id;
    public float traffic;

    public void serialize(StreamSerializer serializer) { 
        serializer.append(id);
        serializer.append(traffic);
    }
    public void deserialize(StreamSerializer deserializer) {
        id = deserializer.getNextInt();
        traffic = deserializer.getNextFloat();
    }

    public override bool Equals(object obj) {
        return Equals(obj as RoadRepr);
    }

    public bool Equals(RoadRepr other) {
        return other != null &&
               id == other.id &&
               traffic == other.traffic;
    }
}
