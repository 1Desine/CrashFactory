using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class Building : MonoBehaviour {

    [SerializeField] private List<Transform> interactionZonePath;






    public int id;



    private void Update() {

        if (Input.GetKeyDown(KeyCode.T)) {
            TaskMaganer.Instance.CreateTask(id, -1, new Resource { amount = 100 }, Car.AllTypes);
        }
    }





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
        public int id;
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
        Level.Instance.RegisterBuilding(this);
        Level.Instance.OnClearLevel += DestroySelf;
    }

    public void DestroySelf() {
        Level.Instance.UnregisterBuilding(this);
        Level.Instance.OnClearLevel -= DestroySelf;

        Destroy(gameObject);
    }



}
