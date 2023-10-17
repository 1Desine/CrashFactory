using System;
using UnityEngine;

public class VoxelTool : MonoBehaviour {
    public static VoxelTool Instance { get; private set; }


    public Action<bool> OnSetToolActive;
    private bool toolIsActive;

    public Action<Voxel.Type> OnSetBrush;
    private Voxel.Type currentBrush;


    private void Awake() {
        Instance = this;

        toolIsActive = false;
        currentBrush = Voxel.Type.Solid;
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
            if ((int)currentBrush > 1) currentBrush = 0;
            OnSetBrush?.Invoke(currentBrush);
        }


        Ray ray = Player.Instance.GetPlayerCamera().ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {

            Vector3 cellPosition = new Vector3(
                    Mathf.Round(hit.point.x),
                    Mathf.Round(hit.point.y + 1),
                    Mathf.Round(hit.point.z));


            if (hit.transform.parent.TryGetComponent(out Voxel voxel)) {
                cellPosition = new Vector3(
                    Mathf.Round(voxel.transform.position.x),
                    Mathf.Round(voxel.transform.position.y),
                    Mathf.Round(voxel.transform.position.z));

                cellPosition += hit.normal;
                Debug.DrawLine(voxel.transform.position, cellPosition, Color.green);


                if (Input.GetMouseButtonDown(1)) {
                    Map.Instrance.TryRemoveVoxel(voxel.transform.position);
                }
            }

            if (Input.GetMouseButtonDown(0)) {
                Map.Instrance.TryAddVoxel(cellPosition, currentBrush);
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
