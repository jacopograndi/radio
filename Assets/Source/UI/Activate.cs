using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activate : MonoBehaviour {

	public GameObject target;
	public bool deactivateSelf = true;

	public void Act () {
		target.SetActive(true);
		if (deactivateSelf) {
			gameObject.SetActive(false);
		}
	}
}
