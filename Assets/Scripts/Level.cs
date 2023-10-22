#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class Level : MonoBehaviour {
    public static Level Instance { get; private set; }

    const string SAVES_PATH = "\\Levels\\";
    [SerializeField] private string fileName;


    [SerializeField] private VoxelsSO voxelsSO;
    [SerializeField] private Transform solidVoxelsHolder;
    [SerializeField] private Transform roadVoxelsHolder;

    [SerializeField] private CarsSO carsSO;
    [SerializeField] private Transform carHolder;

    [SerializeField] private BuildingsSO buildingsSO;
    [SerializeField] private Transform buildingHolder;





    public Action OnClearLevel;


    private Dictionary<Vector3Int, Voxel> voxelsDictionary = new Dictionary<Vector3Int, Voxel>();
    private List<Car> carsList = new List<Car>();
    private Dictionary<int, Building> buildingsList = new Dictionary<int, Building>();


    private void Awake() {
        Instance = this;

        voxelsDictionary = new Dictionary<Vector3Int, Voxel>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            pathList = GetPathByRoad(Vector3Int.up, new Vector3Int(3, 1, 3));
        }
    }


    [Serializable]
    public class LevelInfo {
        // info


        // Voxels
        public List<Vector3Int> solidVoxelsPositions = new List<Vector3Int>(); //solid
        public List<Vector3Int> roadVoxelsPositions = new List<Vector3Int>(); //road

        // Cars
        public List<Vector3> carsPositions = new List<Vector3>();
        public List<Quaternion> carsRotations = new List<Quaternion>();
        public List<Car.Type> carsTypes = new List<Car.Type>();

        // Buildings
        public List<Vector3> buildingsPositions = new List<Vector3>();
        public List<Quaternion> buildingsRotations = new List<Quaternion>();
        public List<Building.Type> buildingsTypes = new List<Building.Type>();

    }
    private void SaveLevelToJson() {
        LevelInfo LevelInfo = new LevelInfo();

        // Voxels
        foreach (var voxel in voxelsDictionary) {
            //solid
            if (voxel.Value is SolidVoxel solidVoxel) {
                LevelInfo.solidVoxelsPositions.Add(Vector3Int.RoundToInt(voxel.Value.transform.position));
            }
            //road
            if (voxel.Value is RoadVoxel roadVoxel) {
                LevelInfo.roadVoxelsPositions.Add(Vector3Int.RoundToInt(voxel.Value.transform.position));
            }
        }

        // Cars
        foreach (Car car in carsList) {
            Car.CarInfo carInfo = car.GetCarInfo();
            LevelInfo.carsPositions.Add(carInfo.position);
            LevelInfo.carsRotations.Add(carInfo.rotation);
            LevelInfo.carsTypes.Add(carInfo.type);
        }

        // Buildings
        foreach (var building in buildingsList) {
            Building.BuildingInfo buildingInfo = building.Value.GetBuildingInfo();
            LevelInfo.buildingsPositions.Add(buildingInfo.position);
            LevelInfo.buildingsRotations.Add(buildingInfo.rotation);
            LevelInfo.buildingsTypes.Add(buildingInfo.type);
        }

        SaveSystem.WriteJson(SAVES_PATH + fileName, SaveSystem.SerializeJson(LevelInfo));
    }
    private void LoadLevelFromJson() {
        LevelInfo LevelInfo = SaveSystem.DeserializeJson<LevelInfo>(SaveSystem.ReadJson(SAVES_PATH + fileName));

        // Voxels
        OnClearLevel?.Invoke();
        voxelsDictionary.Clear();
        //solid
        for (int i = 0; i < LevelInfo.solidVoxelsPositions.Count; i++) {
            SolidVoxel newVoxel = Instantiate(voxelsSO.solidVoxelPrefabs, solidVoxelsHolder);

            newVoxel.transform.position = LevelInfo.solidVoxelsPositions[i];

            voxelsDictionary.Add(LevelInfo.solidVoxelsPositions[i], newVoxel);
        }
        //road
        for (int i = 0; i < LevelInfo.roadVoxelsPositions.Count; i++) {
            RoadVoxel newVoxel = Instantiate(voxelsSO.roadVoxelPrefabs, solidVoxelsHolder);

            newVoxel.transform.position = LevelInfo.roadVoxelsPositions[i];

            voxelsDictionary.Add(LevelInfo.roadVoxelsPositions[i], newVoxel);
        }

        // Cars
        carsList.Clear();
        for (int i = 0; i < LevelInfo.carsTypes.Count; i++) {
            Car newCar = Instantiate(carsSO.GetCarByType(LevelInfo.carsTypes[i]), carHolder);
            newCar.transform.position = LevelInfo.carsPositions[i];
            newCar.transform.rotation = LevelInfo.carsRotations[i];
        }

        // Buildings
        buildingsList.Clear();
        for (int i = 0; i < LevelInfo.buildingsTypes.Count; i++) {
            Building newBuilding = Instantiate(buildingsSO.GetBuildingByType(LevelInfo.buildingsTypes[i]), buildingHolder);
            newBuilding.transform.position = LevelInfo.buildingsPositions[i];
            newBuilding.transform.rotation = LevelInfo.buildingsRotations[i];
        }

    }
    public void TryAddVoxel(string voxelType, Vector3Int position) {
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
    public void TryRemoveVoxel(Vector3Int position) {
        voxelsDictionary.TryGetValue(position, out Voxel voxelFromDictionary);
        if (voxelFromDictionary == null) {
            return;
        }
        voxelFromDictionary.DestroySelf();
        voxelsDictionary.Remove(position);
    }

    public void RegisterCar(Car car) => carsList.Add(car);
    public void UnregisterCar(Car car) => carsList.Remove(car);

    public void RegisterBuilding(Building building) {
        building.id = buildingsList.Count == 0 ? 0 : buildingsList.Last().Key;
        buildingsList.TryAdd(building.id, building);
    }
    public void UnregisterBuilding(Building building) => buildingsList.Remove(building.id);

    public void RegisterVoxel(Voxel voxel) => voxelsDictionary.TryAdd(Vector3Int.RoundToInt(voxel.transform.position), voxel);

    public List<Car> GetCarsList() => carsList;




    List<Vector3Int> pathList = new List<Vector3Int>();

    [ExecuteInEditMode]
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        if (pathList != null)
            foreach (var pathPoint in pathList)
                Gizmos.DrawSphere(pathPoint, 0.3f);
    }







    private class PathNode {
        public Vector3Int position;
        public float g; // distanceFromStart;
        public float h; // distanceFromEnd;
        public float f { get { return g + h; } }

        public PathNode parent;
    }
    public List<Vector3Int> GetPathByRoad(Vector3Int startPosition, Vector3Int endPosition) {
        PathNode lastPathNode = GetLastPathNode(startPosition, endPosition);
        List<Vector3Int> newPathList = new List<Vector3Int>();
        if (lastPathNode == null) {
            Debug.Log("no path found");
            return newPathList;
        }


        PathNode current = lastPathNode;
        while (current.parent != null) {
            newPathList.Add(current.position);
            current = current.parent;
        }
        newPathList.Reverse();

        StartCoroutine(DrawPath_Coroutine(newPathList));

        return newPathList;
    }

    private IEnumerator DrawPath_Coroutine(List<Vector3Int> list) {
        for (int i = 0; i < list.Count; i++) {
            Debug.DrawRay(list[i], Vector3Int.up, Color.green, 1);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private PathNode GetLastPathNode(Vector3Int startPosition, Vector3Int endPosition) {
        // 3D grid of nodes (only roads)
        Dictionary<Vector3Int, PathNode> pathNodes = new Dictionary<Vector3Int, PathNode>();
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


        Dictionary<Vector3Int, PathNode> opened = new Dictionary<Vector3Int, PathNode>();
        Dictionary<Vector3Int, PathNode> closed = new Dictionary<Vector3Int, PathNode>();
        opened.Add(startPosition, startNode);


        // offsets for later use
        List<Vector3Int> neibors = new List<Vector3Int> {
                Vector3Int.forward, Vector3Int.right, Vector3Int.back, Vector3Int.left,
                //Vector3Int.forward + Vector3Int.right, Vector3Int.right + Vector3Int.back, Vector3Int.back + Vector3Int.left, Vector3Int.left + Vector3Int.forward,
        };


        while (true) {
            if (opened.Count == 0) {
                Debug.Log(closed.Count);
                foreach (var c in closed) {
                    Debug.DrawRay(c.Value.position + Vector3Int.up, Vector3Int.up, Color.red, 1f);
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
    }




#if UNITY_EDITOR
    [CustomEditor(typeof(Level))]
    public class LevelEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            Level level = (Level)target;

            if (GUILayout.Button("SaveToJson")) {
                level.SaveLevelToJson();
            }
            if (GUILayout.Button("ReadFromJson")) {
                level.LoadLevelFromJson();
            }

            if (GUILayout.Button("RemoveOldObjects /Editor only!")) {
                // remove old objects
                for (int i = 0; i < level.roadVoxelsHolder.childCount; i++) {
                    DestroyImmediate(level.roadVoxelsHolder.GetChild(i).gameObject);
                }
                for (int i = 0; i < level.solidVoxelsHolder.childCount; i++) {
                    DestroyImmediate(level.solidVoxelsHolder.GetChild(i).gameObject);
                }
                for (int i = 0; i < level.buildingHolder.childCount; i++) {
                    DestroyImmediate(level.buildingHolder.GetChild(i).gameObject);
                }
                for (int i = 0; i < level.carHolder.childCount; i++) {
                    DestroyImmediate(level.carHolder.GetChild(i).gameObject);
                }
            }

        }
    }
#endif



}
