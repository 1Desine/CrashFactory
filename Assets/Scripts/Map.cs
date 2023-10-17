using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Map;


public class Map : MonoBehaviour {
    public static Map Instrance { get; private set; }

    [SerializeField] private VoxelSO voxelSO;
    [SerializeField] private Transform voxelsHolder;

    const string SAVES_PATH = "\\Maps\\";
    [SerializeField] private string fileName;


    public Action OnDeleteVoxels;


    private Dictionary<Vector3, Voxel> voxelsDictionary;


    private void Awake() {
        Instrance = this;

        voxelsDictionary = new Dictionary<Vector3, Voxel>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            Transform a = Instantiate(voxelSO.GetVoxelPrefabByType(Voxel.Type.Solid), voxelsHolder);
            a.GetComponent<Voxel>().name = a.GetComponent<Voxel>().type.ToString();
        }
    }

    [Serializable]
    public class MapInfo {
        public List<Vector3> voxelsPositions = new List<Vector3>();
        public List<int> voxelsIds = new List<int>();
        public List<Voxel.Type> voxelsTypes = new List<Voxel.Type>();
    }


    private void SaveMap() {
        MapInfo mapInfo = new MapInfo();
        foreach (var dic in voxelsDictionary) {
            mapInfo.voxelsPositions.Add(dic.Value.transform.position);
            mapInfo.voxelsIds.Add(dic.Value.id);
            mapInfo.voxelsTypes.Add(dic.Value.type);
        }

        SaveSystem.WriteJson(SAVES_PATH + fileName, SaveSystem.SerializeJson(mapInfo));
    }
    private void LoadMap() {
        MapInfo mapInfo = SaveSystem.DeserializeJson<MapInfo>(SaveSystem.ReadJson(SAVES_PATH + fileName));

        OnDeleteVoxels?.Invoke();
        voxelsDictionary.Clear();
        for (int i = 0; i < mapInfo.voxelsIds.Count; i++) {
            TryAddVoxel(mapInfo.voxelsPositions[i], mapInfo.voxelsTypes[i]);
        }
    }

    public void TryAddVoxel(Vector3 position, Voxel.Type type) {
        voxelsDictionary.TryGetValue(position, out Voxel voxelDictionary);
        if (voxelDictionary != null) {
            return;
        }

        Voxel newVoxel = Instantiate(voxelSO.GetVoxelPrefabByType(type), voxelsHolder).GetComponent<Voxel>();
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
}
