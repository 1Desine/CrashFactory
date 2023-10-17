using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    [SerializeField] private Camera playerCamera;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float cameraDistanceChenge = 1f;

    private Vector3 lookPivotPoint;

    private void Awake() {
        Instance = this;
    }


    private void Update() {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        // pivot around 
        if (GameInput.Instance.GetPivotAroundButton()) {
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

            if (lookPivotPoint != Vector3.zero) {

                Vector2 moveInput = GameInput.Instance.GetMoveVector();
                Vector2 lookInput = GameInput.Instance.GetLookDeltaVector();

                Vector3 toPoint = lookPivotPoint - transform.position;
                Vector3 upMoveDirection = Vector3.Cross(transform.right, toPoint);
                Vector3 rightMoveDirection = Vector3.Cross(transform.up, toPoint);

                transform.position += (upMoveDirection * lookInput.y + rightMoveDirection * -lookInput.x) * lookSensitivity * Mathf.PI / 180;

                transform.eulerAngles += Vector3.up * lookInput.x * lookSensitivity;
                playerCamera.transform.eulerAngles = new Vector3(
                    Mathf.Clamp(playerCamera.transform.eulerAngles.x - lookInput.y * lookSensitivity, 0, 90),
                    playerCamera.transform.eulerAngles.y,
                    playerCamera.transform.eulerAngles.z);
            }
            else if (hit.collider != null) {
                lookPivotPoint = hit.point;
            }
        }
        else lookPivotPoint = Vector3.zero;

        // free camera
        if (GameInput.Instance.GetFreeCameraButton()) {
            Vector2 moveInput = GameInput.Instance.GetMoveVector();
            Vector2 lookInput = GameInput.Instance.GetLookDeltaVector();

            transform.position += (transform.right * moveInput.x + transform.forward * moveInput.y) * moveSpeed * Time.deltaTime;
            transform.eulerAngles += Vector3.up * lookInput.x * lookSensitivity;

            playerCamera.transform.eulerAngles = new Vector3(
                Mathf.Clamp(playerCamera.transform.eulerAngles.x - lookInput.y * lookSensitivity, 0, 90),
                playerCamera.transform.eulerAngles.y,
                playerCamera.transform.eulerAngles.z);
        }

        // zoom in and out
        transform.position += ray.direction * GameInput.Instance.GetCameraHightDeltaFloat() * cameraDistanceChenge * Time.deltaTime;
    }


    public Camera GetPlayerCamera() {
        return Instance.playerCamera;
    }

}
