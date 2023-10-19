using UnityEngine;

public class GameInput : MonoBehaviour {
    public static GameInput Instance { get; private set; }

    private PlayerInputAction playerInputAction;


    private bool previousMainActionButton;


    private void Awake() {
        Instance = this;

        playerInputAction = new PlayerInputAction();
        playerInputAction.Player.Enable();
    }


    public bool GetMainActionButtonDown() {
        bool switchedOn = false;
        if (previousMainActionButton == false && playerInputAction.Player.MainActionButton.IsInProgress()) {
            switchedOn = true;
        }
        previousMainActionButton = playerInputAction.Player.MainActionButton.IsInProgress();
        return switchedOn;
    }

    public float GetCameraZoomFloat() {
        return playerInputAction.Player.CameraZoomFloat.ReadValue<float>();
    }
    public bool GetSecondaryActionButton() {
        return playerInputAction.Player.SecondaryActionButton.IsInProgress();
    }
    public bool GetPivotAroundButton() {
        return playerInputAction.Player.PivotAroundPointButton.IsInProgress();
    }
    public Vector3 GetMoveVector3() {
        return playerInputAction.Player.MoveVector3.ReadValue<Vector3>();
    }
    public Vector2 GetLookVector2() {
        return playerInputAction.Player.LookVector2.ReadValue<Vector2>();
    }



}
