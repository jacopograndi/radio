using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Permanent : MonoBehaviour {

    public static Permanent instance;

    public const int gamePort = 49999;
    public NetUDP net = new NetUDP(gamePort);

    public LobbyConfiguration config = new LobbyConfiguration();

    public NetRadio radio = null;

	public string localNameId;

    public static Permanent get() {
        if (instance == null) {
            instance = FindObjectOfType<Permanent>();
            if (instance == null) {
                GameObject obj = new GameObject();
                obj.name = typeof(Permanent).Name;
                instance = obj.AddComponent<Permanent>();
				var netradio = obj.AddComponent<NetRadio>();
				var radioin = obj.AddComponent<RadioIn>();
				radioin.netRadio = netradio;
                DontDestroyOnLoad(instance.gameObject);
            }
        }
        return instance;
    }

    public NetRadio getRadio () {
        if (!radio) radio = GetComponent<NetRadio>();
        return radio;
	}

    void OnApplicationQuit() {
        var perm = get();
        if (perm != null) perm.net.close();
    }
}
