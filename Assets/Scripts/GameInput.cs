using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {
    public static GameInput Instance { get; private set; }

    private PlayerInputAction playerInputAction;


    private void Awake() {
        Instance = this;

        playerInputAction = new PlayerInputAction();
        playerInputAction.Player.Enable();
    }



    public float GetCameraHightDeltaFloat() {
        return playerInputAction.Player.CameraHeight.ReadValue<float>();
    }
    public bool GetMoveAndLookButton() {
        return playerInputAction.Player.MoveAndLook.IsInProgress();
    }
    public Vector2 GetMoveVector() {
        return playerInputAction.Player.Move.ReadValue<Vector2>();
    }
    public Vector2 GetLookDeltaVector() {
        return playerInputAction.Player.Look.ReadValue<Vector2>();
    }



}
