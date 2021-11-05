using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemView : MonoBehaviour {

    GameStateController controller;
    GameObject emptyPanel;
    GameObject itemPanel;

    public GameObject livesNot;
    public GameObject bonkFx;

	void Refresh () {
        if (!controller) controller = FindObjectOfType<GameStateController>();

        if (!emptyPanel) {
            emptyPanel = transform.Find("Empty").gameObject;
            itemPanel = transform.Find("Item").gameObject;
            emptyPanel.SetActive(true);
            itemPanel.SetActive(false);
		}
       
        if (controller.bonkNotification == 1) {
            controller.bonkNotification = 0;
            var obj = Instantiate(livesNot);
            obj.transform.SetParent(itemPanel.transform);
            obj.GetComponent<RectTransform>().anchoredPosition3D = 
                itemPanel.transform.Find("Lives").GetComponent<RectTransform>().anchoredPosition3D;

            var fx = Instantiate(bonkFx);
            fx.transform.SetParent(itemPanel.transform.parent.parent);
            fx.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
		}

		if (controller.taskLinks.Count == 0) return;
		if (!controller.started) return;
		
        var player = controller.getLocalPlayer();
        var playerRepr = controller.gameState.playerList.getPlayer(player.nameId);
        if (playerRepr.acceptedTaskId == -1) {
            if (!emptyPanel.activeSelf) emptyPanel.SetActive(true);
            if (itemPanel.activeSelf) itemPanel.SetActive(false);
		} else {
            if (emptyPanel.activeSelf) emptyPanel.SetActive(false);
            if (!itemPanel.activeSelf) itemPanel.SetActive(true);

            var itemId = controller.gameState.taskList.fromId(playerRepr.acceptedTaskId).itemId;
			var item = controller.items.items.Find(x => x.id == itemId);
            var invw = 1f / item.weight * 20;
            string descr = "Carrying a " + item.name + " (weighs " + invw.ToString("F1") + ")";
            string lives = "Lives: " + playerRepr.lives;

			itemPanel.transform.Find("Description").GetComponent<TMP_Text>().text = descr;
			itemPanel.transform.Find("Lives").GetComponent<TMP_Text>().text = lives;
		}
	}
}
