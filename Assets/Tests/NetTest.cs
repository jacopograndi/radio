using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NetTest {

    void sleep (float s) {
        for (int i = 0; i < s * 100000; i++) ;
    }

    NetUDP.Packet waitForPacket (NetUDP net) {
        NetUDP.Packet packet = null;
        int iters = 30;
        while (iters > 0) {
            sleep(1);
            packet = net.pop();
            if (packet != null) break;
            iters--;
        }
        return packet;
    }

    [Test]
    public void MultipleClients() {
        NetUDP server = new NetUDP(49999);
        server.openServer("Omega");
        NetUDP client1 = new NetUDP(49999);
        client1.openClient("Lul", "127.0.0.1");
        NetUDP client2 = new NetUDP(49999);
        client2.openClient("Pepo", "127.0.0.1");

        var bytes1 = new byte[] { 0, 1, 2, 3 };
        client1.send(bytes1);
        var bytes2 = new byte[] { 7, 8, 9, 8 };
        client2.send(bytes2);

        NetUDP.Packet packet1 = waitForPacket(server);
        NetUDP.Packet packet2 = waitForPacket(server);

        var bytes3 = new byte[] { 5, 5, 5, 4 };
        if (packet1 != null && packet2 != null) {
            server.sendAll(bytes3);
        } else Assert.Fail("client packets did not arrive");

        NetUDP.Packet packet3 = waitForPacket(client1);
        NetUDP.Packet packet4 = waitForPacket(client2);

        client1.close();
        client2.close();
        server.close();

        if (packet3 != null && packet4 != null) {
            Assert.AreEqual("Omega", packet3.id);
            Assert.AreEqual("Omega", packet4.id);
        } else Assert.Fail("server packets did not arrive");
    }

    [Test]
    public void SendAndReceive() {
        NetUDP server = new NetUDP(49999);
        server.openServer("Omega");
        NetUDP client = new NetUDP(49999);
        client.openClient("Lul", "127.0.0.1");

        var bytes = new byte[] { 0, 1, 2, 3 };
        client.send(bytes);

        NetUDP.Packet packet = waitForPacket(server);

        client.close();
        server.close();

        if (packet != null) {
            Assert.AreEqual(bytes, packet.data);
        } else Assert.Fail("packet did not arrive");
    }

    [Test]
    public void SendAndReceiveTwice() {
        NetUDP server = new NetUDP(49999);
        server.openServer("Omega");
        NetUDP client = new NetUDP(49999);
        client.openClient("Lul", "127.0.0.1");

        var bytes1 = new byte[] { 0, 1, 2, 3 };
        var bytes2 = new byte[] { 0, 1, 2, 3 };
        client.send(bytes1);

        NetUDP.Packet packet1 = waitForPacket(server);
        if (packet1 != null) {
            server.sendAll(bytes2);
        } else Assert.Fail("first packet did not arrive");

        NetUDP.Packet packet2 = waitForPacket(client);

        client.close();
        server.close();

        if (packet2 != null) {
            Assert.AreEqual(bytes2, packet2.data);
        } else Assert.Fail("second packet did not arrive");
    }

    [Test]
    public void BigPacketFragmentation() {
        NetUDP server = new NetUDP(49999);
        server.openServer("Omega");
        NetUDP client = new NetUDP(49999);
        client.openClient("Lul", "127.0.0.1");

        int size = 50000;
        var bytes = new byte[size];
        for (int i = 0; i < size; i++) bytes[i] = (byte)Random.Range(0, 255);

        client.send(bytes);
        
        NetUDP.Packet packet = waitForPacket(server);

        client.close();
        server.close();

        if (packet != null) {
            Assert.IsTrue(Enumerable.SequenceEqual(bytes, packet.data));
        } else Assert.Fail("packet did not arrive");
    }

    [Test]
    public void ForceCloseClient() {
        NetUDP client = new NetUDP(49999);
        client.openClient("Lul", "127.0.0.1");
        client.close();
    }

    [Test]
    public void SendCloseToServer() {
        NetUDP client = new NetUDP(49999);
        client.openClient("Lul", "127.0.0.1");
        var bytes2 = new byte[] { 0, 1, 2, 3 };
        client.send(bytes2, NetUDP.Protocol.kill);
        client.close();
    }

    [Test]
    public void ForceCloseServer() {
        NetUDP server = new NetUDP(49999);
        server.openServer("Omega");
        server.close();
    }

    [Test]
    public void ForceCloseBoth() {
        NetUDP client = new NetUDP(49999);
        client.openClient("Lul", "127.0.0.1");
        client.close();
        NetUDP server = new NetUDP(49999);
        server.openServer("Omega");
        server.close();
    }

    [Test]
    public void ForceCloseBothTwice() {
        NetUDP client = new NetUDP(49999);
        client.openClient("Lul", "127.0.0.1");
        client.close();
        NetUDP server = new NetUDP(49999);
        server.openServer("Omega");
        server.close();

        client.openClient("Lul", "127.0.0.1");
        client.close();
        server.openServer("Omega");
        server.close();
    }
}
