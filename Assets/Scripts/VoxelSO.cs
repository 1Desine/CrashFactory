using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class VoxelSO : ScriptableObject {

    public List<Transform> voxelPrefabsList;


    public Transform GetVoxelPrefabByType(Voxel.Type type) {
        return voxelPrefabsList[(int)type];
    }

}
