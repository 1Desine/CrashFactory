using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class VoxelsSO : ScriptableObject {

    public List<Voxel> voxelPrefabsList;


    public Transform GetVoxelPrefabByType(Voxel.Type type) {
        return voxelPrefabsList[(int)type].transform;
    }

}
