using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetRadio : MonoBehaviour {

    public static int frequency = 48000;

    public float whiteNoiseVolume = 0.5f;
    public bool radioTooManySources = false;
    public bool talking = false;

    public RadioIn radioIn;
    public RadioOut radioOut;
    Permanent permanent;

    public Dictionary<string, AudioStream> audios = new Dictionary<string, AudioStream>();

    public void addAudio (string id, byte[] packet) {
        StreamSerializer streamSerializer = new StreamSerializer(packet);
        byte[] data = streamSerializer.getNextBytes();

        float[] vals = new float[Mathf.CeilToInt((float)data.Length / sizeof(float))];
        Buffer.BlockCopy(data, 0, vals, 0, data.Length);
        vals[vals.Length - 1] = 0;

        if (!audios.ContainsKey(id)) {
            var stream = new AudioStream(frequency * 2);
            stream.delay = frequency / 2;
            audios.Add(id, stream);
        }

        audios[id].write(vals);

        //print("recvd " + id + " " + vals.Length);
    }

    RadioView radioView;

    void Start () {
        permanent = Permanent.get();

        mixer();
    }
    float[] silence (int size) {
        float[] sil = new float[size];
        for (int i = 0; i < size; i++) sil[i] = 0;
        return sil;
    }

    float[] whiteNoise (int size) {
        float[] noise = new float[size];
        for (int i = 0; i < size; i++) {
            float u1 = UnityEngine.Random.Range(0, 1f);
            float u2 = UnityEngine.Random.Range(0, 1f);
            float randStdNormal =
                Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                Mathf.Sin(2.0f * Mathf.PI * u2);
            noise[i] = randStdNormal * whiteNoiseVolume;
        }
        return noise;
    }

    float[] mix (Dictionary<string, float[]> sources, string id, int amt, bool nois) {
        var noise0 = whiteNoise(amt);

        float[] mixed = new float[amt];
        for (int i = 0; i < mixed.Length; i++) mixed[i] = 0;

        for (int i = 0; i < amt; i++) {
            int num = 0;
            foreach (var pair in sources) {
                if (pair.Key == id) continue;

                float sample = pair.Value[i];
                mixed[i] += sample;
            }
            if (nois) {
                mixed[i] += noise0[i];
                num++;
                num += 15;
            }
            if (num > 0) mixed[i] /= num;
		}

        float[] filtered = filterHiPass(mixed);
        return filtered;
    }

    float[] filterLowPass (float[] samples) {
        float[] result = new float[samples.Length];
        float alpha = 1f;
        result[0] = samples[0];
        for (int i=1; i<samples.Length; i++) {
            result[i] = alpha * (samples[i] + samples[i - 1]);
		}
        return result;
    }

    float[] filterHiPass (float[] samples) {
        float[] result = new float[samples.Length];
        float alpha = 0.5f;
        result[0] = samples[0];
        for (int i=1; i<samples.Length; i++) {
            result[i] = alpha * (result[i - 1] + samples[i] - samples[i - 1]);
		}
        return result;
    }

    void mixer () {
        int tps = 10;
        /*
        float sum = 0;
        foreach (var f in vals) sum += f;
        print(id + "  " + sum);
        */

        if (!radioIn) radioIn = FindObjectOfType<RadioIn>();
        if (radioIn && radioIn.stream != null && radioIn.isOpen()) {
            float[] audio = radioIn.stream.read(frequency / tps);
            float[] audioListen = radioIn.streamListen.read(frequency / tps);
            float[] added = new float[frequency / tps];
            for (int i = 0; i < added.Length; i++) added[i] = audio[i] + audioListen[i] / 2;
            byte[] msg = new byte[added.Length * sizeof(float)];
            Buffer.BlockCopy(added, 0, msg, 0, msg.Length);

            send(msg);
        }

        Dictionary<string, float[]> sources = new Dictionary<string, float[]>();
        int noise = 0;
        foreach (var pair in audios) {
            var read = pair.Value.read(frequency / tps);
			sources.Add(pair.Key, read);

            float sum = 0;
            foreach (var f in read) sum += f * f;
            if (sum > 1) noise++;
        }
        
        radioTooManySources = noise > 1;

		if (!radioOut) radioOut = FindObjectOfType<RadioOut>();
        if (radioOut && radioOut.stream != null) {
            float[] mixed = mix(sources, permanent.net.nameId, frequency / tps, noise > 1);

            float sum = 0;
            foreach (var f in mixed) sum += f;
            

            radioOut.stream.write(mixed);
        }

        if (permanent.net.server && permanent.net.open) {
            foreach (var audio in audios) {
                if (audio.Key == permanent.net.nameId) continue;

                float[] mixed = mix(sources, audio.Key, frequency / tps, noise > 1);
                byte[] msg = new byte[mixed.Length * 4];
                Buffer.BlockCopy(mixed, 0, msg, 0, msg.Length);

                StreamSerializer stream = new StreamSerializer();
                stream.append(msg);
                permanent.net.sendTo(
                    stream.getBytes(),
                    permanent.net.clientMap.fromId(audio.Key), 
                    NetUDP.Protocol.radio
                );
            }
		}

        Invoke(nameof(mixer), 1.0f/tps);
	}

    void Update () {
        talking = radioIn.ptt_pressed || radioIn.ptt;

        if (!radioView) radioView = FindObjectOfType<RadioView>();
        else radioView.Refresh();
    }

    public void send (byte[] msg) {
        if (!permanent.net.open) return;
        if (permanent.net.server) {
            StreamSerializer stream = new StreamSerializer();
            stream.append(msg);
            addAudio(permanent.net.nameId, stream.getBytes());
        } else {
            StreamSerializer stream = new StreamSerializer();
            stream.append(msg);
            permanent.net.send(stream.getBytes(), NetUDP.Protocol.radio);
        }
    }

    public void startStream () {
        radioOut.startStream();
    }
}
