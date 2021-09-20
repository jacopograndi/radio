using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StreamSerializer {

    int size = 0;
    int iter = 0;

    byte[] buffer;

    public StreamSerializer() { buffer = new byte[128]; }

    public StreamSerializer(byte[] bytes) {
        size = bytes.Length;
        buffer = new byte[size];
        Buffer.BlockCopy(bytes, 0, buffer, 0, size);
    }

    public int getByteSize() {
        return size;
    }

    public byte[] getBytes() {
        byte[] bytes = new byte[size];
        Buffer.BlockCopy(buffer, 0, bytes, 0, size);
        return bytes;
    }

    void checkBufferSize (int count) {
        if (size+count >= buffer.Length) {
            var backup = new byte[buffer.Length];
            Buffer.BlockCopy(buffer, 0, backup, 0, size);
            buffer = new byte[buffer.Length * 2];
            Buffer.BlockCopy(backup, 0, buffer, 0, size);
        }
    }

    public byte[] getNextBytes() {
        byte[] bytes = new byte[size - iter];
        Buffer.BlockCopy(buffer, iter, bytes, 0, size - iter);
        return bytes;
    }

    byte[] getNextBytes (int l) {
        byte[] bytes = new byte[l];
        Buffer.BlockCopy(buffer, iter, bytes, 0, l);
        return bytes;
    }

    public void append(byte[] bs) {
        checkBufferSize(bs.Length);
        Buffer.BlockCopy(bs, 0, buffer, size, bs.Length);
        size += bs.Length;
    }

    public void append(bool v) {
        append(BitConverter.GetBytes(v));
    }

    public void append (int v) {
        append(BitConverter.GetBytes(v));
    }

    public void append (float v) {
        append(BitConverter.GetBytes(v));
    }

    public void append (string s) {
        append(s.Length);
        append(Encoding.ASCII.GetBytes(s));
    }

    public void append(Vector3 vec) {
        append(vec.x);
        append(vec.y);
        append(vec.z);
    }

    public void append(Quaternion q) {
        append(q.x);
        append(q.y);
        append(q.z);
        append(q.w);
    }

    void checkOverflow (int next) {
        if (iter+next > size) {
            throw new OverflowException();
        }
    }

    public int getNextInt() {
        int sizeNext = sizeof(int);
        checkOverflow(sizeNext);
        var v = BitConverter.ToInt32(buffer, iter);
        iter += sizeNext;
        return v;
    }

    public float getNextFloat() {
        int sizeNext = sizeof(float);
        checkOverflow(sizeNext);
        var v = BitConverter.ToSingle(buffer, iter);
        iter += sizeNext;
        return v;
    }

    public string getNextString() {
        int length = getNextInt();
        checkOverflow(length);
        var v = Encoding.ASCII.GetString(getNextBytes(length));
        iter += length;
        return v;
    }

    public bool getNextBool() {
        int length = sizeof(bool);
        checkOverflow(length);
        var v = BitConverter.ToBoolean(buffer, iter);
        iter += length;
        return v;
    }

    public Vector3 getNextVector3() {
        float x = getNextFloat();
        float y = getNextFloat();
        float z = getNextFloat();
        return new Vector3(x, y, z);
    }

    public Quaternion getNextQuaternion() {
        float x = getNextFloat();
        float y = getNextFloat();
        float z = getNextFloat();
        float w = getNextFloat();
        return new Quaternion(x, y, z, w);
    }

    public class OverflowException : Exception { };
}
