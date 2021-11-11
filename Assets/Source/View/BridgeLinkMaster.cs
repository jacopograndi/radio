using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeLinkMaster : MonoBehaviour {

    public string nameId;
    public float timer = 0;
    public float timerMin = 0;
    public float timerMax = 60;

    public float angleOpen = 45;

    GameStateController controller;

    public List<GameObject> bridgePivot = new List<GameObject>();

    public void setScale (float scale) {
        var p0 = transform.Find("BridgePivot.0").gameObject;
        p0.transform.localPosition = new Vector3(scale, -0.5f, 0);
        p0.transform.Find("BridgeSection").localPosition = new Vector3(-scale/2, 0, 0);
        p0.transform.Find("BridgeSection").localScale = new Vector3(scale, 1, 30);
        bridgePivot.Add(p0);
        var p1 = transform.Find("BridgePivot.1").gameObject;
        p1.transform.localPosition = new Vector3(-scale, -0.5f, 0);
        p1.transform.Find("BridgeSection").localPosition = new Vector3(scale/2, 0, 0);
        p1.transform.Find("BridgeSection").localScale = new Vector3(scale, 1, 30);
        bridgePivot.Add(p1);

	}

    public void SetPivotRotation (float angle) {
        foreach (var pivot in bridgePivot) {
            int heading = pivot.transform.localPosition.x < 0 ? 1 : -1;
            pivot.transform.rotation = transform.rotation * Quaternion.Euler(
                0, 0, angle * heading
            );
        }
    }

    public void Refresh() {
        if (!controller) controller = FindObjectOfType<GameStateController>();

        var obsTimer = controller.gameState.timerList.getTimer(nameId);
        if (obsTimer != null) {
            timer = obsTimer.time;
            float quarter = (timerMax - timerMin) / 4;
            if (timer < quarter)
                SetPivotRotation(0);

            if (timer >= quarter && timer < quarter * 2)
                SetPivotRotation((timer - quarter) / quarter * angleOpen);

            if (timer >= quarter * 2 && timer < quarter * 3)
                SetPivotRotation(angleOpen);

            if (timer >= quarter * 3)
                SetPivotRotation(angleOpen - (timer - quarter * 3) / quarter * angleOpen);
        } else {
            Debug.LogWarning("no timer found for " + nameId);
        }
    }
}
