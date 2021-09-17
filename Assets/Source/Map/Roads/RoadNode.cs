using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadNode : MonoBehaviour {

    public int lanes = 4;
    public float lenght = 100;

    public int id;
    public string RoadName;

    CarLoader cl;
    List<GameObject> cars = new List<GameObject>();
    int[] carsIndexes;

    float refreshTimer = 0;
    public float traffic = 0;
    public float trafficPhase = 0;
    public float trafficFreq = 100;

    public float refreshDistance = 150f;
    public bool masterMode = false;

    public List<GameObject> targets = new List<GameObject>();
    Renderer rendererCube;

    void Start() {
        if (targets.Count == 0) {
            var ts = GameObject.FindGameObjectsWithTag("Player");
            foreach (var t in ts) {
                targets.Add(t);
            }
        }

        if (!masterMode) cl = FindObjectOfType<CarLoader>();

        trafficPhase = Random.Range(0, 4f);
        trafficFreq = Random.Range(10f, 100f);
        refreshTimer = Random.Range(0, 1f);

        if (!masterMode) TrafficFill();
        else {
            rendererCube = GetComponentInChildren<Renderer>();
            rendererCube.material.mainTexture = null;
        }
    }

    void Update() {
        if (!masterMode) {
            float mindist = 9999999;
            foreach (var t in targets) {
                float dist = Vector3.SqrMagnitude(t.transform.position - transform.position);
                if (dist < mindist) { mindist = dist; }
            }
            if (mindist > refreshDistance * refreshDistance && refreshTimer < Time.time) {
                TrafficRefresh();
                refreshTimer = Time.time + 1f;
            }
        } else TrafficRefreshMaster();

        traffic = Mathf.Sin(Time.time / trafficFreq + trafficPhase) * 0.5f + 0.5f;
    }

    void PlaceCar(GameObject graphic, Vector3 pos, Quaternion rot) {
        var obj = Instantiate(cl.car, pos, rot);
        obj.transform.SetParent(transform);

        var objGr = Instantiate(graphic, pos, rot);
        objGr.transform.SetParent(obj.transform);

        cars.Add(obj);
    }

    void TrafficRefreshMaster() {
        rendererCube.material.color = Color.Lerp(Color.green, Color.red, traffic);
    }

    void TrafficRefresh() {
        if (!cl.loaded) { return; }

        for (int i = 0; i < cars.Count; i++) {
            if (i < cars.Count * traffic) {
                cars[carsIndexes[i]].SetActive(true);
            } else {
                cars[carsIndexes[i]].SetActive(false);
            }
        }
    }

    public static void Shuffle<T>(T[] array) {
        int n = array.Length;
        while (n > 1) {
            int k = Random.Range(0, n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public void TrafficFill() {
        if (!cl.loaded) {
            Invoke("TrafficFill", 0.1f);
            return;
        }

        float laneWidth = 15.0f/4;
        int numcars = (int)(lenght / 7);
        for (int i = 0; i < lanes; i++) {
            for (int j = 0; j < numcars; j++) {
                Vector3 pos = new Vector3(
                    (i +0.5f) * laneWidth - (float)laneWidth * lanes / 2.0f,
                    0,
                    j * lenght / numcars - lenght / 2
                );
                pos += new Vector3(Random.Range(-0.8f, 0.8f), 0, 0); // nudge
                Quaternion rot = transform.rotation;
                Quaternion rotPos = Quaternion.Euler(
                    rot.eulerAngles.x,
                    rot.eulerAngles.y + 90,
                    rot.eulerAngles.z
                );
                pos = rotPos * pos;
                pos += transform.position;
                if (i >= lanes / 2) { 
                    rot = Quaternion.Euler(
                        rot.eulerAngles.x,
                        rot.eulerAngles.y+180,
                        rot.eulerAngles.z
                    );  
                }
                GameObject prefab = cl.carGraphics[Random.Range(0, cl.carGraphics.Count)];
                PlaceCar(prefab, pos, rot);
            }
        }

        carsIndexes = new int[cars.Count];
        for (int i = 0; i < cars.Count; i++) carsIndexes[i] = i;
        Shuffle(carsIndexes);
    }

    public void TrafficEmpty() {
        if (!masterMode) {
            foreach (var car in cars) Destroy(car);
            cars.Clear();
        }
    }
}
