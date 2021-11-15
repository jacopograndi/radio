using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

    public GameObject bankingPivot;
    public GameObject steeringPivot;
    public TMP_Text textVelocity;

    public float angleSensitivity = 1;
    public float accelerationSensitivity = 1;

    public CharacterController characterController;

    public float v;
    float steeringVelocity;

    float headingAngle = 0;
    float steeringAngle = 0;

    public bool still = false;

    public float weight = 1;
    public bool bonked = true;

    void Start() {
        characterController = GetComponent<CharacterController>();
    }

    void Update() {
        if (still) return;
        float acceleration = Input.GetAxis("Vertical") * accelerationSensitivity;
        acceleration *= 0.2f;
        acceleration *= Mathf.Exp(-Mathf.Abs(v)*0.1f);
        if (acceleration < 0 && v > 1) acceleration *= Mathf.Sqrt(v);
        if (acceleration < 0 && v < 1) acceleration *= 0.1f;
        acceleration *= (0.5f+weight*0.5f);
        v += acceleration;
        if (v < 0) v *= 0.99f;
        else v *= 0.995f; // friction

        float steeringAcceleration = -Input.GetAxis("Horizontal") * angleSensitivity;
        steeringAcceleration *= 0.1f;
        steeringVelocity += steeringAcceleration;
        steeringVelocity *= 0.5f;
        steeringAngle += steeringVelocity;
        float maxSteeringAngle = Mathf.PI / 3.0f;
        steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle);
        steeringAngle *= 0.8f;

        float L = 1.5f;
        float lr = 0.75f;
        float beta = Mathf.Atan(lr * Mathf.Tan(steeringAngle) / L);
        float deltaHeadingAngle = v * Mathf.Tan(steeringAngle) * Mathf.Cos(beta) / L;

        //v *= Mathf.Exp(-Mathf.Abs(deltaHeadingAngle)*0.002f);
        v *= Mathf.SmoothStep(1, 0, Mathf.Abs(deltaHeadingAngle) * 0.003f);

        Vector3 velocity = new Vector3(
            v * Mathf.Cos(beta + headingAngle), 
            0,
            v * Mathf.Sin(beta + headingAngle)
        );
        Vector3 gravity = Vector3.down * 5 * Time.deltaTime; // linear
        characterController.Move(velocity * Time.deltaTime + gravity);
        headingAngle += deltaHeadingAngle * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0, 90 - headingAngle * Mathf.Rad2Deg, 0);
        float bankingVel = deltaHeadingAngle * 3;
        bankingPivot.transform.localRotation = Quaternion.Euler(0, 0, bankingVel);
        steeringPivot.transform.localRotation = Quaternion.Euler(0, -steeringAngle * Mathf.Rad2Deg, 0);

        textVelocity.text = v.ToString("F1");

    }

    private void OnControllerColliderHit (ControllerColliderHit hit) {
        float heightForgive = Mathf.Abs(hit.point.z - transform.position.z);
        float isUp = Vector3.Cross(hit.normal, Vector3.up).magnitude;
        if (isUp > 0.8f && Mathf.Abs(v) > 5 && heightForgive > 0.5f) {
            print(isUp + " " + heightForgive);
            v *= 0.5f;
            bonked = true;
        }
    }
}
