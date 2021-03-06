using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyConfiguration {

    public List<ConfigPlayer> players = new List<ConfigPlayer>();

    public int taskNumber = 3;
    public float gameTime = 300;
    public string mapname = "__noname";
    public float carDensity = 0.5f;
    public int video = 1;
    public int gps = 1;

    public byte[] serialize() {
        StreamSerializer serializer = new StreamSerializer();
        serializer.append(players.Count);
        foreach (var player in players) {
            player.serialize(serializer);
        }
        serializer.append(taskNumber);
        serializer.append(gameTime);
        serializer.append(mapname);
        serializer.append(carDensity);
        serializer.append(video);
        serializer.append(gps);
        return serializer.getBytes();
    }

    public void deserialize(byte[] raw) {
        StreamSerializer deserializer = new StreamSerializer(raw);
        players.Clear();
        int count = deserializer.getNextInt();
        for (int i = 0; i < count; i++) {
            ConfigPlayer player = new ConfigPlayer();
            player.deserialize(deserializer);
            players.Add(player);
        }
        taskNumber = deserializer.getNextInt();
        gameTime = deserializer.getNextFloat();
        mapname = deserializer.getNextString();
        carDensity = deserializer.getNextFloat();
        video = deserializer.getNextInt();
        gps = deserializer.getNextInt();
    }
}
