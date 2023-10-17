using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    [SerializeField] private Camera playerCamera;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float hightChange = 1f;


    private void Awake() {
        Instance = this;
    }


    private void Update() {
        if (GameInput.Instance.GetMoveAndLookButton()) {
            Vector2 moveInput = GameInput.Instance.GetMoveVector();
            Vector2 lookInput = GameInput.Instance.GetLookDeltaVector();

            transform.position += (transform.right * moveInput.x + transform.forward * moveInput.y) * moveSpeed * Time.deltaTime;
            transform.eulerAngles += Vector3.up * lookInput.x * lookSensitivity;

            playerCamera.transform.eulerAngles = new Vector3(
                Mathf.Clamp(playerCamera.transform.eulerAngles.x - lookInput.y * lookSensitivity, 0, 90),
                playerCamera.transform.eulerAngles.y,
                playerCamera.transform.eulerAngles.z);


        }


        transform.position += Vector3.up * hightChange * GameInput.Instance.GetCameraHightDeltaFloat() * Time.deltaTime;

    }


    public Camera GetPlayerCamera() {
        return Instance.playerCamera;
    }

}
