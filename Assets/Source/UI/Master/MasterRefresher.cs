using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterRefresher : MonoBehaviour {

    GameObject canvas;

    public void Refresh() {
        if (!canvas) canvas = GameObject.Find("Canvas");
        canvas.BroadcastMessage("Refresh");
    }
}
