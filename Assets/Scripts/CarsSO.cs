using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CarsSO : ScriptableObject {

    public List<Car> carsList;



    public Car GetCarByType(Car.Type type) {
        foreach (Car car in carsList) {
            if (car.type == type) return car;
        }
        Debug.LogError("me. No car found by type: " + type.ToString());
        return null;
    }


}
