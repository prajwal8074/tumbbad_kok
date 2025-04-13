using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int inventorySize = 5; // Number of inventory slots
    [HideInInspector]
    public List<InventorySlot> inventorySlots;
    public Sprite defaultItemIcon;

    private DisplayInventory displayInventory;

    public class InventorySlot
    {
        public InventoryItem item;
        public int currentStack = 0;

        public InventorySlot(InventoryItem item, int stack)
        {
            this.item = item;
            this.currentStack = stack;
        }
    }

    // Base class for all inventory items
    public class InventoryItem
    {
        public string itemName;
        public Sprite itemIcon;
        public int stackSize = 1; // How many of this item can be in one slot
        public ThrowableObject inHandObject;

        public InventoryItem(string itemName, Sprite itemIcon, int stackSize, ThrowableObject inHandObject)
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

    private void Start()
    {
        InitializeInventorySlots();
        displayInventory = GetComponent<DisplayInventory>();
        //displayInventory.UpdateInventoryDisplay();
    }

    private void InitializeInventorySlots()
    {
        inventorySlots = new List<InventorySlot>(new InventorySlot[inventorySize]);
    }

    public bool AddItem(InventoryItem itemToAdd)
    {
        bool wasInventoryEmpty = false;
        if(IsInventoryEmpty())
        {
            wasInventoryEmpty = true;
        }
        
        // Try to stack with existing items first
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] != null && inventorySlots[i].item != null && inventorySlots[i].item.itemName == itemToAdd.itemName && inventorySlots[i].currentStack < inventorySlots[i].item.stackSize)
            {
                /*int canStack = itemToAdd.stackSize; // We are adding one at a time here
                int needed = inventorySlots[i].item.stackSize - inventorySlots[i].currentStack;

                int toStack = Mathf.Min(canStack, needed);
                inventorySlots[i].currentStack += toStack;*/
                inventorySlots[i].currentStack++;
                displayInventory.UpdateInventoryDisplay();
                return true; // Assuming we are adding one at a time for simplicity
            }
        }

        // If no stackable slot is found, try to add to an empty slot
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] == null || inventorySlots[i].item == null)
            {
                if (itemToAdd.itemIcon == null)
                {
                    itemToAdd.itemIcon = defaultItemIcon;
                }
                inventorySlots[i] = new InventorySlot(itemToAdd, 1);
                displayInventory.UpdateInventoryDisplay();
                if(wasInventoryEmpty)
                {
                    displayInventory.SelectedIndex = 0;
                    displayInventory.Select();
                }
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    public bool RemoveItem(string itemName)
    {
        displayInventory.Unselect();
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] != null && inventorySlots[i].item != null && inventorySlots[i].item.itemName == itemName && inventorySlots[i].currentStack > 0)
            {
                inventorySlots[i].currentStack--;
                if (inventorySlots[i].currentStack <= 0)
                {
                    inventorySlots[i] = null; // Remove the empty slot
                    displayInventory.SelectedIndex = i;
                }
                displayInventory.UpdateInventoryDisplay();
                if(IsInventoryEmpty())
                    displayInventory.SelectedIndex = -1;
                Debug.Log(displayInventory.SelectedIndex);
                return true;
            }
        }
        Debug.Log(itemName + " not found in inventory.");
        return false;
    }

    /*public bool RemoveItem(string itemName, int amount)
    {
        displayInventory.Unselect();
        int removedCount = 0;
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] != null && inventorySlots[i].item != null && inventorySlots[i].item.itemName == itemName && inventorySlots[i].currentStack > 0)
            {
                int canRemove = Mathf.Min(amount - removedCount, inventorySlots[i].currentStack);
                inventorySlots[i].currentStack -= canRemove;
                removedCount += canRemove;

                if (inventorySlots[i].currentStack <= 0)
                {
                    inventorySlots[i] = null; // Remove the empty slot
                }

                displayInventory.UpdateInventoryDisplay();
                if(IsInventoryEmpty())
                    displayInventory.SelectedIndex = -1;

                if (removedCount >= amount)
                {
                    return true;
                }
            }
        }
        Debug.Log(amount + " of " + itemName + " not found in inventory.");
        return false;
    }*/

    public bool HasItem(string itemName)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot != null && slot.item != null && slot.item.itemName == itemName && slot.currentStack > 0)
            {
                return true;
            }
        }
        return false;
    }

    public int GetItemCount(string itemName)
    {
        int count = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot != null && slot.item != null && slot.item.itemName == itemName)
            {
                count += slot.currentStack;
            }
        }
        return count;
    }

    public void UseItem(string itemName)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i] != null && inventorySlots[i].item != null && inventorySlots[i].item.itemName == itemName && inventorySlots[i].currentStack > 0)
            {
                inventorySlots[i].item.Use();
                inventorySlots[i].currentStack--;
                if (inventorySlots[i].currentStack <= 0)
                {
                    inventorySlots[i] = null;
                }
                return;
            }
        }
        Debug.Log(itemName + " not found in inventory to use.");
    }

    public bool IsInventoryEmpty()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot != null && slot.item != null && slot.currentStack > 0)
            {
                return false; // Found at least one item, so the inventory is not empty
            }
        }
        return true; // No items found in any slot, so the inventory is empty
    }

    public List<InventorySlot> GetInventory()
    {
        return inventorySlots;
    }
}