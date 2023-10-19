using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

    [SerializeField] private List<Transform> interactionZonePath;


    public Type type;
    public enum Type {
        Recycler,
        Mining,
        Processing,
        Production,
    }



    public class BuildingInfo {

        public Vector3 position;
        public Quaternion rotation;

        public Type type;
    }

    public BuildingInfo GetBuildingInfo() {
        BuildingInfo carInfo = new BuildingInfo {
            position = transform.position,
            rotation = transform.rotation,
            type = type,
        };

        return carInfo;
    }



    private void Start() {
        Level.Instrance.RegisterBuilding(this);
        Level.Instrance.OnClearLevel += DestroySelf;
    }

    public void DestroySelf() {
        Level.Instrance.UnregisterBuilding(this);
        Level.Instrance.OnClearLevel -= DestroySelf;

        Destroy(gameObject);
    }



}
