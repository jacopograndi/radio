using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

public class Net : MonoBehaviour {

    public class Packet {
        public byte[] data;
        public IPEndPoint ip;
        public Packet(byte[] d, IPEndPoint ip) {
            data = new byte[d.Length];
            Buffer.BlockCopy(d, 0, data, 0, d.Length);
            this.ip = ip;
        }
    }

    UdpClient sock;
    static public List<Packet> recv = new List<Packet>();
    static public HashSet<IPEndPoint> ips = new HashSet<IPEndPoint>();

    public string ip;
    public int port;
    public bool server = true;
    public bool open = false;

    public void Open() {
        if (server) OpenServer();
        else OpenClient();
        open = true;
    }

    void OpenServer() {
        IPEndPoint addr = new IPEndPoint(IPAddress.Any, port);
        sock = new UdpClient(addr);
        sock.BeginReceive(new AsyncCallback(OnUdpData), sock);
        print("server init");
    }

    void OpenClient() {
        sock = new UdpClient();
        sock.BeginReceive(new AsyncCallback(OnUdpData), sock);

        // send initial packet
        IPEndPoint target = new IPEndPoint(IPAddress.Parse(ip), port);
        string name12 = name.Length > 12 ? name.Substring(0, 12) : name;
        byte[] message = new byte[12];
        Buffer.BlockCopy(Encoding.ASCII.GetBytes(name12), 0, message, 0, name12.Length);
        SendTo(message, target);

        print("client init");
    }

    /*
    void Update() {
        if (sock != null && sock.Available > 0) {
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = sock.Receive(ref source);
            print("recv from " + source.ToString());
            if (!ips.Contains(source)) {
                ips.Add(source);
            } else {
                recv.Add(new Packet(message, source));
            }
        }
    }*/

    static void OnUdpData(IAsyncResult result) {
        try {
            UdpClient socket = result.AsyncState as UdpClient;
            IPEndPoint source = new IPEndPoint(0, 0);
            byte[] message = socket.EndReceive(result, ref source);
            if (!ips.Contains(source)) {
                ips.Add(source);
            } else {
                lock (recv) recv.Add(new Packet(message, source));
            }
            socket.BeginReceive(new AsyncCallback(OnUdpData), socket);
        } catch (Exception e) {
            print(e);
        }
    }

    public void SendTo(byte[] msg, IPEndPoint ip) {
        int size = 1024;
        int i = 0;
        while (i * size < msg.Length) {
            int clamp = Math.Min(msg.Length - i * size, size);
            byte[] msgw = new byte[clamp];
            Buffer.BlockCopy(msg, i * clamp, msgw, 0, clamp);
            sock.Send(msgw, msgw.Length, ip);
            i++;
        }
    }

    public void SendAll(byte[] msg) {
        foreach (IPEndPoint ip in ips) {
            SendTo(msg, ip);
        }
    }

    static byte[] ArraySlice(byte[] source, int start, int length) {
        byte[] destfoo = new byte[length];
        Array.Copy(source, start, destfoo, 0, length);
        return destfoo;
    }
}
