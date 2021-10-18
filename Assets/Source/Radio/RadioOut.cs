using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class RadioOut : MonoBehaviour {

    int channels = 2;

    public AudioStream stream;

    AudioSource outAudio;

    void Start() {
        outAudio = GetComponent<AudioSource>();
        outAudio.clip = AudioClip.Create("out", NetRadio.frequency, channels, NetRadio.frequency, false);
        outAudio.Play();
    }

	public void startStream () {
        stream = new AudioStream(NetRadio.frequency*10);
        stream.delay = NetRadio.frequency / 2; 
	}

	void OnAudioFilterRead(float[] data, int channels) {
        if (stream == null) return;

        float[] buf = stream.read(data.Length/2);

        int pos = 0;
        for (int n = 0; n < data.Length / channels; n++) {
            for (int c = 0; c < channels; c++) {
                data[n * channels + c] += buf[pos];
            }
            pos++;
        }
    }
}
