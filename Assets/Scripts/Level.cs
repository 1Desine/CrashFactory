#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;



public class Level : MonoBehaviour {
    public static Level Instrance { get; private set; }

    const string SAVES_PATH = "\\Levels\\";
    [SerializeField] private string fileName;


    [SerializeField] private VoxelsSO voxelsSO;
    [SerializeField] private Transform voxelsHolder;

    [SerializeField] private CarsSO carsSO;





    public Action OnDeleteVoxels;


    private Dictionary<Vector3, Voxel> voxelsDictionary;


    private void Awake() {
        Instrance = this;

        voxelsDictionary = new Dictionary<Vector3, Voxel>();
    }


    [Serializable]
    public class LevelInfo {
        // info


        // voxels
        public List<Vector3> voxelsPositions = new List<Vector3>();
        public List<int> voxelsIds = new List<int>();
        public List<Voxel.Type> voxelsTypes = new List<Voxel.Type>();

        // cars
        

    }


    private void SaveLevelToJson() {
        LevelInfo LevelInfo = new LevelInfo();

        // voxels
        foreach (var dic in voxelsDictionary) {
            LevelInfo.voxelsPositions.Add(dic.Value.transform.position);
            LevelInfo.voxelsIds.Add(dic.Value.id);
            LevelInfo.voxelsTypes.Add(dic.Value.type);
        }


        SaveSystem.WriteJson(SAVES_PATH + fileName, SaveSystem.SerializeJson(LevelInfo));
    }
    private void LoadLevelFromJson() {
        LevelInfo LevelInfo = SaveSystem.DeserializeJson<LevelInfo>(SaveSystem.ReadJson(SAVES_PATH + fileName));

        // voxels
        OnDeleteVoxels?.Invoke();
        voxelsDictionary.Clear();
        for (int i = 0; i < LevelInfo.voxelsIds.Count; i++) {
            TryAddVoxel(LevelInfo.voxelsPositions[i], LevelInfo.voxelsTypes[i]);
        }


    }

    public void TryAddVoxel(Vector3 position, Voxel.Type type) {
        voxelsDictionary.TryGetValue(position, out Voxel voxelDictionary);
        if (voxelDictionary != null) {
            return;
        }

        Voxel newVoxel = Instantiate(voxelsSO.GetVoxelPrefabByType(type), voxelsHolder).GetComponent<Voxel>();
        newVoxel.name = newVoxel.type.ToString();

        newVoxel.transform.position = position;
        newVoxel.id = voxelsDictionary.Count;
        newVoxel.type = type;

        voxelsDictionary.Add(position, newVoxel);
    }
    public void TryRemoveVoxel(Vector3 position) {
        voxelsDictionary.TryGetValue(position, out Voxel voxelDictionary);
        if (voxelDictionary == null) {
            return;
        }
        voxelDictionary.DestroySelf();
        voxelsDictionary.Remove(position);
    }




    public static List<Vector3> GetPathToPoint(Vector3 start, Vector3 end) {
        List<Vector3> path = new List<Vector3>();



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
