using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLoader : MonoBehaviour {

    public GameObject car;
    public List<GameObject> carGraphics = new List<GameObject>();

    public bool loaded = false;

    void Start() {
        if (!loaded) Load();
    }

    public void Load() {
        var cars = Resources.LoadAll<GameObject>("Prefabs/Cars");
        foreach (var car in cars) carGraphics.Add(car);
        loaded = true;
    }
}
