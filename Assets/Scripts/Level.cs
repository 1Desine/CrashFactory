#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.InputSystem;

public class Level : MonoBehaviour {
    public static Level Instrance { get; private set; }

    const string SAVES_PATH = "\\Levels\\";
    [SerializeField] private string fileName;


    [SerializeField] private VoxelsSO voxelsSO;
    [SerializeField] private Transform solidVoxelsHolder;
    [SerializeField] private Transform roadVoxelsHolder;

    [SerializeField] private CarsSO carsSO;





    public Action OnDeleteVoxels;


    private Dictionary<Vector3, Voxel> voxelsDictionary;


    private void Awake() {
        Instrance = this;

        voxelsDictionary = new Dictionary<Vector3, Voxel>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            foreach (var dic in voxelsDictionary) {
                if (dic.Value is RoadVoxel road) Debug.Log("road");
                if (dic.Value is SolidVoxel solid) Debug.Log("solid");
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            pathList = GetPathByRoad(Vector3.up, new Vector3(3, 1, 3));
        }
    }


    [Serializable]
    public class LevelInfo {
        // info


        // Voxels
        //solid
        public List<Vector3> solidVoxelsPositions = new List<Vector3>();
        //road
        public List<Vector3> roadVoxelsPositions = new List<Vector3>();

        // cars


    }


    private void SaveLevelToJson() {
        LevelInfo LevelInfo = new LevelInfo();

        // Voxels
        foreach (var dic in voxelsDictionary) {
            //solid
            if (dic.Value is SolidVoxel solidVoxel) {
                LevelInfo.solidVoxelsPositions.Add(dic.Value.transform.position);
            }
            //road
            if (dic.Value is RoadVoxel roadVoxel) {
                LevelInfo.solidVoxelsPositions.Add(dic.Value.transform.position);
            }
        }


        SaveSystem.WriteJson(SAVES_PATH + fileName, SaveSystem.SerializeJson(LevelInfo));
    }
    private void LoadLevelFromJson() {
        LevelInfo LevelInfo = SaveSystem.DeserializeJson<LevelInfo>(SaveSystem.ReadJson(SAVES_PATH + fileName));

        // Voxels
        OnDeleteVoxels?.Invoke();
        //solid
        voxelsDictionary.Clear();
        for (int i = 0; i < LevelInfo.solidVoxelsPositions.Count; i++) {
            SolidVoxel newVoxel = Instantiate(new SolidVoxel(), solidVoxelsHolder);

            newVoxel.transform.position = LevelInfo.solidVoxelsPositions[i];

            voxelsDictionary.Add(LevelInfo.solidVoxelsPositions[i], newVoxel);
        }
        //road
        voxelsDictionary.Clear();
        for (int i = 0; i < LevelInfo.solidVoxelsPositions.Count; i++) {
            RoadVoxel newVoxel = Instantiate(new RoadVoxel(), solidVoxelsHolder);

            newVoxel.transform.position = LevelInfo.solidVoxelsPositions[i];

            voxelsDictionary.Add(LevelInfo.solidVoxelsPositions[i], newVoxel);
        }

    }

    public void TryAddVoxel(string voxelType, Vector3 position) {
        voxelsDictionary.TryGetValue(position, out Voxel voxelFromDictionary);
        if (voxelFromDictionary != null) {
            return;
        }


        switch (voxelType) {
            case "Solid": {
                Voxel newVoxel = Instantiate(voxelsSO.solidVoxelPrefabs, solidVoxelsHolder);
                newVoxel.transform.position = position;
                voxelsDictionary.Add(position, newVoxel);
                break;
            }
            case "Road": {
                Voxel newVoxel = Instantiate(voxelsSO.roadVoxelPrefabs, roadVoxelsHolder);
                newVoxel.transform.position = position;
                voxelsDictionary.Add(position, newVoxel);
                break;
            }
            default: {
                Debug.LogError("me. Wrong string, type of Voxel");
                break;
            }
        }

    }
    public void TryRemoveVoxel(Vector3 position) {
        voxelsDictionary.TryGetValue(position, out Voxel voxelFromDictionary);
        if (voxelFromDictionary == null) {
            return;
        }
        voxelFromDictionary.DestroySelf();
        voxelsDictionary.Remove(position);
    }



    List<Vector3> pathList = new List<Vector3>();

    [ExecuteInEditMode]
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        if (pathList != null)
            foreach (var pathPoint in pathList) {
                Gizmos.DrawSphere(pathPoint, 0.3f);
            }
    }




    private class PathNode {
        public Vector3 position;
        public float g; // distanceFromStart;
        public float h; // distanceFromEnd;
        public float f { get { return g + h; } }

        public PathNode parent;
    }
    public List<Vector3> GetPathByRoad(Vector3 startPosition, Vector3 endPosition) {
        PathNode lastPathNode = GetLastNode(startPosition, endPosition);
        if (lastPathNode == null) {
            Debug.Log("no path found");
            return null;
        }

        List<Vector3> newPathList = new List<Vector3>();

        PathNode current = lastPathNode;
        while (current.parent != null) {
            newPathList.Add(current.position);
            current = current.parent;
        }
        newPathList.Reverse();

        StartCoroutine(DrawPath_Coroutine(newPathList));

        return newPathList;
    }

    private IEnumerator DrawPath_Coroutine(List<Vector3> list) {
        for (int i = 0; i < list.Count; i++) {
            Debug.DrawRay(list[i], Vector3.up, Color.green, 1);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private PathNode GetLastNode(Vector3 startPosition, Vector3 endPosition) {
        // 3D grid of nodes (only roads)
        Dictionary<Vector3, PathNode> pathNodes = new Dictionary<Vector3, PathNode>();
        foreach (var voxel in voxelsDictionary) {
            if (voxel.Value is RoadVoxel) {
                pathNodes.Add(voxel.Key, new PathNode {
                    position = voxel.Key,
                    h = (voxel.Key - endPosition).magnitude,
                });
            }
        }
        if (pathNodes.TryGetValue(startPosition, out PathNode startNode)) {
            startNode.g = 0;
        }
        else {
            Debug.Log("startNode is NOT found");
            return null;
        }


        Dictionary<Vector3, PathNode> opened = new Dictionary<Vector3, PathNode>();
        Dictionary<Vector3, PathNode> closed = new Dictionary<Vector3, PathNode>();
        opened.Add(startPosition, startNode);


        // offsets for later use
        List<Vector3> neibors = new List<Vector3> {
                Vector3.forward, Vector3.right, Vector3.back, Vector3.left,
                Vector3.forward + Vector3.right, Vector3.right + Vector3.back, Vector3.back + Vector3.left, Vector3.left + Vector3.forward,
        };


        int iterations = 1000;
        while (--iterations > 0) {
            if (opened.Count == 0) {
                Debug.Log(closed.Count);
                foreach (var c in closed) {
                    Debug.DrawRay(c.Value.position + Vector3.up, Vector3.up, Color.red, 1f);
                }
                return null;
            }

            PathNode currentNode = opened.First().Value;

            float f_lowest = float.PositiveInfinity;
            foreach (PathNode pathNode in opened.Values) {
                if (pathNode.f < f_lowest) {
                    f_lowest = pathNode.f;
                    currentNode = pathNode;
                }
            }
            opened.Remove(currentNode.position);
            closed.Add(currentNode.position, currentNode);

            if (currentNode.position == endPosition) return currentNode; // found the end node


            foreach (var neibor in neibors) {
                pathNodes.TryGetValue(currentNode.position + neibor, out PathNode neiborNode);

                if (neiborNode == null // if not a roadVoxel
                    || closed.ContainsKey(neiborNode.position) == true) // in closed
                    continue;

                neiborNode.g = currentNode.g + (currentNode.position - neiborNode.position).magnitude;
                neiborNode.parent = currentNode;
                if (opened.ContainsKey(neiborNode.position) == false) { // not in open
                    opened.Add(neiborNode.position, neiborNode);
                }
            }
        }

        return null;
    }




#if UNITY_EDITOR
    [CustomEditor(typeof(Level))]
    public class LevelEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            Level level = (Level)target;

            if (GUILayout.Button("SaveToJson")) {

            }
            if (GUILayout.Button("ReadFromJson")) {

            }

        }
    }
#endif



}
