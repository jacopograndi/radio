using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioIn : MonoBehaviour {

    int pos = 0;
    int lastPos = 0;

    AudioClip mic;
    public NetRadio netRadio;

    bool ptt_pressed = false;

    public AudioStream stream = new AudioStream(48000*2);

    void Start() {
        mic = Microphone.Start(null, true, 1, NetRadio.frequency);
        stream.delay = 48000/2;
    }

    private void Update() {
        bool ptt = false; // push to talk
        if (Input.GetKey(KeyCode.Space)) { ptt = true; }
        if (Input.GetKeyDown(KeyCode.Return)) { ptt_pressed = !ptt_pressed; }

        if ((pos = Microphone.GetPosition(null)) > 0) {
            if (lastPos > pos) lastPos = 0;

            if (lastPos < pos) {
                int len = (pos - lastPos) * mic.channels;
                float[] samples = new float[len];
                mic.GetData(samples, lastPos);
                lastPos = pos;
                
                if (!ptt && !ptt_pressed) {
					samples = new float[len];
                    for (int i = 0; i < len; i++) samples[i] = 0;
				}

                //if (ptt || ptt_pressed) stream.write(samples);
                stream.write(samples);
            }
        }
    }
}
