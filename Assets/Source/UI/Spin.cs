using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour {
	public float speed = 1;

	void Update () {
		transform.rotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0);
	}
}
