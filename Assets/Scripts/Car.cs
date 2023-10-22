// REFERENCE https://youtu.be/CdPYlj5uZeI or https://www.youtube.com/watch?v=CdPYlj5uZeI&t=13s&ab_channel=ToyfulGames

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Car : MonoBehaviour {

    [SerializeField] private List<TireRayCast> tireRayCastList;


    [Header("Suspention")]
    [SerializeField, Min(0)] private float springStrength = 400f;
    [SerializeField, Min(0)] private float springDamper = 5f;
    [SerializeField, Min(0)] private float springDistance = 0.12f;


    [Header("Wheels")]
    [SerializeField, Min(0)] private float tireFriction = 500f;
    [SerializeField, Min(0)] private float tireFrictionLimit = 500f;
    [SerializeField, Min(0)] private float tireRadiusVisual = 0.25f;
    [SerializeField, Min(0)] private float tireWidth = 0.1f;

    [Header("Engine")]
    [SerializeField] private AnimationCurve powerCurve;
    [SerializeField, Min(0)] private float carTopSpeed = 20f;
    [SerializeField, Min(0)] private float acceleration = 100f;
    [SerializeField, Min(0)] private float deceleration = 80f;
    [SerializeField, Min(0)] private float autoDecelerationForceLimit = 20f;

    [Header("Steering")]
    [SerializeField] private AnimationCurve steerCurve;
    [SerializeField, Min(0)] private float maxSteeringAngle = 60f;
    [SerializeField, Min(0)] private float steerSpeed = 20f;
    private float currentWheelAngle = 0;

    private Rigidbody body;

    private int amountOfAcceleratingWheels;
    private int amountOfDeceleratingWheels;

    private float wheelBase;


    public Type type;
    public static List<Type> AllTypes {
        get {
            return new List<Type> {
                Type.Pickup,
                Type.Van,
                Type.TankTruck,
                Type.RoadTrain1,
                Type.RoadTrain3,
                Type.RoadTrain5,
        };
        }
    }
    public enum Type {
        Pickup,
        Van,
        TankTruck,
        RoadTrain1,
        RoadTrain3,
        RoadTrain5,
    }
    public List<Resource> availableCargoResourceToCarry;
    public int carryingCapacity;


    public bool isOnMission;
    private List<Vector3Int> roadPath = new List<Vector3Int>();
    private int currentWayPoint;
    public TaskMaganer.Task task;
    public Resource cargoResource;



    public class CarInfo {
        public Vector3 position;
        public Quaternion rotation;

        public Type type;
    }
    public CarInfo GetCarInfo() {
        CarInfo carInfo = new CarInfo {
            position = transform.position,
            rotation = transform.rotation,
            type = type,
        };

        return carInfo;
    }



    private void Awake() {
        body = GetComponent<Rigidbody>();

        wheelBase = 0;

        float zPositive = 0;
        float zNegative = 0;
        foreach (TireRayCast tire in tireRayCastList) {
            // count wheels that can accelerate and wheels that can decelerate
            if (tire.canAccelerate) amountOfAcceleratingWheels++;
            if (tire.canDecelerate) amountOfDeceleratingWheels++;
            // find wheel base
            if (Vector3.Dot(transform.forward, tire.transform.position - transform.position) > 0) zPositive = Vector3.Dot(transform.forward, tire.transform.position - transform.position);
            else zNegative = Vector3.Dot(transform.forward, tire.transform.position - transform.position);
        }
        wheelBase = zPositive - zNegative;
    }

    private void Start() {
        Level.Instance.RegisterCar(this);
        Level.Instance.OnClearLevel += DestroySelf;
    }

    private void Update() {
        AgentControl();
        //Steer(Input.GetAxisRaw("Horizontal") * maxSteeringAngle);
        //HandleAccelerationAndDeceleration(0);


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
    }

    public void DestroySelf() {
        Level.Instance.UnregisterCar(this);
        Level.Instance.OnClearLevel -= DestroySelf;

        Destroy(gameObject);
    }


    private void Steer(float desiredSteerAngle) {
        float carSpeed = Vector3.Dot(transform.forward, body.velocity);
        float speed01 = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);
        float evaluatedSteerSpeed = steerCurve.Evaluate(speed01) * steerSpeed * Time.deltaTime;

        currentWheelAngle += Mathf.Clamp(desiredSteerAngle - currentWheelAngle, -evaluatedSteerSpeed, evaluatedSteerSpeed);
        currentWheelAngle = Mathf.Clamp(currentWheelAngle, -maxSteeringAngle, maxSteeringAngle);

        //// correction
        //float steeringCorrection = 0.9f;
        //float sideVelocity = Vector3.Dot(transform.right, body.velocity);
        //wheelsAngle += sideVelocity * steeringCorrection;

        float desiredPivorDistance = wheelBase / Mathf.Tan(currentWheelAngle * Mathf.Deg2Rad);

        foreach (TireRayCast tire in tireRayCastList) {
            if (tire.canSteer == false) continue;

            tire.transform.localEulerAngles = Vector3.up * (Mathf.Atan(wheelBase / (-tire.sideOffset + desiredPivorDistance)) * Mathf.Rad2Deg);
        }
    }

    private void HandleAccelerationAndDeceleration(float throttleInput) {
        Debug.Log(throttleInput);

        if (Input.GetKey(KeyCode.W)) {
            throttleInput++;
        }
        if (Input.GetKey(KeyCode.S)) {
            throttleInput--;
        }
        if (Input.GetKey(KeyCode.LeftShift)) {
            throttleInput *= 1.5f;
        }

        foreach (TireRayCast tire in tireRayCastList) {
            if (Physics.Raycast(tire.transform.position, -tire.transform.up, out RaycastHit tireRay, springDistance)) {
                Vector3 forwardByNormal = Vector3.Dot(tire.transform.forward, Vector3.Cross(tire.transform.right, tireRay.normal).normalized) * Vector3.Cross(tire.transform.right, tireRay.normal).normalized;

                Vector3 tireForwardVelocity = Vector3.Dot(body.GetPointVelocity(tire.transform.position), forwardByNormal) * forwardByNormal;


                float forceToApply = 0;
                float directionOfForce = 0;
                // auto decelerating
                if (throttleInput == 0) {
                    if (tire.canDecelerate) {
                        forceToApply = Mathf.Min(tireForwardVelocity.magnitude * 1000, autoDecelerationForceLimit);
                    }
                    directionOfForce = Vector3.Dot(forwardByNormal, tireForwardVelocity) > 0 ? -1 : 1;
                }
                else {
                    // acceletating or decelerating
                    float throtleToApply = acceleration * powerCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(Vector3.Dot(transform.forward, body.velocity)) / carTopSpeed));

                    if (Vector3.Dot(forwardByNormal * throttleInput, tireForwardVelocity) > 0) {
                        if (tire.canAccelerate) forceToApply = throtleToApply / amountOfAcceleratingWheels;
                    }
                    else {
                        if (tire.canDecelerate) forceToApply = deceleration / amountOfDeceleratingWheels;
                    }
                    directionOfForce = throttleInput;
                }

                body.AddForceAtPosition(forwardByNormal * directionOfForce * forceToApply * Time.deltaTime, tireRay.point);
            }
        }
    }

    private void ApplySideGrip() {
        foreach (TireRayCast tire in tireRayCastList) {
            if (Physics.Raycast(tire.transform.position, -tire.transform.up, out RaycastHit tireRay, springDistance)) {
                Vector3 velocityRightComponent = Vector3.Dot(body.GetPointVelocity(tire.transform.position), Vector3.Cross(tire.transform.forward, tireRay.normal).normalized) * Vector3.Cross(tire.transform.forward, tireRay.normal).normalized;

                Vector3 sideVelocity = velocityRightComponent * body.mass / tireRayCastList.Count * (springDistance - tireRay.distance) * tireFriction;
                sideVelocity = Vector3.ClampMagnitude(sideVelocity, tireFrictionLimit);

                //Debug.Log(velocityRightComponent.magnitude);
                //if (velocityRightComponent.magnitude > 3) sideVelocity *= 0.3f;

                body.AddForceAtPosition(-sideVelocity * Time.fixedDeltaTime, tireRay.point);

                //Debug.DrawRay(tireRay.point + tire.transform.up, sideVelocity, Color.red);
                //Debug.DrawRay(tireRay.point + tire.transform.up, Vector3.Cross(tire.transform.right, tireRay.normal) / 2, Color.blue);
            }
        }
    }

    private void HandleSuspention() {
        foreach (TireRayCast tire in tireRayCastList) {
            if (Physics.Raycast(tire.transform.position, -tire.transform.up, out RaycastHit tireRay, springDistance)) {
                Vector3 springDirection = tire.transform.up;
                Vector3 tireWorldVelocity = body.GetPointVelocity(tire.transform.position);

                float offset = springDistance - tireRay.distance;
                float springStrengthPerWheel = springStrength / tireRayCastList.Count;
                float springForce = (offset * springStrengthPerWheel);

                float velocity = Vector3.Dot(springDirection, tireWorldVelocity);
                float damperForce = -Mathf.Min(velocity * springDamper, springForce);

                body.AddForceAtPosition(springDirection * (springForce + damperForce), tireRay.point);
            }
        }
    }

    private void UpdateTireVisual() {
        foreach (TireRayCast tire in tireRayCastList) {
            Physics.Raycast(tire.transform.parent.position, -tire.transform.up, out RaycastHit tireRay, springDistance);


            tire.TireVisualTransform.position = tire.transform.position - tire.transform.up * tireRay.distance + tire.transform.up * tireRadiusVisual / 2;

            if (tireRay.collider == null) {
                tire.TireVisualTransform.position = tire.transform.position - tire.transform.up * springDistance + tire.transform.up * tireRadiusVisual / 2;
            }

            tire.TireVisualTransform.localScale = new Vector3(tireRadiusVisual, tireWidth, tireRadiusVisual);

            if (tireRay.collider != null)
                Debug.DrawRay(tire.transform.position, tire.transform.up * (springDistance - tireRay.distance), Color.black);
        }
    }





    [ExecuteInEditMode]
    private void OnDrawGizmos() {
        UpdateTireVisual();

        if (body == null) return;
        Gizmos.color = Color.green;

        Vector3 currentWayPoint = GetCurrentWayPoint();
        Gizmos.DrawSphere(currentWayPoint, 0.3f);
    }



    public void SetTask(TaskMaganer.Task task) {
        Debug.Log("car: tast set");
        Debug.Log("amount of cargo: " + task.cargoResource.amount);
        roadPath = Level.Instance.GetPathByRoad(Vector3Int.up, new Vector3Int(5, 1, 5));
    }


    private void AgentControl() {
        Steer(AngleOffAroundAxis(transform.forward, GetCurrentWayPoint() - transform.position - transform.forward * wheelBase / 2, Vector3.up));

        float forwardForce = 0;
        if (roadPath.Count > 0) {
            if (body.velocity.magnitude < 1) forwardForce = 1;
            if (body.velocity.magnitude > 1) forwardForce = -1;
        }
        HandleAccelerationAndDeceleration(forwardForce);
    }

    private float AngleOffAroundAxis(Vector3 v, Vector3 forward, Vector3 axis) {
        Vector3 right = Vector3.Cross(forward, axis);
        forward = Vector3.Cross(axis, right);

        return Mathf.Atan2(Vector3.Dot(v, right), Vector3.Dot(v, forward)) * Mathf.Rad2Deg;
    }

    private Vector3 GetCurrentWayPoint() {
        if (roadPath.Count == 0) return transform.position + transform.forward;

        currentWayPoint = Mathf.Min(currentWayPoint, roadPath.Count - 1);

        int smoothing = 1 + (int)body.velocity.magnitude / 5;
        Vector3 pointToGo = Vector3.zero;
        for (int i = 0; i < smoothing; i++) {
            pointToGo += roadPath[Mathf.Min(currentWayPoint + i, roadPath.Count - 1)];
        }
        pointToGo /= smoothing;

        if ((pointToGo - transform.position).magnitude < 1f) {
            //Debug.Log("got here");
            if (currentWayPoint < roadPath.Count - 1) currentWayPoint++;
            else currentWayPoint = 0;
        }

        return pointToGo;
    }




}
