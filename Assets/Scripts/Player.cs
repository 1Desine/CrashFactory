using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Player : MonoBehaviour {
    public static Player Instance { get; private set; }

    [SerializeField] private Camera playerCamera;

    [Header("Movement")]
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private AnimationCurve moveSpeedCurve;
    [SerializeField] private float cameraDistanceChenge = 1f;
    [SerializeField] private AnimationCurve cameraDictanceChengeCurve;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float zoomPitch = 10f;
    [SerializeField] private float applyZoomPitchSinseDistance = 10f;
    [SerializeField] private float zoomYaw = 0.03f;
    [SerializeField] private float applyZoomYawSinseDistance = 20f;
    [SerializeField] private AnimationCurve zoomTiltCurve;


    private float minHeight = 1;
    private float maxHeight = 100;


    private Vector3 lookPivotPoint;

    private void Awake() {
        Instance = this;
    }


    private void Update() {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        // pivot around 
        if (GameInput.Instance.GetPivotAroundButton()) {
            if (lookPivotPoint != Vector3.zero)
                PivotAroundPoint(lookPivotPoint, GameInput.Instance.GetLookVector2() * lookSensitivity);
            else if (Physics.Raycast(ray, out RaycastHit hit)) 
                lookPivotPoint = hit.point;
        }
        else lookPivotPoint = Vector3.zero;


        var hight = transform.position.y;
        // free camera
        if (GameInput.Instance.GetSecondaryActionButton()) {
            RotatePlayerY_CameraX(GameInput.Instance.GetLookVector2() * lookSensitivity);
        }

        // zoom in and out
        float zoomInput = GameInput.Instance.GetCameraZoomFloat() * zoomSpeed * Time.deltaTime;
        if (zoomInput != 0) {
            Vector3 desiredPosition = transform.position + ray.direction * zoomInput * cameraDistanceChenge * cameraDictanceChengeCurve.Evaluate(hight / maxHeight);

            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            if (hit.collider != null) {
                if ((transform.position - hit.point).magnitude < applyZoomPitchSinseDistance)
                    PivotAroundPoint(hit.point, Vector2.up * zoomInput * zoomPitch * zoomTiltCurve.Evaluate((transform.position - hit.point).magnitude / applyZoomPitchSinseDistance));

                if (desiredPosition.y < maxHeight)
                    transform.position = desiredPosition;

                if ((transform.position - hit.point).magnitude < applyZoomYawSinseDistance)
                    if (zoomInput > 0)
                        RotatePlayerY_CameraX(new Vector2(Vector3.SignedAngle(transform.forward, hit.point - transform.position, Vector3.up) * zoomInput * zoomYaw, 0));
            }
        }

        Vector3 moveInput = GameInput.Instance.GetMoveVector3();
        transform.position += (transform.right * moveInput.x + transform.up * moveInput.y + transform.forward * moveInput.z) * moveSpeed * moveSpeedCurve.Evaluate(hight / maxHeight) * Time.deltaTime;


        // hight check
        if (transform.position.y < minHeight) transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
        if (transform.position.y > maxHeight) transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
    }


    private void PivotAroundPoint(Vector3 pivotPoint, Vector2 rotate) {
        Vector3 toPoint = pivotPoint - transform.position;
        Vector3 upMoveDirection = Vector3.Cross(transform.right, toPoint);
        Vector3 rightMoveDirection = Vector3.Cross(transform.up, toPoint);

        transform.position += (upMoveDirection * rotate.y + rightMoveDirection * -rotate.x) * Mathf.PI / 180;

        RotatePlayerY_CameraX(rotate);
    }
    private void RotatePlayerY_CameraX(Vector2 rotate) {
        transform.eulerAngles += Vector3.up * rotate.x;

        playerCamera.transform.eulerAngles = new Vector3(
            Mathf.Clamp(playerCamera.transform.eulerAngles.x - rotate.y, 0, 90),
            playerCamera.transform.eulerAngles.y,
            playerCamera.transform.eulerAngles.z);
    }

    public Camera GetPlayerCamera() {
        return Instance.playerCamera;
    }

}
