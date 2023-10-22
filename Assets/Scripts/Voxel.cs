using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour {



    private void Start() {
        Level.Instance.RegisterVoxel(this);

        Level.Instance.OnClearLevel += Map_OnDeleteVoxels;
    }

    private void Map_OnDeleteVoxels() {
        DestroySelf();
    }
    public void DestroySelf() {
        Level.Instance.OnClearLevel -= Map_OnDeleteVoxels;

        Destroy(gameObject);
    }
    



}
