using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateController : MonoBehaviour {

    public bool master = false;

    public GameObject taskAreaVisualizer;
    public GameObject playerPrefab;

    PlayerLink localPlayerLink;
    public List<PlayerLink> playerLinks = new List<PlayerLink>();
    public List<TaskAreaLink> taskLinks = new List<TaskAreaLink>();
    public List<BridgeLink> bridgeLinks = new List<BridgeLink>();

    public GameState gameState;
    public RoadGraph graph;

    public Permanent permanent;


    void Start() {
        permanent = Permanent.get();
        InitGamestate(permanent.config);
    }

    RoadGraph LoadGraph () {
        TextAsset textAsset = Resources.Load("Generated/RoadGraph") as TextAsset;
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
        graph = LoadGraph();
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
        var packet = permanent.net.pop();
        if (packet != null) {
            if (master) {
                var stream = new StreamSerializer(packet.data);
                var pos = stream.getNextVector3();
                gameState.refreshPlayerPosition(packet.id, pos);
            } else {
                gameState.deserialize(packet.data);
            }
        }

        if (master) {
            gameState.passTime(Time.deltaTime);
            permanent.net.sendAll(gameState.serialize());
        } else {
            var stream = new StreamSerializer();
            stream.append(localPlayerLink.transform.position);
            permanent.net.send(stream.getBytes());
        }
    }

    void Update() {
        Sync();
        RequireRefresh();
    }

    void RequireRefresh() {
        SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
    }
}
