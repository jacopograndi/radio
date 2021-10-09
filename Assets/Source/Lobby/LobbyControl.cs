using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class LobbyControl : MonoBehaviour {
   
    public string playerName;
    public string ipString;

    public enum PanelState {
        joinhost, lobby, join
    }

    public enum Protocol {
        joinreq = 100, syncconf, start
    }

    public PanelState state = PanelState.joinhost;

    public GameObject playerLobbyPrefab;

    public GameObject panelJoinHost;
    public GameObject panelJoin;
    public GameObject panelLobby;

    public GameObject playerList;

    public TMP_InputField nameField;
    public TMP_InputField ipField;
    public TMP_InputField taskNumField;
    public TMP_InputField timeLeftField;
    public TMP_Dropdown mapDropdown;

    public TMP_Text joinLabel;

    public Permanent permanent;

    float connectionTimeout = 0;

    LobbyConfiguration defaultLobbyConfiguration() { 
        var config = new LobbyConfiguration();
        var names = new List<string>();
        foreach (TextAsset textAsset in Resources.LoadAll("Maps")) {
            names.Add(textAsset.name);
        }
        config.mapname = names[0];
        return config;
    }

    void linkConfigToUI(LobbyConfiguration config) {
        var names = new List<string>();
        mapDropdown.ClearOptions();
        foreach (TextAsset textAsset in Resources.LoadAll("Maps")) {
            names.Add(textAsset.name);
        }
        mapDropdown.AddOptions(names);
        mapDropdown.onValueChanged.AddListener(x =>
        { config.mapname = names[mapDropdown.value]; }
        );
        timeLeftField.onValueChanged.AddListener(x => {
            float res = 0;
            if (float.TryParse(timeLeftField.text, out res)) {
                config.gameTime = res;
            }
        }
        );
        taskNumField.onValueChanged.AddListener(x => {
            int res = 0;
            if (int.TryParse(taskNumField.text, out res)) {
                config.taskNumber = res;
            }
        }
        );
        timeLeftField.text = config.gameTime.ToString();
        taskNumField.text = config.taskNumber.ToString();
        mapDropdown.value = mapDropdown.options.FindIndex(x => x.text == config.mapname);
    }


    void Start() {
        permanent = Permanent.get();
        if (permanent.net.open) {
            while (permanent.net.pop() != null);
            state = PanelState.lobby;
            linkConfigToUI(permanent.config);
        }

        RefreshPanels();
        RefreshPlayerList();

        playerName = PlayerPrefs.GetString("PlayerName");
        ipString = PlayerPrefs.GetString("JoinIp");
        nameField.text = playerName;
        ipField.text = ipString;

        permanent = Permanent.get();
    }

    void Update() {
        if (state == PanelState.join) {
            if (connectionTimeout < Time.time) {
                permanent.net.close();
                state = PanelState.joinhost;
                RefreshPanels();
            } else {
                var packet = permanent.net.pop();
                if (packet != null) {
                    var stream = new StreamSerializer(packet.data);
                    Protocol protocol = (Protocol)stream.getNextInt();
                    if (protocol == Protocol.syncconf) {
                        state = PanelState.lobby;
                        permanent.config = new LobbyConfiguration();
                        permanent.config.deserialize(stream.getNextBytes());
                        RefreshPlayerList();
                        RefreshPanels();
                    }
                }
            }
        }
        if (state == PanelState.lobby) {
            while (true) {
                var packet = permanent.net.pop();
                if (packet != null) {
                    var stream = new StreamSerializer(packet.data);
                    Protocol protocol = (Protocol)stream.getNextInt();
                    if (permanent.net.server) {
                        if (protocol == Protocol.joinreq) {
                            permanent.config.players.Add(new ConfigPlayer(packet.id, false));
                            var streamSend = new StreamSerializer();
                            streamSend.append((int)Protocol.syncconf);
                            streamSend.append(permanent.config.serialize());
                            permanent.net.sendAll(streamSend.getBytes());
                        }
                    } else {
                        if (protocol == Protocol.syncconf) {
                            permanent.config.deserialize(stream.getNextBytes());
                            RefreshConfig();
                        }
                        if (protocol == Protocol.start) {
                            StartGame();
                        }
                    }
                    RefreshPlayerList();
                } else break;
            }

            if (permanent.net.server) {
                var streamSend = new StreamSerializer();
                streamSend.append((int)Protocol.syncconf);
                streamSend.append(permanent.config.serialize());
                permanent.net.sendAll(streamSend.getBytes());
            }
        }
    }

    public void JoinGame () {
        permanent.net.openClient(playerName, ipString);
        var stream = new StreamSerializer();
        stream.append((int)Protocol.joinreq);
        permanent.net.send(stream.getBytes());
        connectionTimeout = Time.time + 10f;
    }

    public void StartGame() {
		permanent.localNameId = playerName;
        if (permanent.net.server) {
            var stream = new StreamSerializer();
            stream.append((int)Protocol.start);
            permanent.net.sendAll(stream.getBytes());
            SceneManager.LoadScene("Master");
        } else {
            SceneManager.LoadScene(permanent.config.mapname);
        }
    }

    void RefreshPlayerList () {
        foreach (Transform child in playerList.transform) Destroy(child.gameObject);
        foreach (var player in permanent.config.players) {
            GameObject obj = Instantiate(playerLobbyPrefab, Vector3.zero, Quaternion.identity);
            obj.transform.SetParent(playerList.transform);
            obj.GetComponentInChildren<TMP_Text>().text = player.nameId;
        }
    }

    void RefreshConfig () {
        var names = new List<string>();
        names.Add(permanent.config.mapname);
        mapDropdown.ClearOptions();
        mapDropdown.AddOptions(names);

        timeLeftField.text = permanent.config.gameTime.ToString();
        taskNumField.text = permanent.config.taskNumber.ToString();
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
            joinLabel.text = "Contacting server...";
            joinLabel.GetComponent<Button>().interactable = false;
            JoinGame();
        }
        RefreshPanels();
    }

    public void OnHostClick() {
        if (!validPlayerName(playerName)) return;
        state = PanelState.lobby;
        permanent.net.openServer(playerName);

        permanent.config = defaultLobbyConfiguration();
        linkConfigToUI(permanent.config);
        permanent.config.players.Add(new ConfigPlayer(playerName, true));
        RefreshPanels();
        RefreshPlayerList();
    }

    public void OnBackClick() {
        if (state == PanelState.lobby) {
            state = PanelState.joinhost;
            permanent.net.close();
        } else if (state == PanelState.joinhost) {
            print("back out of scene");
        }
        RefreshPanels();
    }

    public void OnStartClick() {
        if (permanent.net.server) {
            StartGame();
        }
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

    public static bool validPlayerName(string name) {
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
}
