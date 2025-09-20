
using UnityEngine;

[System.Serializable]
public class TransformData
{
    public string id;
    public string item;         // Référence vers SimpleItemData.id (champ relation PocketBase)
    public string nom;

    // Position
    public float px;
    public float py;
    public float pz;

    // Rotation (angles Euler)
    public float rx;
    public float ry;
    public float rz;

    // Scale
    public float sx = 1f;
    public float sy = 1f;
    public float sz = 1f;

    public string created;
    public string updated;
}