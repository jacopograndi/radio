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

    public float invincibilityTime = 0;

    public AudioSource audioAccel;
    public AudioSource audioDecel;

    public Vector3 gravity = new Vector3();

    void Start() {
        characterController = GetComponent<CharacterController>();
        audioAccel = transform.Find("audioSourceAccel").GetComponent<AudioSource>();
        audioDecel = transform.Find("audioSourceDecel").GetComponent<AudioSource>();
    }

    void Update() {
        if (still) return;
        float acceleration = Input.GetAxis("Vertical") * accelerationSensitivity;
        acceleration *= 0.2f;
        acceleration *= Mathf.Exp(-Mathf.Abs(v)*0.1f);
        if (acceleration < 0 && v > 1) acceleration *= Mathf.Sqrt(v);
        if (acceleration < 0 && v < 1) acceleration *= 0.1f;
        v += acceleration * Time.deltaTime * 60;
        if (v < 0) v *= 0.99f;
        else v *= 0.999f; // friction

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

        if (characterController.isGrounded) {
            gravity = new Vector3();
		} else {
            gravity += Vector3.down * 0.3f 
                * Time.deltaTime;
		}

        characterController.Move(velocity * Time.deltaTime + gravity);
        headingAngle += deltaHeadingAngle * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0, 90 - headingAngle * Mathf.Rad2Deg, 0);
        float bankingVel = deltaHeadingAngle * 3;
        bankingPivot.transform.localRotation = Quaternion.Euler(0, 0, bankingVel);
        steeringPivot.transform.localRotation = Quaternion.Euler(0, -steeringAngle * Mathf.Rad2Deg, 0);

        textVelocity.text = v.ToString("F1");

        if (acceleration > 0) {
            audioAccel.volume = acceleration;
            audioDecel.volume = 0;
        } else {
            audioAccel.volume = 0;
            audioDecel.volume = 0.2f;
            audioDecel.pitch = v/20;
		}
    }

    private void OnControllerColliderHit (ControllerColliderHit hit) {
        float heightForgive = Mathf.Abs(hit.point.z - transform.position.z);
        float isUp = Vector3.Cross(hit.normal, Vector3.up).magnitude;
        if (isUp > 0.8f && Mathf.Abs(v) > 5 && heightForgive > 0.5f) {
            if (invincibilityTime < Time.time) {
                v *= 0.5f;
                bonked = true;
                invincibilityTime = Time.time + GameState.playerBonkCooldownTimer;
            } 
        }
    }
}
