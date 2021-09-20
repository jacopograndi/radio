using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyConfiguration {

    public List<string> players = new List<string>();

    public byte[] serialize() {
        StreamSerializer serializer = new StreamSerializer();
        serializer.append(players.Count);
        foreach (string id in players) {
            serializer.append(id);
        }
        return serializer.getBytes();
    }

    public void deserialize(byte[] raw) {
        StreamSerializer deserializer = new StreamSerializer(raw);
        players.Clear();
        int count = deserializer.getNextInt();
        for (int i = 0; i < count; i++) {
            string id = deserializer.getNextString();
            players.Add(id);
        }
    }
}
