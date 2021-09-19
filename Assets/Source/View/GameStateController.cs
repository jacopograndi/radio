using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateController : MonoBehaviour {

    public GameObject taskAreaVisualizer;
    public GameObject playerPrefab;

    PlayerLink localPlayerLink;
    public List<PlayerLink> playerLinks = new List<PlayerLink>();
    public List<TaskAreaLink> taskLinks = new List<TaskAreaLink>();

    public GameState gameState;
    public RoadGraph graph;

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

    void InitGamestate () {
        graph = LoadGraph();
        gameState = new GameState();
        gameState.timeLeft = 5 * 60;
        gameState.generateTasks(graph, 3);

        VisualizeTasks();
        AddLocalPlayer("Player1", new Vector3(-10f, 0, 0));

        RequireRefresh();
    }

    void Start() {
        InitGamestate();
    }

    public void AddLocalPlayer (string name, Vector3 pos) {
        GameObject obj = Instantiate(playerPrefab, pos, Quaternion.identity);
        var link = obj.GetComponent<PlayerLink>();
        link.nameId = name;
        playerLinks.Add(link);
        localPlayerLink = link;

        PlayerRepr player = new PlayerRepr();
        player.pos = pos;
        gameState.addPlayer(name, player);
    }

    public PlayerLink getLocalPlayer () {
        return localPlayerLink;
    }

    void Gather() { 
        foreach (var link in playerLinks) {
            gameState.refreshPlayerPosition(link.nameId, link.transform.position);
        }
    }

    void Update() {
        Gather();

        gameState.timeLeft -= Time.deltaTime;
        RequireRefresh();
    }

    void RequireRefresh() {
        SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
    }
}
