using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonkAnim : MonoBehaviour {

	public Vector3 motionSpeed = Vector3.zero;

	void Start () {
		Destroy(gameObject, 2f);
	}
	void Update () {
		GetComponent<RectTransform>().anchoredPosition3D += motionSpeed * Time.deltaTime;
	}
}
