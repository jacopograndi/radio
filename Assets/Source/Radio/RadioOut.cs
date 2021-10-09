using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class RadioOut : MonoBehaviour {

    float[] buffer = new float[4];
    int pos = 0;
    int readpos = 0;
    int channels = 2;

    AudioSource outAudio;

    void Start() {
        outAudio = GetComponent<AudioSource>();
        outAudio.clip = AudioClip.Create("out", 1024, channels, NetRadio.frequency, false);
        outAudio.loop = true;
        outAudio.Play();

        buffer = new float[NetRadio.frequency * 10];
    }

    void Update() {
    }

    public void ConcatBuffer(float[] data, int ch) {
        if (readpos > pos) readpos = pos;
        for (int i = 0; i < data.Length; i++) {
            for (int c = 0; c < channels; c++) {
                buffer[pos] = data[i];
                pos = ClampBuffer(pos);
            }
        }
    }

    int ClampBuffer (int p) {
        p++;
        if (p >= buffer.Length) { p -= buffer.Length; }
        return p;
    }

    void OnAudioFilterRead(float[] data, int channels) {
        for (int n = 0; n < data.Length / 2; ++n) {
            for (int c = 0; c < channels; c++) {
                data[n * 2 + c] += buffer[readpos];
                buffer[readpos] = 0; // clear past
                readpos = ClampBuffer(readpos);
            }
        }
    }
}
