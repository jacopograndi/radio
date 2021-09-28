using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetUDP {

    public class ClientEndpointMap {
        Dictionary<string, IPEndPoint> ips = new Dictionary<string, IPEndPoint>();

        public void addClient (string id, IPEndPoint ip) {
            ips[id] = ip;
        }

        public IPEndPoint fromId(string id) {
            IPEndPoint ip = null;
            ips.TryGetValue(id, out ip);
            return ip;
        }

        public Dictionary<string, IPEndPoint>.ValueCollection getAll() {
            return ips.Values;
        }
    }

    public class Packet {
        public byte[] data;
        public string id;
        public Packet(byte[] d, string id) {
            data = new byte[d.Length];
            d.CopyTo(data, 0);
            this.id = id;
        }
    }

    public enum Protocol {
        normal, kill
    }

    UdpClient sock;
    public List<Packet> recv = new List<Packet>();
    public ClientEndpointMap clientMap = new ClientEndpointMap();

    public string ip;
    public int port;
    public bool server = true;
    public bool open = false;
    public string nameId;

    public const int SIO_UDP_CONNRESET = -1744830452;

    public NetUDP(int port) {
        this.port = port;
    }

    void socketFixICMP () {
        sock.Client.IOControl(
            (IOControlCode)SIO_UDP_CONNRESET,
            new byte[] { 0, 0, 0, 0 }, null
        );
    }

    public void openServer(string nameId) {
        this.nameId = nameId;
        open = true;
        server = true;
        IPEndPoint addr = new IPEndPoint(IPAddress.Any, port);
        sock = new UdpClient(addr);
        socketFixICMP();
        sock.BeginReceive(new AsyncCallback(onUdpData), sock);
        Debug.Log ("server init");
    }

    public void openClient(string nameId, string ip) {
        this.nameId = nameId;
        this.ip = ip;
        open = true;
        server = false;
        sock = new UdpClient();
        socketFixICMP();
        sock.BeginReceive(new AsyncCallback(onUdpData), sock);

        Debug.Log("client init");
    }

    public void close () {
        if (!open) return;
        open = false;
        sock.Close();
    }

    public Packet pop () {
        lock (recv) {
            if (recv.Count > 0) {
                Packet packet = recv[0];
                recv.RemoveAt(0);
                return packet;
            }
        }
        return null;
    }

    void onUdpData(IAsyncResult result) {
        if (!open) {
            Debug.Log("closed " + (server ? "server" : "client"));
            return;
        }
        try {
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = socket.EndReceive(result, ref source);
            StreamSerializer stream = new StreamSerializer(message);
            string id = stream.getNextString();
            Protocol protocol = (Protocol)stream.getNextInt();
            if (protocol == Protocol.kill) {
                close();
                return;
            }
            if (clientMap.fromId(id) == null) {
                clientMap.addClient(id, source);
            }
            lock (recv) recv.Add(new Packet(stream.getNextBytes(), id));
            socket.BeginReceive(new AsyncCallback(onUdpData), socket);
        }
        catch (Exception e) {
            Debug.Log(e);
            close();
        }
    }

    public void sendAll(byte[] msg, Protocol protocol = Protocol.normal) {
        foreach (IPEndPoint ip in clientMap.getAll()) {
            sendTo(msg, ip, protocol);
        }
    }

    public void send(byte[] msg, Protocol protocol = Protocol.normal) {
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ip), port);
        sendTo(msg, target, protocol);
    }

    public void sendTo(byte[] msg, IPEndPoint ip, Protocol protocol = Protocol.normal) {
        int packetSize = 1024 * 8;
        int consumedBytes = 0;
        while (consumedBytes < msg.Length) {
            int unsignedSize = Math.Min(msg.Length - consumedBytes, packetSize - nameId.Length - 1);
            byte[] msgw = new byte[unsignedSize];
            Buffer.BlockCopy(msg, consumedBytes, msgw, 0, unsignedSize);

            StreamSerializer stream = new StreamSerializer();
            stream.append(nameId);
            stream.append((int)protocol);
            stream.append(msg);
            byte[] signedMsg = stream.getBytes();

            sock.Send(signedMsg, signedMsg.Length, ip);
            consumedBytes += unsignedSize;
        }
    }
}
