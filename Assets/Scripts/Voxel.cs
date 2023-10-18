using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour {


    private void Start() {
        Level.Instrance.OnDeleteVoxels += Map_OnDeleteVoxels;
    }

    private void Map_OnDeleteVoxels() {
        DestroySelf();
    }
    public void DestroySelf() {
        Level.Instrance.OnDeleteVoxels -= Map_OnDeleteVoxels;

        Destroy(gameObject);
    }




}
