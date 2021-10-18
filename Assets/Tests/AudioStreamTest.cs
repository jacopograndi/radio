using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AudioStreamTest {
    
    public float[] getPayload (int n) {
        float[] payload = new float[n];
        for (int i = 0; i < n; i++) payload[i] = Random.Range(-1f, 1f);
        return payload;
	}

    public float[] getSilence(int n) {
        float[] payload = new float[n];
        for (int i = 0; i < n; i++) payload[i] = 0;
        return payload;
	}

    [Test]
    public void Write () {
        AudioStream stream = new AudioStream(48000);
        float[] payload = getPayload(800);
        stream.write(payload);
        Assert.AreEqual(stream.posWrite, payload.Length);
    }

    [Test]
    public void WriteRead () {
        AudioStream stream = new AudioStream(48000);
        float[] payload = getPayload(3);
        stream.write(payload);
        var read = stream.read(payload.Length);
        Assert.IsTrue(Enumerable.SequenceEqual(read, payload));
    }

    [Test]
    public void ReadSaturation () {
        AudioStream stream = new AudioStream(48000);
        float[] payload = getPayload(3);
        stream.write(payload);
        float[] read1 = stream.read(payload.Length);
        float[] read2 = stream.read(12);
        float[] empty = new float[12];
        for (int i = 0; i < empty.Length; i++) empty[i] = 0;
        Assert.IsTrue(Enumerable.SequenceEqual(read1, payload));
        Assert.IsTrue(Enumerable.SequenceEqual(read2, empty));
    }

    [Test]
    public void WriteTwice () {
        AudioStream stream = new AudioStream(48000);
        float[] payload1 = getPayload(3);
        float[] payload2 = getPayload(4);
        stream.write(payload1);
        stream.write(payload2);
        float[] read1 = stream.read(payload1.Length);
        float[] read2 = stream.read(payload2.Length);
        Assert.IsTrue(Enumerable.SequenceEqual(read1, payload1));
        Assert.IsTrue(Enumerable.SequenceEqual(read2, payload2));
    }

    [Test]
    public void ReadDelay () {
        AudioStream stream = new AudioStream(48000);
        stream.delay = 3;
        float[] payload = getPayload(3);
        stream.write(payload);
        float[] read1 = stream.read(payload.Length);
        float[] read2 = stream.read(payload.Length);

        float[] silence = getSilence(3);
        Assert.IsTrue(Enumerable.SequenceEqual(read1, silence));
        Assert.IsTrue(Enumerable.SequenceEqual(read2, payload));
    }

    [Test]
    public void WriteLoop () {
        AudioStream stream = new AudioStream(5);
        float[] payload1 = getPayload(3);
        float[] payload2 = getPayload(3);
        stream.write(payload1);
        stream.write(payload2);
        float[] read1 = stream.read(payload1.Length);
        float[] read2 = stream.read(payload2.Length);

        Assert.IsTrue(!Enumerable.SequenceEqual(read1, payload1));
        Assert.IsTrue(Enumerable.SequenceEqual(read2, payload2));
    }

    [Test]
    public void ReadThenWrite () {
        AudioStream stream = new AudioStream(5);
        float[] payload1 = getPayload(3);
        float[] payload2 = getPayload(3);
        float[] read0 = stream.read(payload1.Length);
        stream.write(payload1);
        float[] read1 = stream.read(payload1.Length);
        stream.write(payload2);
        float[] read2 = stream.read(payload2.Length);
        
        Assert.IsTrue(Enumerable.SequenceEqual(read0, getSilence(3)));
        Assert.IsTrue(Enumerable.SequenceEqual(read1, payload1));
        Assert.IsTrue(Enumerable.SequenceEqual(read2, payload2));
    }

    [Test]
    public void ReadThenWriteDelay () {
        AudioStream stream = new AudioStream(6);
        stream.delay = 3;
        float[] payload1 = getPayload(3);
        float[] payload2 = getPayload(3);

        stream.write(payload1);
        float[] read1 = stream.read(3);
        stream.write(payload2);
        float[] read2 = stream.read(3);
        float[] read3 = stream.read(3);
        
        float[] silence = getSilence(3);
        Assert.IsTrue(Enumerable.SequenceEqual(read1, silence));
        Assert.IsTrue(Enumerable.SequenceEqual(read2, payload1));
        Assert.IsTrue(Enumerable.SequenceEqual(read3, payload2));
    }

    [Test]
    public void ReadALotThenWriteRead () {
        AudioStream stream = new AudioStream(6);
        stream.delay = 3;
        float[] payload = getPayload(3);
        
        for (int i=0; i<200; i++) stream.read(3);
        stream.write(payload);
        float[] read = stream.read(3);
        
        Assert.IsTrue(Enumerable.SequenceEqual(read, payload));
    }
}
