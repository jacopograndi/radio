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
        public Protocol protocol;
        public int full;
        public Packet(byte[] d, string id, Protocol protocol) {
            data = new byte[d.Length];
            d.CopyTo(data, 0);
            this.id = id;
            this.protocol = protocol;
            full = 0;
        }
    }

    public enum Protocol {
        normal, kill,
        joinreq = 100, syncconf, start, 
        masterstate=1000, startgame, ready, clientstate, over, videoframe,
        radio=10000
    }

    UdpClient sock;
    public List<Packet> recv = new List<Packet>();
    public Dictionary<int, Packet> incomplete = new Dictionary<int, Packet>();
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
            int packetSerial = stream.getNextInt();
            int offset = stream.getNextInt();
            int size = stream.getNextInt();
            byte[] data = stream.getNextBytes();

            if (protocol == Protocol.kill) {
                close();
                return;
            }
            if (clientMap.fromId(id) == null) {
                clientMap.addClient(id, source);
            }

            if (incomplete.ContainsKey(packetSerial)) {
                Buffer.BlockCopy(data, 0, incomplete[packetSerial].data, offset, data.Length);
                incomplete[packetSerial].full += data.Length;
            } else {
                byte[] all = new byte[size];
                Buffer.BlockCopy(data, 0, all, offset, data.Length);
                incomplete.Add(packetSerial, new Packet(all, id, protocol));
                incomplete[packetSerial].full += data.Length;
	        }

            if (incomplete[packetSerial].full == size) {
                lock (recv) recv.Add(incomplete[packetSerial]);
                incomplete.Remove(packetSerial);
			}

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

    int serial = 0;

    public void sendTo(byte[] msg, IPEndPoint ip, Protocol protocol = Protocol.normal) {
        if (msg.Length == 0) msg = new byte[1] { 127 }; // fix empty message fail
        int packetSize = 1024;
        int consumedBytes = 0;
        int packetSent = 0;
        while (consumedBytes < msg.Length) {
            int unsignedSize = Math.Min(msg.Length - consumedBytes, packetSize - nameId.Length - 16);
            byte[] msgw = new byte[unsignedSize];
            Buffer.BlockCopy(msg, consumedBytes, msgw, 0, unsignedSize);

            StreamSerializer stream = new StreamSerializer();
            stream.append(nameId);
            stream.append((int)protocol);
            stream.append((int)serial);
            stream.append((int)consumedBytes);
            stream.append((int)msg.Length);
            stream.append(msgw);
            byte[] signedMsg = stream.getBytes();

            sock.Send(signedMsg, signedMsg.Length, ip);
            consumedBytes += unsignedSize;
            packetSent++;
        }
        serial++;
    }
}
