using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class GameStateController : MonoBehaviour {

    public static float SyncPerSecond = 10;

    public bool master = false;
    public bool started = false;
    public bool isover = false;
    public HashSet<string> readyPlayers = new HashSet<string>();

    public GameObject taskAreaVisualizer;
    public GameObject playerPrefab;

    PlayerLink localPlayerLink;
    public List<PlayerLink> playerLinks = new List<PlayerLink>();
    public List<TaskAreaLink> taskLinks = new List<TaskAreaLink>();
    public List<BridgeLink> bridgeLinks = new List<BridgeLink>();

    public GameState gameState;
    public RoadGraph graph;

    public Permanent permanent;

    public GameObject endPanel;
    public float timerEnd;

    public RenderTexture texCameraPlayer;

    public Dictionary<int, (Vector3, Vector3)> carsLast = new Dictionary<int, (Vector3, Vector3)>();
    public TrafficState traffic;
	public float carLastTime;

    public int trafficStep = 0;
    public int trafficStepServer = 0;

    public int bonkNotification;

    public Items items;

    public Dictionary<string, float> lastSync = new Dictionary<string, float>();

	void Start() {
        permanent = Permanent.get();
        configureGamestate(permanent.config);

        items = JsonUtility.FromJson<Items>(Resources.Load<TextAsset>("items").text);

        traffic = new TrafficState(graph);
        traffic.rails = LoadRailGraph(permanent.config.mapname);
        if (master) {
            traffic.generateCars(permanent.config.carDensity);
            carLastTime = Time.time;
            foreach (var car in traffic.cars.Values) {
                carsLast[car.id] = (traffic.absPos(car),  traffic.absDir(car));
			}
            if (permanent.net != null && permanent.net.open) {
                StreamSerializer stream = new StreamSerializer();
                traffic.serializeCars(stream);
                permanent.net.sendAll(stream.getBytes(), NetUDP.Protocol.mastercars, 1);
			}
        }

        var dynNodes = FindObjectsOfType<RoadNode>();
        foreach (var n in dynNodes) {
            n.disable = true;
		}

        Sync();
        TrafficStep();

        if (!permanent.net.open) {
            // singleplayer testing
            traffic.generateCars(1);
            carLastTime = Time.time;
            foreach (var car in traffic.cars.Values) {
                carsLast[car.id] = (traffic.absPos(car),  traffic.absDir(car));
			}
            started = true;
            FindObjectOfType<PlayerMove>().still = false;
        } else {
            // multiplayer
            if (!master) SendVideo();
            if (!master) FindObjectOfType<PlayerMove>().still = true;
		}

        if (!endPanel) endPanel = GameObject.Find("EndPanel");
    }

    RailGraph LoadRailGraph (string mapname) {
        TextAsset textAsset = Resources.Load("Maps/Rails/"+mapname) as TextAsset;
        var rgu = JsonUtility.FromJson<RailGraphUnindexed>(textAsset.text);
        return new RailGraph(rgu);
    }

    RoadGraph LoadGraph (string mapname) {
        TextAsset textAsset = Resources.Load("Maps/Roads/"+mapname) as TextAsset;
        return JsonUtility.FromJson<RoadGraph>(textAsset.text);
    }

    void VisualizeTasks () {
        foreach (var link in taskLinks) Destroy(link.gameObject);
        foreach (Task task in gameState.taskList.tasks) {
            GameObject objStart = Instantiate(taskAreaVisualizer, task.start, Quaternion.identity);
            objStart.GetComponentInChildren<Renderer>().material.color = new Color(1f, 0f, 0f, 0.2f);
            var linkStart = objStart.GetComponent<TaskAreaLink>();
            linkStart.taskId = task.id;
            linkStart.order = 0;
            taskLinks.Add(linkStart);

            GameObject objDest = Instantiate(taskAreaVisualizer, task.destination, Quaternion.identity);
            objDest.GetComponentInChildren<Renderer>().material.color = new Color(0f, 1f, 0f, 0.2f);
            var linkDest = objDest.GetComponent<TaskAreaLink>();
            linkDest.taskId = task.id;
            linkDest.order = 1;
            taskLinks.Add(linkDest);
        }
    }

    void configureGamestate (LobbyConfiguration config) {
        timerEnd = Time.time + 5;
        isover = false;
        if (config.mapname == "__noname") {
             // singleplayer fix
            config.mapname = SceneManager.GetActiveScene().name;
            config.players.Add(new ConfigPlayer(permanent.localNameId, false));
        }
        graph = LoadGraph(config.mapname);
        gameState = new GameState();
        gameState.timeLeft = config.gameTime;

        if (master) {
            gameState.generateTasks(graph, config.taskNumber, items);
        }

        VisualizeTasks();

        //var playerName = PlayerPrefs.GetString("PlayerName");

        var node0Pos = graph.nodes[0].pos + Vector3.up;

		var playerName = permanent.localNameId;
        if (!master) AddLocalPlayer(playerName, node0Pos);

        foreach (var player in config.players) {
            if (player.nameId != playerName && !player.master) {
                AddRemotePlayer(player.nameId, node0Pos);
            }
        }

        var bridges = FindObjectsOfType<BridgeLink>();
        foreach (var bridge in bridges) bridgeLinks.Add(bridge);

        foreach (var obst in graph.obstacles) {
            gameState.timerList.addTimer(obst.name, 
                new ObstacleTimer(obst.time, obst.min, obst.max));
        }

        if (master) {
            var rm = FindObjectOfType<RoadGraphMaker>();
            rm.visualizeGraph(graph);
            
            foreach (RoadGraphObstacle obst in graph.obstacles) {
                var obj = Instantiate(rm.visualizerBridgePrefab) as GameObject;
                obj.transform.SetParent(rm.visualizerHolder.transform);
                obj.transform.position = obst.pos;
                obj.transform.rotation = obst.rot;
                var bl = obj.AddComponent<BridgeLinkMaster>();
                bl.nameId = obst.name;
                bl.setScale(obst.scale);
            }
        }

        RequireRefresh();
    }

    PlayerLink instantiatePlayer (string name, Vector3 pos) {
        GameObject obj = Instantiate(playerPrefab, pos, Quaternion.identity);
        var link = obj.GetComponent<PlayerLink>();
        link.nameId = name;
        playerLinks.Add(link);

        PlayerRepr player = new PlayerRepr();
        player.pos = pos;
        gameState.playerList.addPlayer(name, player);

        /*
            obj.transform.Find("Main Camera").GetComponent<PostProcessLayer>().enabled = false;
		*/
        return link;
    }

    public void AddRemotePlayer(string name, Vector3 pos) {
        var rem = instantiatePlayer(name, pos);
		if (!master) {
			Destroy(rem.GetComponent<PlayerMove>());
			Destroy(rem.transform.Find("Main Camera").gameObject);
			Destroy(rem.transform.Find("CameraToTexture").gameObject);
		} 
    }

    public void AddLocalPlayer (string name, Vector3 pos) {
        localPlayerLink = instantiatePlayer(name, pos);
    }

    public PlayerLink getLocalPlayer () {
        return localPlayerLink;
    }

    // from https://gist.github.com/Santarh/899ed0914cf7c4517bdb36233be66c19
    private byte[] SaveRenderTextureAsPng(RenderTexture rt) {
        if (rt == null || !rt.IsCreated()) return null;

        // Allocate
        var sRgbRenderTex = RenderTexture.GetTemporary(rt.width, rt.height, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB);
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, mipChain: false, linear: false);

        // Linear to Gamma Conversion
        Graphics.Blit(rt, sRgbRenderTex);

        // Copy memory from RenderTexture
        var tmp = RenderTexture.active;
        RenderTexture.active = sRgbRenderTex;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = tmp;

        // Get PNG bytes
        var bytes = tex.EncodeToPNG();

        // Destroy
        Destroy(tex);
        RenderTexture.ReleaseTemporary(sRgbRenderTex);

        return bytes;
    }


    void SendVideo() {
        if (permanent.config.video == 0) return;
        if (!master && started) {
            var stream = new StreamSerializer();
            var img = SaveRenderTextureAsPng(texCameraPlayer);
            if (img != null) {
                stream.append(img);
                permanent.net.send(stream.getBytes(), NetUDP.Protocol.videoframe);
            }
        }
        Invoke(nameof(SendVideo), 0.1f);
    }

    void processPackets () {
        while (true) {
            var packet = permanent.net.pop();
            if (packet != null) {
                var stream = new StreamSerializer(packet.data);
                var protocol = packet.protocol;
                if (protocol == NetUDP.Protocol.radio) {
                    permanent.getRadio().addAudio(packet.id, stream.getNextBytes());
                } else {
                    if (master) {
                        if (protocol == NetUDP.Protocol.ready) {
                            readyPlayers.Add(packet.id);
                            // Assuming there is only one master
                            if (readyPlayers.Count == permanent.config.players.Count - 1) {
                                var sendstream = new StreamSerializer();
                                permanent.net.sendAll(sendstream.getBytes(), NetUDP.Protocol.startgame);
                                started = true;
                            }
                        }
                        if (protocol == NetUDP.Protocol.clientstate) {
                            var pos = stream.getNextVector3();
                            var rot = stream.getNextQuaternion();
                            var v = stream.getNextFloat();
                            var bonked = stream.getNextBool();
                            gameState.refreshPlayerPosition(packet.id, pos);
                            if (bonked) gameState.playerBonk(packet.id);

                            gameState.playerList.getPlayer(packet.id).rot = rot;
                            gameState.playerList.getPlayer(packet.id).vel = v;
                            lastSync[packet.id] = Time.time;
                        }
                        if (protocol == NetUDP.Protocol.videoframe) {
                            Texture2D textVideo = new Texture2D(256, 128);
                            textVideo.LoadImage(stream.getNextBytes());
                            foreach (var player in playerLinks) {
                                if (player.nameId == packet.id) {
                                    var linkMaster = player.GetComponent<PlayerLinkMaster>();
                                    if (linkMaster.viewportTex) {
                                        linkMaster.viewportTex.texture = textVideo;
                                    }
                                }
                            }
                        }
                    } else {
                        if (protocol == NetUDP.Protocol.masterstate) {
                            trafficStepServer = stream.getNextInt();
                            gameState.deserialize(stream.getNextBytes());
                            if (taskLinks.Count == 0) VisualizeTasks();

                            foreach (var player in gameState.playerList.players) {
                                lastSync[player.Key] = Time.time;
                            }
                        }
                        if (protocol == NetUDP.Protocol.mastercars) {
                            traffic.deserializeCars(stream);
                            carLastTime = Time.time;
                            foreach (var car in traffic.cars.Values) {
                                carsLast[car.id] = (traffic.absPos(car),  traffic.absDir(car));
			                }
                        }
                        if (protocol == NetUDP.Protocol.startgame) {
                            started = true;
                            if (!master) FindObjectOfType<PlayerMove>().still = false;
                        }
                        if (protocol == NetUDP.Protocol.over) {
                            SceneManager.LoadScene("Lobby");
                        }
                    }
                }
            } else break;
        }
	}

    void Sync () {
        if (permanent.net != null && permanent.net.open) {
            processPackets();

            if (master) {
                if (started) {
                    bool ended = gameState.isWon() || gameState.isLost();
                    if (ended && isover) {
                        var stream = new StreamSerializer();
                        permanent.net.sendAll(stream.getBytes(), NetUDP.Protocol.over, 1);
                        SceneManager.LoadScene("Lobby");
                    }
                    else {
                        gameState.passTime(1 / SyncPerSecond);
                        var stream = new StreamSerializer();
                        stream.append(trafficStep);
                        stream.append(gameState.serialize());
                        permanent.net.sendAll(stream.getBytes(), NetUDP.Protocol.masterstate);
                    }
                }
            } else {
                if (started) {
                    var stream = new StreamSerializer();
                    var pm = getLocalPlayer().GetComponent<PlayerMove>();
                    stream.append(pm.transform.position);
                    stream.append(pm.transform.rotation);
                    stream.append(pm.v);
                    if (pm.bonked) {
                        pm.bonked = false;
                        stream.append(true);
                        bonkNotification = 1;
					} else stream.append(false);
                    permanent.net.send(stream.getBytes(), NetUDP.Protocol.clientstate);
                } else {
                    if (traffic.cars.Count > 0) {
                        var stream = new StreamSerializer();
                        permanent.net.send(stream.getBytes(), NetUDP.Protocol.ready);
                    }
                }
            }
        }

        Invoke(nameof(Sync), 1 / SyncPerSecond);
    }

    void TrafficStep () {
		var watch = System.Diagnostics.Stopwatch.StartNew();

        if (master) trafficStepServer++;

        if (started) {
            carLastTime = Time.time;
            foreach (var car in traffic.cars.Values) {
                carsLast[car.id] = (traffic.absPos(car), traffic.absDir(car));
            }
            for (int i = 0; i < 4; i++) { 
                if (trafficStep >= trafficStepServer) break;
                traffic.step(1 / SyncPerSecond);
                trafficStep++;
            }
        }

		watch.Stop();
        float timeUsed = watch.ElapsedMilliseconds * 0.001f;
        Invoke(nameof(TrafficStep), Mathf.Max(0.03f, 1 / SyncPerSecond - timeUsed));
	}

    void Update() {
        RequireRefresh();

        bool ended = gameState.isWon() || gameState.isLost();
        if (!ended) {
            timerEnd = Time.time + 5;
        } else if (timerEnd < Time.time) {
            isover = true;
        }

        if (!master) {
            var player = getLocalPlayer();
            var move = player.GetComponent<PlayerMove>();
            var playerRepr = gameState.playerList.getPlayer(player.nameId);
            if (playerRepr.acceptedTaskId == -1) {
                move.weight = 1;
            } else {
                var task = gameState.taskList.fromId(playerRepr.acceptedTaskId);
                if (task != null) {
                    var item = items.items.Find(x => x.id == task.itemId);
                    move.weight = item.weight;
                }
            }
		}
    }

    void RequireRefresh() {
        SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
    }
}
