#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

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




    public List<Vector2> GetPathToPoint(Vector3 start, Vector3 end) {
        List<Vector2> path = new List<Vector2>();


        foreach (var dic in voxelsDictionary) {
            if (dic.Value is RoadVoxel road) Debug.Log("road");
            if (dic.Value is SolidVoxel solid) Debug.Log("solid");
        }


        return path;
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
