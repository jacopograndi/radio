using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigPlayer {

    public string nameId;
    public bool master;

    public ConfigPlayer() { }

    public ConfigPlayer (string id, bool master) {
        nameId = id; this.master = master;
    }

    public void serialize(StreamSerializer serializer) {
        serializer.append(nameId);
        serializer.append(master);
    }
    public void deserialize(StreamSerializer deserializer) {
        nameId = deserializer.getNextString();
        master = deserializer.getNextBool();
    }
}
