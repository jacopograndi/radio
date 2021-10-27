using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {

	Camera cam;

	Vector3 mouseLast;

	void Start () {
		cam = GetComponent<Camera>();
		mouseLast = Input.mousePosition;
	}

	void Update () {
		cam.orthographicSize -= Input.mouseScrollDelta.y * (cam.orthographicSize*0.1f);
		cam.orthographicSize = Mathf.Max(cam.orthographicSize, 50);
		cam.orthographicSize = Mathf.Min(cam.orthographicSize, 1000);

		var delta = mouseLast - Input.mousePosition;
		mouseLast = Input.mousePosition;

		if (Input.GetMouseButton(0)) {
			var move = new Vector3(delta.x, 0, delta.y);
			cam.transform.position += move;
		}
	}
}
