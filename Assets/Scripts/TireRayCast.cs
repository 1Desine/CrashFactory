using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TireRayCast : MonoBehaviour {

    public Transform TireVisualTransform;

    public float sideOffset;

    
    public bool canSteer;
    public bool canAccelerate;
    public bool canDecelerate;


    private void Awake() {
        sideOffset = Vector3.Dot(transform.right, transform.position - transform.parent.position);
    }



}
