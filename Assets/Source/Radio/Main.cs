using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Main : MonoBehaviour {

    Net net;

    GameObject buttonHost;
    GameObject buttonJoin;
    TMP_Text labelNet;

    void Start() {
        net = GameObject.Find("Net").GetComponent<Net>();

        labelNet = GameObject.Find("LabelNet").GetComponent<TMP_Text>();
        labelNet.gameObject.SetActive(false);
        buttonHost = GameObject.Find("ButtonHost");
        buttonJoin = GameObject.Find("ButtonJoin");
    }

    public void OnHost() {
        buttonHost.SetActive(false);
        buttonJoin.SetActive(false);
        labelNet.gameObject.SetActive(true);
        labelNet.text = "Game Master";

        net.server = true;
        net.Open();
    }

    public void OnJoin() {
        buttonHost.SetActive(false);
        buttonJoin.SetActive(false);
        labelNet.gameObject.SetActive(true);
        labelNet.text = "Player";

        net.server = false;
        net.ip = "127.0.0.1";
        net.port = 60000;
        net.Open();
    }
}
