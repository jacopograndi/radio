using System;
using UnityEngine;

public class AudioStream {

    public float[] buffer = new float[4096];
    public int posWrite = 0;
    public int posRead = 0;
    public int delay = 0;

    public AudioStream (int size) { 
        buffer = new float[size];
        for (int i = 0; i < size; i++) buffer[i] = 0;
    }

    int localPos (int pos) {
        int p = pos;
        while (p >= buffer.Length) p -= buffer.Length;
        while (p < 0) p += buffer.Length;
        return p; 
    }

    public void write (float[] oth) {
        int left = oth.Length;
        while (left > 0) {
            int space = buffer.Length - localPos(posWrite);
            int clampSize = Mathf.Min(space, left);
            if (space > 0) {
                Buffer.BlockCopy(
                    oth, (oth.Length - left) * sizeof(float),
                    buffer, localPos(posWrite) * sizeof(float), 
                    clampSize * sizeof(float));
                posWrite += clampSize;
                left -= clampSize;
            }
        }
    }

    public bool checkAvailable (int amt) {
        return unread() >= amt;
	}

    void advance (int amt) {/*   
        for (int i = 0; i < amt; i++) {
            if (posRead - delay > 0) {
                buffer[localPos(posRead + i)] = 0;
            }
        }*/
        posRead += amt;
    }

    public float[] read (int amt) {
        //Debug.Log(this.GetHashCode() + " " + unread());
        float[] res = new float[amt];
        if (checkAvailable(amt)) {
            //Debug.Log(this.GetHashCode() + " " + unread());
            res = peek(amt);
            advance(amt);
		} else {
            for (int i = 0; i < amt; i++) res[i] = 0;
		}
        return res;
    }

    float[] peek (int amt) {
        int head = posRead - delay;
        float[] res = new float[amt];
        for (int i = 0; i < res.Length; i++) {
            if (head < posWrite && head >= 0) {
                res[i] = buffer[localPos(head)];
                head++;
            } else res[i] = 0;
        }
        return res;
    }

    public int unread () {
        return posWrite - posRead + delay;
    } 
}