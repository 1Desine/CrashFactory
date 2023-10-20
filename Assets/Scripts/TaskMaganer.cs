using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TaskMaganer : MonoBehaviour {
    public static TaskMaganer Instance { get; private set; }



    private List<Task> tasksList = new List<Task>();



    float tickTime = 5;
    float tickCurrentTime;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        tickCurrentTime += Time.deltaTime;
        if (tickCurrentTime > tickTime) {
            tickCurrentTime %= tickTime;
            UpdateCarTasks();
        }
    }

    public class Task {
        public int buildingID_from;
        public int buildingID_to;
        public Resource cargoResource;
        public List<Car.Type> carTypesAllowed;
    }


    public void UpdateCarTasks() {
        List<Task> takenTasksList = new List<Task>();

        foreach (Task task in tasksList) {
            foreach (Car car in Level.Instance.GetCarsList()) {
                bool carAvailible = car.isOnMission == false;
                bool carTypeMatch = false;
                foreach (Car.Type carType in task.carTypesAllowed) {
                    if (car.type == carType) {
                        carTypeMatch = true;
                        break;
                    }
                }
                bool resourceTypeMatch = false;
                foreach (Resource carResurceAvailable in car.availableCargoResourceToCarry) {
                    if (task.cargoResource.type == carResurceAvailable.type) {
                        resourceTypeMatch = true;
                        break;
                    }
                }
                if (carAvailible && carTypeMatch && resourceTypeMatch) {
                    car.SetTask(task);
                    takenTasksList.Add(task);
                    continue;
                }
            }
        }

        foreach (Task task in takenTasksList) {
            tasksList.Remove(task);
        }

        Debug.Log("Updated car tasks");
    }

    public void CreateTask(int buildingID_from, int buildingID_to, Resource cargoResource, List<Car.Type> carTypesAllowed) {
        tasksList.Add(new Task {
            buildingID_from = buildingID_from,
            buildingID_to = buildingID_to,
            cargoResource = cargoResource,
            carTypesAllowed = carTypesAllowed,
        });
    }



}
