using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	}
	public Dictionary<int, LightState> rnodeState = new Dictionary<int, LightState>();

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
