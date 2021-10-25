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

    public PanelState state = PanelState.joinhost;

    public GameObject playerLobbyPrefab;

    public GameObject panelJoinHost;
    public GameObject panelJoin;
    public GameObject panelLobby;

    public GameObject playerList;
    public GameObject configList;

    public TMP_InputField nameField;
    public TMP_InputField ipField;
    TMP_InputField taskNumField;
    TMP_InputField timeLeftField;
    TMP_Dropdown mapDropdown;

    public GameObject configField;
    public GameObject configDropdown;

    public TMP_Text joinLabel;

    public Permanent permanent;

    float connectionTimeout = 0;

    public bool configRefresh = false;

    LobbyConfiguration defaultLobbyConfiguration() { 
        var config = new LobbyConfiguration();
        var names = new List<string>();
        foreach (TextAsset textAsset in Resources.LoadAll("Maps")) {
            names.Add(textAsset.name);
        }
        config.mapname = names[0];
        return config;
    }

    TMP_Dropdown buildConfigDropDown (string label, List<string> options, Action<object> setter) {
        GameObject obj = Instantiate(configDropdown);
        obj.transform.SetParent(configList.transform);
        obj.transform.Find("Label").GetComponent<TMP_Text>().text = label;

        var dropdown = obj.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(x =>
            { 
                setter(dropdown.options[dropdown.value].text);
                configRefresh = true; 
            }
        );
        return dropdown;
	}

    TMP_InputField buildConfigField (string label, TMP_InputField.ContentType content, Action<object> setter) {
        GameObject obj = Instantiate(configField);
        obj.transform.SetParent(configList.transform);
        obj.transform.Find("Label").GetComponent<TMP_Text>().text = label;

        var field = obj.transform.Find("Field").GetComponent<TMP_InputField>();
        field.contentType = content;
        field.onValueChanged.AddListener(x => {
            if (content == TMP_InputField.ContentType.IntegerNumber) {
                int res = 0;
                if (int.TryParse(field.text, out res)) {
                    setter(res);
                }
            }
            if (content == TMP_InputField.ContentType.DecimalNumber) {
                float res = 0;
                if (float.TryParse(field.text, out res)) {
                    setter(res);
                }
            }
            configRefresh = true;
        }
        );
        return field;
	}

    void buildConfig (LobbyConfiguration config) {
        foreach (Transform child in configList.transform) Destroy(child.gameObject);
        
        var names = new List<string>();
        foreach (TextAsset textAsset in Resources.LoadAll("Maps")) {
            names.Add(textAsset.name);
        }
        
        {
            var obj = buildConfigDropDown("Map name",
                names, x => config.mapname = x.ToString());
            obj.value = obj.options.FindIndex(x => x.text == config.mapname);
        }

        {
            var obj = buildConfigField("Time limit",
                TMP_InputField.ContentType.DecimalNumber,
                x => config.gameTime = (float)x);
            obj.text = config.gameTime.ToString();
		}
        {
            var obj = buildConfigField("Task number",
                TMP_InputField.ContentType.IntegerNumber,
                x => config.taskNumber = (int)x);
            obj.text = config.taskNumber.ToString();
        }
	}

    void Start() {
        permanent = Permanent.get();
        if (permanent.net.open) {
            while (permanent.net.pop() != null);
            state = PanelState.lobby;
            buildConfig(permanent.config);
        }

        RefreshPanels();
        RefreshPlayerList();

        playerName = PlayerPrefs.GetString("PlayerName");
        ipString = PlayerPrefs.GetString("JoinIp");
        nameField.text = playerName;
        ipField.text = ipString;

        permanent = Permanent.get();

        Invoke(nameof(forceConfigRefresh), 0.5f);
    }

    void forceConfigRefresh () { 
        configRefresh = true; 
        Invoke(nameof(forceConfigRefresh), 0.5f);
    }

    void consumePackets () {
        while (true) {
            var packet = permanent.net.pop();
            if (packet != null) {
                var stream = new StreamSerializer(packet.data);
                var protocol = packet.protocol;
                if (protocol == NetUDP.Protocol.radio) {
                    permanent.getRadio().addAudio(packet.id, stream.getNextBytes());
                } else {
                    if (state == PanelState.join) {
                        if (protocol == NetUDP.Protocol.syncconf) {
                            state = PanelState.lobby;
                            permanent.config = new LobbyConfiguration();
                            permanent.config.deserialize(stream.getNextBytes());
                            RefreshPlayerList();
                            RefreshPanels();
                            buildConfig(permanent.config);
                            permanent.getRadio().startStream();
                        }
                    } else if (state == PanelState.lobby) {
                        if (permanent.net.server) {
                            if (protocol == NetUDP.Protocol.joinreq) {
                                if (permanent.config.players.Find(x => x.nameId == packet.id) == null) 
                                    permanent.config.players.Add(new ConfigPlayer(packet.id, false));
                                var streamSend = new StreamSerializer();
                                streamSend.append(permanent.config.serialize());
                                permanent.net.sendAll(streamSend.getBytes(), NetUDP.Protocol.syncconf);
                            }
                        } else {
                            if (protocol == NetUDP.Protocol.syncconf) {
                                permanent.config.deserialize(stream.getNextBytes());
                                buildConfig(permanent.config);
                            }
                            if (protocol == NetUDP.Protocol.start) {
                                StartGame();
                            }
                        }
                        RefreshPlayerList();
					}
                }
            } else break;
        }
    }

    void Update() {
        if (state == PanelState.join) {
            if (connectionTimeout < Time.time) {
                permanent.net.close();
                state = PanelState.joinhost;
                RefreshPanels();
            } else {
                consumePackets();
            }
        }
        if (state == PanelState.lobby) {
            consumePackets();

            if (configRefresh && permanent.net.server) {
                configRefresh = false;
                var streamSend = new StreamSerializer();
                streamSend.append(permanent.config.serialize());
                permanent.net.sendAll(streamSend.getBytes(), NetUDP.Protocol.syncconf);
            }
        }
    }


    public void JoinGame () {
        permanent.net.openClient(playerName, ipString);
        var stream = new StreamSerializer();
        stream.append("no data");
        permanent.net.send(stream.getBytes(), NetUDP.Protocol.joinreq);
        connectionTimeout = Time.time + 3f;
    }

    public void StartGame() {
        permanent.getRadio().startStream();
		permanent.localNameId = playerName;
        if (permanent.net.server) {
            var stream = new StreamSerializer();
            permanent.net.sendAll(stream.getBytes(), NetUDP.Protocol.start, 1);
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
        permanent.getRadio().startStream();

        permanent.config = defaultLobbyConfiguration();
        //linkConfigToUI(permanent.config);
        permanent.config.players.Add(new ConfigPlayer(playerName, true));
        RefreshPanels();
        RefreshPlayerList();
        buildConfig(permanent.config);

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
