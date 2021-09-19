using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialCulling : MonoBehaviour {

    List<GameObject> gameObjects = new List<GameObject>();
    public List<GameObject> targets = new List<GameObject>();

    public float cullingDistance;

    void checkForTargets () {
        if (targets.Count == 0) {
            var ts = GameObject.FindGameObjectsWithTag("Player");
            foreach (var t in ts) {
                targets.Add(t);
            }
        }
    }

    void Start() {
        foreach (Transform child in transform) {
            gameObjects.Add(child.gameObject);
        }
    }

    void Update() {
        checkForTargets();
        float cullingDistanceSqr = cullingDistance * cullingDistance;
        foreach (var obj in gameObjects) {
            float mindist = 9999999;
            foreach (var t in targets) {
                float dist = Vector3.SqrMagnitude(t.transform.position - obj.transform.position);
                if (dist < mindist) { mindist = dist; }
            }
            bool activate = mindist < cullingDistanceSqr;
            if (!obj.activeSelf && activate) {
                obj.SetActive(true);
            } else if (obj.activeSelf && !activate) {
                obj.SetActive(false);
            }
        }
    }
}
