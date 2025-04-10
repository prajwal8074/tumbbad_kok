using System.Collections.Generic;
using UnityEngine;

// Base class for all inventory items
[System.Serializable]
public class InventoryItem : MonoBehaviour
{
    public string itemName;
    public Sprite itemIcon;
    public int stackSize = 1; // How many of this item can be in one slot
    public GameObject inHandObject;

    public InventoryItem(string itemName, Sprite itemIcon, int stackSize, GameObject inHandObject)
    {
        this.itemName = itemName;
        this.itemIcon = itemIcon;
        this.stackSize = stackSize;
        this.inHandObject = inHandObject;
    }

    // Override this method for specific item actions (e.g., using an item)
    public virtual void Use()
    {
    }
}
