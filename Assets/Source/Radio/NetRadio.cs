using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class NetRadio : MonoBehaviour {

	public static int frequency = 48000;
	
    RadioOut radioOut;

    byte[] localMsg = new byte[0];

    TMP_Text labelIn;

	Permanent permanent;

    void Start () {
        //radioOut = FindObjectOfType<RadioOut>();

		permanent = Permanent.get();

        //labelIn = GameObject.Find("LabelInputs").GetComponent<TMP_Text>();
    }

    float[] WhiteNoise (int size) {
        float[] noise = new float[size];
        for (int i = 0; i < size; i++) {
            float u1 = UnityEngine.Random.Range(0, 1f);
            float u2 = UnityEngine.Random.Range(0, 1f);
            float randStdNormal = 
                Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                Mathf.Sin(2.0f * Mathf.PI * u2);
            noise[i] = randStdNormal;
        }
        return noise;
    }

    float[] Mix (IPEndPoint ip = null) {
        int maxsize = 0;

        // cache received packets
        List<Net.Packet> packs = new List<Net.Packet>();
        lock(Net.recv) packs.AddRange(Net.recv);

        // count all inputs and set flag if they are >2
        int inputs = 0;
        bool toomany = false;
        if (packs.Count > 0) {
            List<IPEndPoint> ips = new List<IPEndPoint>();
            foreach (Net.Packet pack in packs) {
                if (!ips.Contains(pack.ip)) {
                    inputs += 1;
                    ips.Add(pack.ip);
                }
            }
            if (localMsg.Length > 0) inputs++;
            if (inputs > 1) toomany = true;
        }


        // get a list of sound sources
        List<float[]> oths = new List<float[]>();
        foreach (Net.Packet pack in packs) {
            if (pack.ip != ip || toomany) {
                float[] vals = new float[pack.data.Length / 4+1];
                Buffer.BlockCopy(pack.data, 0, vals, 0, pack.data.Length);
                oths.Add(vals);
                maxsize = Mathf.Max(maxsize, vals.Length);
            }
        }
        if ((ip != null || toomany) && localMsg.Length > 0) {
            float[] vals = new float[localMsg.Length / 4+1];
            Buffer.BlockCopy(localMsg, 0, vals, 0, localMsg.Length);
            oths.Add(vals);
            maxsize = Mathf.Max(maxsize, vals.Length);
        }

        if (labelIn) labelIn.text = oths.Count.ToString();

        // add noise to sources if more than one
        if (oths.Count > 1) {
            oths.Add(WhiteNoise(maxsize));
        }

        // sum all sources
        float[] mixed = new float[maxsize];
        for (int i = 0; i < mixed.Length; i++) {
            mixed[i] = 0;
        }
        foreach (float[] oth in oths) {
            for (int i = 0; i < oth.Length; i++) {
                mixed[i] += oth[i] / oths.Count;
            }
        }
        return mixed;
    }

    void Update () {
        if (permanent.net.server) {
            foreach (IPEndPoint ip in Net.ips) {
                float[] mixed = Mix(ip); // Mix(null) to be tested
                if (mixed.Length > 0) {
                    byte[] msg = new byte[mixed.Length*4];
                    Buffer.BlockCopy(mixed, 0, msg, 0, msg.Length);
                    permanent.net.sendTo(msg, ip);
                }
            }
        }
		if (!radioOut) radioOut = FindObjectOfType<RadioOut>();
		if (radioOut) radioOut.ConcatBuffer(Mix(null), 2);
        localMsg = new byte[0];
        lock (Net.recv) Net.recv.Clear();
    }

    public void Send (byte[] msg) {
        if (permanent.net.server) {
            localMsg = new byte[msg.Length];
            Buffer.BlockCopy(msg, 0, localMsg, 0, msg.Length);
        } else {
            permanent.net.sendAll(msg);
        }
    }
}
