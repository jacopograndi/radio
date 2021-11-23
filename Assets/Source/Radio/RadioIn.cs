using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioIn : MonoBehaviour {

    int pos = 0;
    int lastPos = 0;
    int samplerate = 0;

    AudioClip mic;
    public NetRadio netRadio;
    
    public bool ptt = false;
    public bool ptt_pressed = false;

    public AudioStream stream = new AudioStream(48000*2);
    public AudioStream streamListen = new AudioStream(48000*2);

    bool openMic (int rate) {
        samplerate = rate;
        try {
            mic = Microphone.Start(null, true, 1, rate);
		} catch {
            print("Microphone does not support " + rate);
            return false;
		}
        return true;
	}

    void Start() {
        bool opened = openMic(48000);
        if (!opened) openMic(44100);
        if (!opened) print("Microphone not supported");
        stream.delay = 48000/2;
    }

    float[] resample (float[] samples, int rate) {
        float ratio = 48000.0f/rate;
        int sizeResampled = Mathf.CeilToInt(ratio * samples.Length);
        int dupeSegment = Mathf.CeilToInt(samples.Length / (sizeResampled - samples.Length));
        var res = new List<float>();
        for (int i=0; i<samples.Length; i++) {
            res.Add(samples[i]);
            if (i % dupeSegment == 0) res.Add(samples[i]);
		}
        return res.ToArray();
	}

    public bool isOpen () {
        return ptt || ptt_pressed;
	}

    private void Update() {
        ptt = false; // push to talk
        if (Input.GetKey(KeyCode.Space)) { ptt = true; }
        if (Input.GetKeyDown(KeyCode.Return)) { ptt_pressed = !ptt_pressed; }

        float[] samplesListen = new float[1024];
        AudioListener.GetOutputData(samplesListen, 0);
        streamListen.write(samplesListen);

        if (mic == null) return;
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
                if (samplerate != 48000) samples = resample(samples, samplerate);

                stream.write(samples);
            }
        }
    }
}
