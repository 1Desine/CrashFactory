using System;
using System.Security.Cryptography;
using UnityEngine;

public class VoxelTool : MonoBehaviour {
    public static VoxelTool Instance { get; private set; }


    public Action<bool> OnSetToolActive;
    private bool toolIsActive;

    public Action<VoxelType> OnSetBrush;
    private VoxelType currentBrush;

    public enum VoxelType {
        Doser,
        Solid,
        Road,
    }

    private void Awake() {
        Instance = this;

        toolIsActive = false;
        currentBrush = VoxelType.Doser;
    }


    private void Update() {
        if (Input.GetKeyDown(KeyCode.V)) {
            toolIsActive = !toolIsActive;
            OnSetToolActive?.Invoke(toolIsActive);
            OnSetBrush?.Invoke(currentBrush);
        }
        if (toolIsActive == false) return;

        if (Input.GetKeyDown(KeyCode.B)) {
            currentBrush++;
            if ((int)currentBrush > 2) currentBrush = 0;
            OnSetBrush?.Invoke(currentBrush);
        }


        Ray ray = Player.Instance.GetPlayerCamera().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            Vector3Int cellPosition = Vector3Int.RoundToInt(hit.point);

            if (hit.transform.parent.TryGetComponent(out Voxel voxel)) {
                cellPosition = Vector3Int.RoundToInt(voxel.transform.position);

                Debug.DrawLine(voxel.transform.position, cellPosition + hit.normal, Color.green);
            }

            if (GameInput.Instance.GetMainActionButtonDown()) {
                switch (currentBrush) {
                    case VoxelType.Doser: {
                        if (voxel == null) break;
                        Level.Instance.TryRemoveVoxel(Vector3Int.RoundToInt(voxel.transform.position));
                        break;
                    }
                    case VoxelType.Solid:
                    case VoxelType.Road:
                    Level.Instance.TryAddVoxel(currentBrush.ToString(), Vector3Int.RoundToInt(cellPosition + hit.normal));
                    break;
                }
            }

            Vector3 lb = cellPosition + Vector3.back * 0.5f + Vector3.left * 0.5f;
            Vector3 lf = cellPosition + Vector3.forward * 0.5f + Vector3.left * 0.5f;
            Vector3 rb = cellPosition + Vector3.back * 0.5f + Vector3.right * 0.5f;
            Vector3 rf = cellPosition + Vector3.forward * 0.5f + Vector3.right * 0.5f;
            Debug.DrawLine(lb, rb, Color.yellow);
            Debug.DrawLine(lf, rf, Color.yellow);
            Debug.DrawLine(lf, lb, Color.yellow);
            Debug.DrawLine(rf, rb, Color.yellow);
            Debug.DrawRay(lb, Vector3.down, Color.yellow);
            Debug.DrawRay(lf, Vector3.down, Color.yellow);
            Debug.DrawRay(rb, Vector3.down, Color.yellow);
            Debug.DrawRay(rf, Vector3.down, Color.yellow);

            Debug.DrawRay(hit.point, hit.normal, Color.blue);
        }


    }


}
