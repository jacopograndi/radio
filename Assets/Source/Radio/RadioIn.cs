using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioIn : MonoBehaviour {

    float[] buffer;
    int pos = 0;
    int lastPos = 0;
    int frequency = 48000;

    AudioClip mic;
    NetRadio netRadio;

    bool ptt_pressed = false;

    void Start() {
        netRadio = FindObjectOfType<NetRadio>();
        mic = Microphone.Start(null, true, 1, frequency);

        buffer = new float[frequency];
    }

    private void Update() {
        bool ptt = false; // push to talk
        if (Input.GetKey(KeyCode.Space)) { ptt = true; }
        if (Input.GetKeyDown(KeyCode.Return)) { ptt_pressed = !ptt_pressed; }

        if ((pos = Microphone.GetPosition(null)) > 0) {
            if (lastPos > pos) lastPos = 0;

            if (pos - lastPos > 0) {
                int len = (pos - lastPos) * mic.channels;
                float[] samples = new float[len];
                mic.GetData(samples, lastPos);
                byte[] msg = new byte[samples.Length * 4];
                Buffer.BlockCopy(samples, 0, msg, 0, msg.Length);
                if (ptt || ptt_pressed) netRadio.Send(msg);

                lastPos = pos;
            }
        }
    }
}
