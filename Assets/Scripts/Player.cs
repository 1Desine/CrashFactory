using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public static Player instance { get; private set; }

    [SerializeField] private Camera playerCamera;



    private void Awake() {
        instance= this;
    }


    public Camera GetPlayerCamera() {
        return instance.playerCamera;
    }

}
