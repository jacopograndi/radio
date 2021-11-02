using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrafficLight {

	public int id;
	public int roadNodeId;

	public float timer;
	public float cycleTime;
	public float redTime;
	public float yellowTime;

	public enum LightColor {
		green, yellow, red
	}

	public struct LightState {
		public int parity;
		public LightState(int d) { parity = d; }

		public void serialize (StreamSerializer stream) { stream.append(parity); }
		public void deserialize (StreamSerializer stream) { parity = stream.getNextInt(); }
	}
	public Dictionary<int, LightState> rnodeState = new Dictionary<int, LightState>();

	[System.Serializable]
	public struct lsSer { public int id; public int parity; }
	public List<lsSer> rnodeSerialize = new List<lsSer>();

	public void serialize (StreamSerializer stream) {
		stream.append(id);
		stream.append(roadNodeId);
		stream.append(timer);
		stream.append(cycleTime);
		stream.append(redTime);
		stream.append(yellowTime);
		stream.append(rnodeState.Count);
		foreach (var pair in rnodeState) {
			stream.append(pair.Key);
			pair.Value.serialize(stream);
		}
	}

	public void deserialize (StreamSerializer stream) {
		id = stream.getNextInt();
		roadNodeId = stream.getNextInt();
		timer = stream.getNextFloat();
		cycleTime = stream.getNextFloat();
		redTime = stream.getNextFloat();
		yellowTime = stream.getNextFloat();
		int rnodeStateCount = stream.getNextInt();
		for (int i= 0; i<rnodeStateCount; i++) {
			int key = roadNodeId = stream.getNextInt();
			LightState state = new LightState();
			state.deserialize(stream);
			rnodeState.Add(key, state);
		}
	}

	public TrafficLight (int id, int roadNodeId) {
		this.id = id;
		this.roadNodeId = roadNodeId;

		cycleTime = 40f;
		redTime = 10f;
		yellowTime = 5f;

		init();
	}

	public void init () {
		this.timer = 0;
	}

    public float clampTime (float t) {
        while (!(t < cycleTime && t >= 0)) {
            if (t < 0) { t += cycleTime; }
            if (t >= cycleTime) { t -= cycleTime; }
        }
		return t;
    }

	public void step (float dt) {
		timer += dt;
		timer = clampTime(timer);
	}

	public LightColor getLightColor (int parity) {
		if (parity == 1 && timer < cycleTime / 2) return LightColor.red;
		if (parity == 0 && timer > cycleTime / 2) return LightColor.red;

		float half = cycleTime * 0.5f;
		float off = timer + parity * half;
		off = clampTime(off);

		if (off < redTime) return LightColor.red;
		if (off >= redTime && off < half-yellowTime) return LightColor.green;
		if (off >= half-yellowTime) return LightColor.yellow;
		return LightColor.red;
	}
}
