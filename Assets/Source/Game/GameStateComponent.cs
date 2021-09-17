using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateComponent : MonoBehaviour {

    public GameState gameState;

    void Start() {
        gameState = new GameState();
        gameState.loadExample();
        RequireRefresh();
    }

    void Update() {
        gameState.timeLeft -= Time.deltaTime;
        RequireRefresh();
    }

    void RequireRefresh() {
        SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
    }
}
