using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReprList : IEquatable<PlayerReprList> {
    public Dictionary<string, PlayerRepr> players = new Dictionary<string, PlayerRepr>();


    public void addPlayer(string name, PlayerRepr player) {
        players.Add(name, player);
    }

    public PlayerRepr getPlayer(string name) {
        PlayerRepr player = null;
        players.TryGetValue(name, out player);
        return player;
    }

    public void serialize(StreamSerializer serializer) {
        serializer.append(players.Count);
        foreach (var pair in players) { 
            serializer.append(pair.Key);
            pair.Value.serialize(serializer);
        }
    }

    public void deserialize(StreamSerializer deserializer) {
        int count = deserializer.getNextInt();
        players.Clear();
        for (int i = 0; i < count; i++) {
            string name = deserializer.getNextString();
            PlayerRepr player = new PlayerRepr();
            player.deserialize(deserializer);
            players[name] = player;
        }
    }

    public override bool Equals(object obj) {
        return Equals(obj as PlayerReprList);
    }

    public bool Equals(PlayerReprList other) {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (players.Count != other.players.Count) return false;
        foreach (var pair in players) {
            PlayerRepr player = null;
            other.players.TryGetValue(pair.Key, out player);
            if (player == null) return false;
            if (!player.Equals(players[pair.Key])) return false;
        }
        return true;
    }
}
