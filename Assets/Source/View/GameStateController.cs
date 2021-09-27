using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateController : MonoBehaviour {

    public static float SyncPerSecond = 20;

    public bool master = false;
    public bool started = false;
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

    public enum Protocol {
        masterstate=1000, start, ready, clientstate
    }


    void Start() {
        permanent = Permanent.get();
        InitGamestate(permanent.config);

        Sync();
    }

    RoadGraph LoadGraph (string mapname) {
        TextAsset textAsset = Resources.Load("Maps/"+mapname) as TextAsset;
        return JsonUtility.FromJson<RoadGraph>(textAsset.text);
    }

    void VisualizeTasks () {
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
        graph = LoadGraph(config.mapname);
        gameState = new GameState();
        gameState.timeLeft = config.gameTime;
        gameState.generateTasks(graph, config.taskNumber);

        VisualizeTasks();

        var playerName = PlayerPrefs.GetString("PlayerName");
        if (!master) AddLocalPlayer(playerName, new Vector3(-10f, 0, 0));

        foreach (var player in config.players) {
            if (player.nameId != playerName && !player.master) {
                AddRemotePlayer(player.nameId, new Vector3(-12f, 0, 0));
            }
        }

        /* make bridges part of the model
        var bridges = FindObjectsOfType<BridgeLink>();
        foreach (var bridge in bridges) {
            bridgeLinks.Add(bridge);
            gameState.timerList.addTimer(bridge.nameId,
                new ObstacleTimer(bridge.timer, bridge.timerMin, bridge.timerMax));
        }*/

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
                } else {
                    if (protocol == Protocol.masterstate) {
                        gameState.deserialize(stream.getNextBytes());
                    }
                    if (protocol == Protocol.start) {
                        started = true;
                    }
                }
            } else break;
        }

        if (master) {
            if (started) {
                gameState.passTime(Time.deltaTime);
                var stream = new StreamSerializer();
                stream.append((int)Protocol.masterstate);
                stream.append(gameState.serialize());
                permanent.net.sendAll(stream.getBytes());
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
    }

    void RequireRefresh() {
        SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
    }
}
