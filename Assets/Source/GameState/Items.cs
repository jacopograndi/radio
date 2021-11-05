using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Item {
	public int id;
	public string name;
	public int fragility;
	public float weight;
}


[System.Serializable]
public class Items {
	public List<Item> items = new List<Item>();
}
