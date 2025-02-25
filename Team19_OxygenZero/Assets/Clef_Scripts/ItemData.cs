using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int amount = 1;
    public float cost = 1.2f;
    public float weight = 1.3f;
    public bool usable;
}
