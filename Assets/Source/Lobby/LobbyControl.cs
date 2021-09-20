using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LobbyControl : MonoBehaviour {
   
    public string playerName;
    public string ipString;

    public enum PanelState {
        joinhost, lobby, join
    }

    public PanelState state = PanelState.joinhost;

    public GameObject playerLobbyPrefab;

    public GameObject panelJoinHost;
    public GameObject panelJoin;
    public GameObject panelLobby;

    public GameObject playerList;

    public TMP_InputField nameField;
    public TMP_InputField ipField;

    public TMP_Text joinLabel;

    public const int gamePort = 49999;
    public NetUDP net = new NetUDP(gamePort);

    public LobbyConfiguration config;

    float connectionTimeout = 0;

    void Start() {
        RefreshPanels();
        playerName = PlayerPrefs.GetString("PlayerName");
        ipString = PlayerPrefs.GetString("JoinIp");
        nameField.text = playerName;
        ipField.text = ipString;
    }

    void Update() {
        if (state == PanelState.join) {
            var packet = net.pop();
            if (packet != null) {
                state = PanelState.lobby;
                config = new LobbyConfiguration();
                config.deserialize(packet.data);
                RefreshPlayerList();
                RefreshPanels();
            }
            if (connectionTimeout < Time.time) {
                net.close();
                state = PanelState.joinhost;
                RefreshPanels();
            }
        }
        if (state == PanelState.lobby) {
            if (net.server) {
                var packet = net.pop();
                if (packet != null) {
                    config.players.Add(packet.id);
                    net.sendAll(config.serialize());
                    RefreshPlayerList();
                }
            } else {
                var packet = net.pop();
                if (packet != null) {
                    config.deserialize(packet.data);
                    RefreshPlayerList();
                }
            }
        }
    }

    public static bool validPlayerName (string name) {
        if (name.Length > 0) return true;
        return false;
    }

    // from https://stackoverflow.com/questions/5096780/ip-address-validation
    public static bool IsIPv4(string value) {
        var octets = value.Split('.');
        if (octets.Length != 4) return false;
        foreach (var octet in octets) {
            int q;
            if (!int.TryParse(octet, out q)
                || !q.ToString().Length.Equals(octet.Length)
                || q < 0
                || q > 255) { return false; }
        }
        return true;
    }

    public static bool validIp(string ip) {
        if (ip.Length > 0 && IsIPv4(ip)) return true;
        return false;
    }

    void RefreshPlayerList () {
        foreach (Transform child in playerList.transform) Destroy(child.gameObject);
        foreach (var id in config.players) {
            GameObject obj = Instantiate(playerLobbyPrefab, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(playerList.transform);
            obj.GetComponentInChildren<TMP_Text>().text = id;
        }
    }

    void RefreshPanels () {
        if (state == PanelState.joinhost) {
            panelJoinHost.SetActive(true);
            panelLobby.SetActive(false);
            panelJoin.SetActive(false);
        }
        if (state == PanelState.lobby) {
            panelJoinHost.SetActive(false);
            panelLobby.SetActive(true);
            panelJoin.SetActive(false);
        }
        if (state == PanelState.join) {
            panelJoinHost.SetActive(false);
            panelLobby.SetActive(false);
            panelJoin.SetActive(true);
        }
    }

    public void OnJoinClick() {
        if (state == PanelState.joinhost) {
            if (!validPlayerName(playerName)) return;
            state = PanelState.join;
            joinLabel.text = "Join";
            joinLabel.GetComponent<Button>().interactable = true;
            connectionTimeout = float.PositiveInfinity;
        } else if (state == PanelState.join) {
            if (!validIp(ipString)) return;
            net.openClient(playerName, ipString);
            net.send(new byte[] { 32 });
            joinLabel.text = "Contacting server...";
            joinLabel.GetComponent<Button>().interactable = false;
            connectionTimeout = Time.time + 10f;
        }
        RefreshPanels();
    }

    public void OnHostClick() {
        if (!validPlayerName(playerName)) return;
        state = PanelState.lobby;
        net.openServer(playerName);

        config = new LobbyConfiguration();
        config.players.Add(playerName);
        RefreshPanels();
        RefreshPlayerList();
    }

    public void OnBackClick() {
        if (state == PanelState.lobby) {
            state = PanelState.joinhost;
            net.close();
        } else if (state == PanelState.joinhost) {
            print("back out of scene");
        }
        RefreshPanels();
    }

    public void OnStartClick() {
        print("goto master/map scene");
    }

    public void OnEditName(string _) {
        string newname = nameField.text;
        playerName = newname;
        if (validPlayerName(newname)) {
            PlayerPrefs.SetString("PlayerName", newname);
        }
    }

    public void OnEditIp(string _) {
        string newip = ipField.text;
        ipString = newip;
        if (validIp(newip)) {
            PlayerPrefs.SetString("JoinIp", newip);
        }
    }

    void OnApplicationQuit() {
        net.close();
    }
}
