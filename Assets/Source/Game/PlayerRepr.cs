using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRepr : IEquatable<PlayerRepr> {
    public Vector3 pos;
    public int acceptedTaskId;

    public PlayerRepr() {
        acceptedTaskId = -1;
    }

    public void serialize(StreamSerializer serializer) {
        serializer.append(pos);
        serializer.append(acceptedTaskId);
    }

    public void deserialize(StreamSerializer deserializer) {
        pos = deserializer.getNextVector3();
        acceptedTaskId = deserializer.getNextInt(); ;
    }

    public override bool Equals(object obj) {
        return Equals(obj as PlayerRepr);
    }

    public bool Equals(PlayerRepr other) {
        return other != null &&
               pos.Equals(other.pos) &&
               acceptedTaskId == other.acceptedTaskId;
    }
}
