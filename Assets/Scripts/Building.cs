using UnityEngine;

public class Building : MonoBehaviour {


    [SerializeField] private Type type;
    public enum Type {
        Recycler,
        Mining,
        Processing,
        Production,
    }



}
