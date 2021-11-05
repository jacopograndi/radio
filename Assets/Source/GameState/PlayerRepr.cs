using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRepr : IEquatable<PlayerRepr> {
    public Vector3 pos;
    public int acceptedTaskId;
    public int lives;
    public float bonkCooldown;

    public PlayerRepr() {
        acceptedTaskId = -1;
        lives = 0;
        bonkCooldown = 0;
    }

    public void serialize(StreamSerializer serializer) {
        serializer.append(pos);
        serializer.append(acceptedTaskId);
        serializer.append(lives);
        serializer.append(bonkCooldown);
    }

    public void deserialize(StreamSerializer deserializer) {
        pos = deserializer.getNextVector3();
        acceptedTaskId = deserializer.getNextInt();
        lives = deserializer.getNextInt();
        bonkCooldown = deserializer.getNextFloat();
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
