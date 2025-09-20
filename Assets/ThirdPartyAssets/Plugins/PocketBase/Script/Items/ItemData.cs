using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/New items")]
public class ItemData : ScriptableObject
{
    [Header("PocketBase Data")]
    public string itemId; // Correspondra à l'ID PocketBase
    public string nom;
    public string description;
    public float prix;

    [Header("Unity Assets")]
    public Sprite visualObject; // Chargé depuis l'icon PocketBase
    public GameObject prefabObject; // Chargé depuis le FBX PocketBase

    [Header("Game Settings")]
    public int maxStackSize = 99;

    [Header("PocketBase URLs")]
    public string iconUrl;    // URL de l'icon sur PocketBase
    public string fbxUrl;     // URL du FBX sur PocketBase

    [Header("Transform Data")]

    public string transformId;

    public bool hasCustomTransform = false; 
}
