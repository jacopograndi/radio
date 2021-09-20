using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class StreamSerializerTest {
    
    [Test]
    public void EmptyStream_HasZeroSize() {
        StreamSerializer streamSerializer = new StreamSerializer();
        Assert.AreEqual(0, streamSerializer.getByteSize());
    }

    [Test]
    public void AppendToStream() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append(1.0f);
        Assert.AreEqual(sizeof(float), streamSerializer.getByteSize());
    }

    [Test]
    public void StreamGetNextFromEmpty() {
        StreamSerializer streamSerializer = new StreamSerializer();
        Assert.Throws<StreamSerializer.OverflowException>(delegate 
            { streamSerializer.getNextFloat(); });
    }

    [Test]
    public void AppendAndGetNext() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append(1.0f);
        float v = streamSerializer.getNextFloat();
        Assert.AreEqual(1.0f, v);
    }

    [Test]
    public void AppendTwiceAndGetNextTwice() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append(1.0f);
        streamSerializer.append(2.0f);
        float v1 = streamSerializer.getNextFloat();
        float v2 = streamSerializer.getNextFloat();
        Assert.AreEqual(1.0f, v1);
        Assert.AreEqual(2.0f, v2);
    }

    [Test]
    public void AppendStringGetNext() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append("Hello World");
        string v = streamSerializer.getNextString();
        Assert.AreEqual("Hello World", v);
    }

    [Test]
    public void AppendStringTwiceAndGetNextTwice() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append("Hello World");
        streamSerializer.append("Good good");
        string s1 = streamSerializer.getNextString();
        string s2 = streamSerializer.getNextString();
        Assert.AreEqual("Hello World", s1);
        Assert.AreEqual("Good good", s2);
    }

    [Test]
    public void AppendMixed() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append("Hello World");
        streamSerializer.append(17.4f);
        streamSerializer.append(34);
        streamSerializer.append("Omegalul");
        string s1 = streamSerializer.getNextString();
        float v = streamSerializer.getNextFloat();
        float d = streamSerializer.getNextInt();
        string s2 = streamSerializer.getNextString();
        Assert.AreEqual("Hello World", s1);
        Assert.AreEqual(17.4f, v);
        Assert.AreEqual(34, d);
        Assert.AreEqual("Omegalul", s2);
    }

    [Test]
    public void AppendAndGetNext_Vector3() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append(new Vector3(3, 5, 6));
        Vector3 vec = streamSerializer.getNextVector3();
        Assert.AreEqual(new Vector3(3, 5, 6), vec);
    }

    [Test]
    public void AppendAndGetNext_Quaternion() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append(Quaternion.Euler(30, 0, 60));
        Quaternion q = streamSerializer.getNextQuaternion();
        Assert.AreEqual(Quaternion.Euler(30, 0, 60), q);
    }

    [Test]
    public void AppendAndGetNext_Bool() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append(true);
        bool q = streamSerializer.getNextBool();
        Assert.AreEqual(true, q);
    }

    [Test]
    public void StreamFromBytes() {
        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append(Quaternion.Euler(30, 0, 60));

        byte[] bytes = streamSerializer.getBytes();
        StreamSerializer fromBytes = new StreamSerializer(bytes);
        Assert.AreEqual(bytes, fromBytes.getBytes());
    }

    [Test]
    public void AppendAndGetNext_Bytes() {
        StreamSerializer fromBytes = new StreamSerializer();
        fromBytes.append(Quaternion.Euler(30, 0, 60));
        byte[] bytes = fromBytes.getBytes();

        StreamSerializer streamSerializer = new StreamSerializer();
        streamSerializer.append(bytes);
        Quaternion q = streamSerializer.getNextQuaternion();
        Assert.AreEqual(Quaternion.Euler(30, 0, 60), q);
    }

    [Test]
    public void TestBounds() {
        StreamSerializer fromBytes = new StreamSerializer();
        for (int i = 0; i < 10; i++)
            fromBytes.append(Quaternion.Euler(30, 0, 60));
    }

    [Test]
    public void SerializeAndDeserialize_GameState() {
        GameState gameState = new GameState();
        gameState.loadExample();

        byte[] raw = gameState.serialize();
        GameState remote = new GameState();
        remote.deserialize(raw);

        Assert.AreEqual(gameState.taskList, remote.taskList);
        Assert.AreEqual(gameState.roadList, remote.roadList);
        Assert.AreEqual(gameState.playerList, remote.playerList);
        Assert.AreEqual(gameState, remote);
    }

    [Test]
    public void GetNextBytes() {
        StreamSerializer streamSerializer = new StreamSerializer();
        Quaternion q = Quaternion.Euler(30, 0, 60);
        streamSerializer.append("Hello");
        streamSerializer.append(q);
        Assert.AreEqual("Hello", streamSerializer.getNextString());
        var des = new StreamSerializer(streamSerializer.getNextBytes());
        Assert.AreEqual(q, des.getNextQuaternion());
    }
}
