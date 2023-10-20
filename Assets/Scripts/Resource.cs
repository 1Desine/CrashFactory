using System;

[Serializable]
public class Resource{


    public Type type;
    public enum Type {
        Water,
        Oil,
        Chemicals,
        Rock,
        Ore,
        Coil,
    }

    public int amount;
    public float weightPerPoint;


}
