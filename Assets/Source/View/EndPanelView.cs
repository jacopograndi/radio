using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndPanelView : MonoBehaviour {
    
    GameStateController controller;

    public void Refresh () {
        controller = FindObjectOfType<GameStateController>();

        bool ended = controller.gameState.isWon() || controller.gameState.isLost();
        if (ended && !gameObject.activeSelf) {
            gameObject.SetActive(true);
            if (controller.gameState.isLost()) {
                gameObject.GetComponentInChildren<TMP_Text>().text = "You lose";
            }
        } else if (!ended && gameObject.activeSelf) {
            gameObject.SetActive(false);
        }
    }
}
