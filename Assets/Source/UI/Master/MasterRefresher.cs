using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterRefresher : MonoBehaviour {

    GameObject canvas;

    void Start() {
        if (!canvas) canvas = GameObject.Find("Canvas");
    }

    public void Refresh() {
        canvas.BroadcastMessage("Refresh");
    }
}
