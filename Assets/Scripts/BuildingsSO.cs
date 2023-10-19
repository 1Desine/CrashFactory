using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BuildingsSO : ScriptableObject {

    public List<Building> buildingsList;



    public Building GetBuildingByType(Building.Type type) {
        foreach (Building building in buildingsList) {
            if (building.type == type) return building;
        }
        Debug.LogError("me. No building found by type: " + type.ToString());
        return null;
    }



}
