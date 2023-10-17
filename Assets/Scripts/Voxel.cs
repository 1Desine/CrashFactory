using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour {
    [SerializeField] private GameObject PhysicalObject;

    public int id;
    public Type type;
    public enum Type {
        Solid,
        Road,
    }



    private void Awake() {

    }

    private void Start() {
        Map.Instrance.OnDeleteVoxels += Map_OnDeleteVoxels;
    }

    private void Map_OnDeleteVoxels() {
        DestroySelf();
    }
    public void DestroySelf() {
        Map.Instrance.OnDeleteVoxels -= Map_OnDeleteVoxels;

        Destroy(gameObject);
    }



}
