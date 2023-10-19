// REFERENCE https://youtu.be/CdPYlj5uZeI or https://www.youtube.com/watch?v=CdPYlj5uZeI&t=13s&ab_channel=ToyfulGames

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Car : MonoBehaviour {

    [SerializeField] private List<Transform> tireTransformList;

    [SerializeField] private List<Transform> tireToSteerTransformList;
    [SerializeField] private List<Transform> tireToApplyAccelerationForceTransformList;

    [SerializeField] private List<Transform> tireTransformVisualList;

    [Header("Suspention")]
    [SerializeField, Min(0)] private float springStrength = 400f;
    [SerializeField, Min(0)] private float springDamper = 5f;
    [SerializeField, Min(0)] private float springDistance = 0.12f;
    public float rearAxilOffset = -0.5f;


    [Header("Wheels")]
    [SerializeField, Min(0)] private float tireFriction = 500f;
    [SerializeField, Min(0)] private float tireRadiusVisual = 0.25f;
    [SerializeField, Min(0)] private float tireWidth = 0.1f;

    [Header("Engine")]
    [SerializeField] private AnimationCurve powerCurve;
    [SerializeField, Min(0)] private float carTopSpeed = 20f;
    [SerializeField, Min(0)] private float acceleration = 100f;
    [SerializeField, Min(0)] private float deceleration = 80f;
    [SerializeField, Min(0)] private float autoDecelerationForce = 20f;

    [Header("Steering")]
    [SerializeField] private AnimationCurve steerCurve;
    [SerializeField, Min(0)] private float maxSteeringAngle = 60f;
    [SerializeField, Min(0)] private float steerSpeed = 20f;
    private float currentWheelAngle = 0;

    private Rigidbody body;


    public Type type;
    public enum Type {
        Pickup,
        Van,
        TankTruck,
        RoadTrain1,
        RoadTrain3,
        RoadTrain5,
    }



    [SerializeField]
    public class carInfo {
        public Vector3 position;
        public Quaternion rotation;

        public Type type;



    }



    private void Awake() {
        body = GetComponent<Rigidbody>();
    }

    private void Update() {
        Agent_Steer(Vector3.SignedAngle(transform.forward, GetCurrentWayPoint() - transform.position + transform.forward * rearAxilOffset, Vector3.up));


        //HandleSteering();
        UpdateTireVisual();


        if (Input.GetKey(KeyCode.E)) {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            transform.position += Vector3.up * 0.01f;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate() {
        HandleSuspention();
        ApplySideGrip();
        HandleAccelerationAndDeceleration();
    }

    private void HandleSteering() {
        float steerInputNormalized = 0f;

        if (Input.GetKey(KeyCode.A)) {
            steerInputNormalized--;
        }
        if (Input.GetKey(KeyCode.D)) {
            steerInputNormalized++;
        }
        // straightning
        if (steerInputNormalized == 0) {
            currentWheelAngle -= currentWheelAngle / 2 * Time.deltaTime;
        }


        float carSpeed = Vector3.Dot(transform.forward, body.velocity);
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
        float desiredWheelAngle = steerCurve.Evaluate(normalizedSpeed) * steerInputNormalized * maxSteeringAngle;

        currentWheelAngle += desiredWheelAngle;
        currentWheelAngle = Mathf.Clamp(currentWheelAngle, -maxSteeringAngle, maxSteeringAngle);

        //// correction
        //float steeringCorrection = 0.9f;
        //float sideVelocity = Vector3.Dot(transform.right, body.velocity);
        //wheelsAngle += sideVelocity * steeringCorrection;

        foreach (Transform tireTransform in tireToSteerTransformList) {
            tireTransform.localEulerAngles = new Vector3(0, currentWheelAngle, 0);
        }
    }

    private void HandleAccelerationAndDeceleration() {
        float throttleInputNormalized = 0f;

        if (Input.GetKey(KeyCode.W)) {
            throttleInputNormalized++;
        }
        if (Input.GetKey(KeyCode.S)) {
            throttleInputNormalized--;
        }
        if (Input.GetKey(KeyCode.LeftShift)) {
            throttleInputNormalized *= 1.5f;
        }

        foreach (Transform tireTransform in tireToApplyAccelerationForceTransformList) {
            if (Physics.Raycast(tireTransform.position, -tireTransform.transform.up, out RaycastHit tireRay, springDistance)) {
                Vector3 forwardByNormal = Vector3.Dot(tireTransform.forward, Vector3.Cross(tireTransform.right, tireRay.normal).normalized) * Vector3.Cross(tireTransform.right, tireRay.normal).normalized;

                Vector3 tireForwardVelocity = Vector3.Dot(body.GetPointVelocity(tireTransform.position), forwardByNormal) * forwardByNormal;


                float forceToApply = 0;
                float directionOfForce = 0;
                // auto decelerating
                if (throttleInputNormalized == 0) {
                    forceToApply = autoDecelerationForce;
                    if (tireForwardVelocity.magnitude > 0.1f) directionOfForce = Vector3.Dot(forwardByNormal, tireForwardVelocity) > 0 ? -1 : 1;
                }
                else {
                    // acceletating or decelerating
                    float throtleToApply = acceleration * powerCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(Vector3.Dot(transform.forward, body.velocity)) / carTopSpeed));

                    forceToApply = Vector3.Dot(forwardByNormal * throttleInputNormalized, tireForwardVelocity) > 0 ? throtleToApply : deceleration;
                    directionOfForce = throttleInputNormalized;
                }


                body.AddForceAtPosition(forwardByNormal * directionOfForce * forceToApply / tireToApplyAccelerationForceTransformList.Count * Time.fixedDeltaTime, tireRay.point);
            }
        }
    }

    private void ApplySideGrip() {
        foreach (Transform tireTransform in tireTransformList) {
            if (Physics.Raycast(tireTransform.position, -tireTransform.transform.up, out RaycastHit tireRay, springDistance)) {
                Vector3 velocityRightComponent = Vector3.Dot(body.GetPointVelocity(tireTransform.position), Vector3.Cross(tireTransform.forward, tireRay.normal).normalized) * Vector3.Cross(tireTransform.forward, tireRay.normal).normalized;

                Vector3 sideVelocity = velocityRightComponent * body.mass / tireTransformList.Count * (springDistance - tireRay.distance);

                body.AddForceAtPosition(-sideVelocity * tireFriction * Time.fixedDeltaTime, tireRay.point);


                Debug.DrawRay(tireRay.point + tireTransform.up, sideVelocity * 10, Color.red);
                Debug.DrawRay(tireRay.point + tireTransform.up, Vector3.Cross(tireTransform.right, tireRay.normal) / 2, Color.blue);
            }
        }
    }

    private void HandleSuspention() {
        foreach (Transform tireTransform in tireTransformList) {
            if (Physics.Raycast(tireTransform.position, -tireTransform.transform.up, out RaycastHit tireRay, springDistance)) {
                Vector3 springDirection = tireTransform.up;
                Vector3 tireWorldVelocity = body.GetPointVelocity(tireTransform.position);

                float offset = springDistance - tireRay.distance;
                float springStrengthPerWheel = springStrength / tireTransformList.Count;
                float springForce = (offset * springStrengthPerWheel);

                float velocity = Vector3.Dot(springDirection, tireWorldVelocity);
                float damperForce = -Mathf.Min(velocity * springDamper, springForce);

                body.AddForceAtPosition(springDirection * (springForce + damperForce), tireRay.point);
            }
        }
    }

    private void UpdateTireVisual() {
        foreach (Transform tireTransformVisual in tireTransformVisualList) {
            Transform parentTire = tireTransformVisual.transform.parent;
            Physics.Raycast(tireTransformVisual.transform.parent.position, -parentTire.transform.up, out RaycastHit tireRay, springDistance);


            tireTransformVisual.position = parentTire.position - parentTire.up * tireRay.distance + parentTire.up * tireRadiusVisual / 2;

            if (tireRay.collider == null) {
                tireTransformVisual.position = parentTire.position - parentTire.up * springDistance + parentTire.up * tireRadiusVisual / 2;
            }

            tireTransformVisual.localScale = new Vector3(tireRadiusVisual, tireWidth, tireRadiusVisual);

            if (tireRay.collider != null)
                Debug.DrawRay(parentTire.position, parentTire.up * (springDistance - tireRay.distance), Color.black);
        }
    }





    [ExecuteInEditMode]
    private void OnDrawGizmos() {
        UpdateTireVisual();

        if (body == null) return;
        Gizmos.color = Color.green;

        Vector3 currentWayPoint = GetCurrentWayPoint();
        Gizmos.DrawSphere(new Vector3(currentWayPoint.x, 0, currentWayPoint.y), 0.3f);
    }


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




    private Vector3 GetCurrentWayPoint() {
        if (roadPath.Count == 0) return transform.forward;

        currentWayPoint = Mathf.Min(currentWayPoint, roadPath.Count - 1);

        int smoothing = 1 + (int)body.velocity.magnitude / 5;
        Vector3 pointToGo = Vector3.zero;
        for (int i = 0; i < smoothing; i++) {
            pointToGo += roadPath[Mathf.Min(currentWayPoint + i, roadPath.Count - 1)];
        }
        pointToGo /= smoothing;

        if ((pointToGo- transform.position).magnitude < 1f) {
            //Debug.Log("got here");
            if (roadPath.Count - 1 > currentWayPoint) currentWayPoint++;
            currentWayPoint %= roadPath.Count - 1;
        }

        return pointToGo;
    }


    public void Agent_Steer(float steeringAngle) {
        steeringAngle = Mathf.Clamp(steeringAngle, -maxSteeringAngle, maxSteeringAngle);

        float carSpeed = Vector3.Dot(transform.forward, body.velocity);
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
        float desiredWheelAngle = steerCurve.Evaluate(normalizedSpeed) * steeringAngle;


        //// correction
        //float steeringCorrection = 0.9f;
        //float sideVelocity = Vector3.Dot(transform.right, body.velocity);
        //wheelsAngle += sideVelocity * steeringCorrection;

        foreach (Transform tireTransform in tireToSteerTransformList) {
            currentWheelAngle = Mathf.Lerp(currentWheelAngle, desiredWheelAngle, steerSpeed * Time.deltaTime);
            tireTransform.localEulerAngles = new Vector3(0, currentWheelAngle, 0);
        }
    }

}
