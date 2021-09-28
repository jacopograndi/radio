using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateController : MonoBehaviour {

    public static float SyncPerSecond = 20;

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

    public enum Protocol {
        masterstate=1000, start, ready, clientstate, over, videoframe
    }


    void Start() {
        permanent = Permanent.get();
        InitGamestate(permanent.config);

        Sync();
        if (!master) SendVideo();
    }

    RoadGraph LoadGraph (string mapname) {
        TextAsset textAsset = Resources.Load("Maps/"+mapname) as TextAsset;
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

    void InitGamestate (LobbyConfiguration config) {
        timerEnd = Time.time + 5;
        isover = false;
        graph = LoadGraph(config.mapname);
        gameState = new GameState();
        gameState.timeLeft = config.gameTime;
        if (master) gameState.generateTasks(graph, config.taskNumber);

        VisualizeTasks();

        var playerName = PlayerPrefs.GetString("PlayerName");
        if (!master) AddLocalPlayer(playerName, new Vector3(-10f, 0, 0));

        foreach (var player in config.players) {
            if (player.nameId != playerName && !player.master) {
                AddRemotePlayer(player.nameId, new Vector3(-12f, 0, 0));
            }
        }

        var bridges = FindObjectsOfType<BridgeLink>();
        foreach (var bridge in bridges) bridgeLinks.Add(bridge);

        foreach (var obst in graph.obstacles) {
            gameState.timerList.addTimer(obst.name, 
                new ObstacleTimer(obst.time, obst.min, obst.max));
        }

        if (master) {
            FindObjectOfType<RoadGraphMaker>().visualizeGraph(graph);
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
        return link;
    }

    public void AddRemotePlayer(string name, Vector3 pos) {
        instantiatePlayer(name, pos);
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
        if (!master && started) {
            var stream = new StreamSerializer();
            stream.append((int)Protocol.videoframe);
            stream.append(SaveRenderTextureAsPng(texCameraPlayer));
            permanent.net.send(stream.getBytes());
        }
        Invoke(nameof(SendVideo), 1);
    }

    void Sync () {
        while (true) {
            var packet = permanent.net.pop();
            if (packet != null) {
                var stream = new StreamSerializer(packet.data);
                Protocol protocol = (Protocol)stream.getNextInt();
                if (master) {
                    if (protocol == Protocol.ready) {
                        readyPlayers.Add(packet.id);
                        // Assuming there is only one master
                        if (readyPlayers.Count == permanent.config.players.Count-1) {
                            var sendstream = new StreamSerializer();
                            sendstream.append((int)Protocol.start);
                            permanent.net.sendAll(sendstream.getBytes());
                            started = true;
                        }
                    }
                    if (protocol == Protocol.clientstate) {
                        var pos = stream.getNextVector3();
                        gameState.refreshPlayerPosition(packet.id, pos);
                    }
                    if (protocol == Protocol.videoframe) {
                        print("video");
                        Texture2D textVideo = new Texture2D(256, 128);
                        textVideo.LoadImage(stream.getNextBytes());
                        foreach (var player in playerLinks) {
                            if (player.nameId == packet.id) {
                                var rend = player.transform.Find("VideoPlane").GetComponent<Renderer>();
                                rend.material.mainTexture = textVideo;
                            }
                        }
                    }
                } else {
                    if (protocol == Protocol.masterstate) {
                        gameState.deserialize(stream.getNextBytes());
                        if (taskLinks.Count == 0) VisualizeTasks();
                    }
                    if (protocol == Protocol.start) {
                        started = true;
                    }
                    if (protocol == Protocol.over) {
                        SceneManager.LoadScene("Lobby");
                    }
                }
            } else break;
        }

        if (master) {
            if (started) {
                bool ended = gameState.isWon() || gameState.isLost();
                if (ended && isover) {
                    var stream = new StreamSerializer();
                    stream.append((int)Protocol.over);
                    permanent.net.sendAll(stream.getBytes());
                    SceneManager.LoadScene("Lobby");
                }
                else {
                    gameState.passTime(1 / SyncPerSecond);
                    var stream = new StreamSerializer();
                    stream.append((int)Protocol.masterstate);
                    stream.append(gameState.serialize());
                    permanent.net.sendAll(stream.getBytes());
                }
            }
        } else {
            if (started) {
                var stream = new StreamSerializer();
                stream.append((int)Protocol.clientstate);
                stream.append(localPlayerLink.transform.position);
                permanent.net.send(stream.getBytes());
            } else {
                var stream = new StreamSerializer();
                stream.append((int)Protocol.ready);
                permanent.net.send(stream.getBytes());
            }
        }

        Invoke(nameof(Sync), 1/SyncPerSecond);
    }

    void Update() {
        RequireRefresh();
        RefreshPanels();

        bool ended = gameState.isWon() || gameState.isLost();
        if (!ended) {
            timerEnd = Time.time + 5;
        } else if (timerEnd < Time.time) {
            isover = true;
        }
    }

    void RefreshPanels () {
        if (endPanel == null) return;
        bool ended = gameState.isWon() || gameState.isLost();
        if (ended && !endPanel.activeSelf) {
            endPanel.SetActive(true);
            if (gameState.isLost()) {
                endPanel.GetComponentInChildren<TMP_Text>().text = "You lose";
            }
        } else if (!ended && endPanel.activeSelf) {
            endPanel.SetActive(false);
        }
    }

    void RequireRefresh() {
        SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
    }
}
