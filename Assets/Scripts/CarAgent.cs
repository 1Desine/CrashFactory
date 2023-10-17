using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAgent : MonoBehaviour {


    private List<Vector3> roadPath = new List<Vector3> {
        new Vector3(0,0,0),
        new Vector3(0,0,1),
        new Vector3(0,0,2),
        new Vector3(0,0,3),
        new Vector3(0,0,4),

        new Vector3(1,0,4),
        new Vector3(2,0,4),
        new Vector3(3,0,4),
        new Vector3(4,0,4),

        new Vector3(5,0,5),
        new Vector3(5,0,6),

        new Vector3(6,0,6),
        new Vector3(7,0,6),
        new Vector3(7,0,7),
        new Vector3(8,0,7),
        new Vector3(9,0,7),
        new Vector3(10,0,7),

        new Vector3(10,0,4),
        new Vector3(10,0,0),
        new Vector3(4,0,0),
        new Vector3(0,0,0),
    };
    private int currentWayPoint;

    CarController carController;
    Rigidbody body;

    private void Awake() {
        carController = GetComponent<CarController>();
        body = GetComponent<Rigidbody>();
    }


    private void Update() {
        carController.Agent_Steer(Vector3.SignedAngle(transform.forward, nextWayPoint() - transform.position + transform.forward * carController.rearAxilOffset, Vector3.up));
    }



    private Vector3 nextWayPoint() {
        if (roadPath.Count == 0) return transform.forward;

        currentWayPoint = Mathf.Min(currentWayPoint, roadPath.Count - 1);

        int smoothing = 1 + (int)body.velocity.magnitude / 5;
        Vector3 pointToGo = Vector3.zero;
        for (int i = 0; i < smoothing; i++) {
            pointToGo += roadPath[Mathf.Min(currentWayPoint + i, roadPath.Count - 1)];
        }
        pointToGo /= smoothing;


        if ((pointToGo - transform.position).magnitude < 1f) {
            //Debug.Log("got here");
            if (roadPath.Count - 1 > currentWayPoint) currentWayPoint++;
            currentWayPoint %= roadPath.Count - 1;
        }


        return pointToGo;
    }







    [ExecuteInEditMode]
    private void OnDrawGizmos() {
        if (body == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(nextWayPoint(), 0.3f);
    }
}
