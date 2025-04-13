using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DisplayInventory : MonoBehaviour
{
    private Inventory playerInventory; // Assign the player's Inventory component
    public List<GameObject> inventorySlots;
    [HideInInspector]
    public int SelectedIndex = -1;
    [HideInInspector]
    public ThrowableObject currentItem;
    public Color OriginalColor = Color.white;
    public Color SelectedColor = Color.gray;

    void Start()
    {
        playerInventory = GetComponent<Inventory>();

        if (inventorySlots.Count == 0)
        {
            Debug.LogWarning("No inventory UI slots assigned in PopulateInventoryUI on " + gameObject.name);
            return;
        }
        UpdateInventoryDisplay();
        // Subscribe to inventory changes if needed.
        // playerInventory.OnInventoryChanged += UpdateInventoryDisplay;
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Q))
        {
            SelectNext();
        }
    }

    void SelectNext()
    {
        if(SelectedIndex != -1 && playerInventory.inventorySlots[SelectedIndex] != null)
        {
            Unselect();
        }
        SelectedIndex++;
        if(SelectedIndex >= inventorySlots.Count)
            SelectedIndex = 0;
        if(playerInventory.inventorySlots[SelectedIndex] != null)
        {
            Select();
        }else{
            for(int i=SelectedIndex;i<inventorySlots.Count;i++)
            {
                if(playerInventory.inventorySlots[i] != null)
                {
                    SelectedIndex = i;
                    break;
                }
            }
            if(playerInventory.inventorySlots[SelectedIndex] == null)
            {
                for(int i=0;i<SelectedIndex;i++)
                {
                    if(playerInventory.inventorySlots[i] != null)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
            }
            if(playerInventory.inventorySlots[SelectedIndex] != null)
            {
                Select();
            }
        }
    }

    public void Unselect()
    {
        inventorySlots[SelectedIndex].GetComponent<Image>().color = OriginalColor;
        playerInventory.inventorySlots[SelectedIndex].item.inHandObject.gameObject.SetActive(false);
        currentItem = null;
    }

    public void Select()
    {
        inventorySlots[SelectedIndex].GetComponent<Image>().color = SelectedColor;
        playerInventory.inventorySlots[SelectedIndex].item.inHandObject.gameObject.SetActive(true);
        currentItem = playerInventory.inventorySlots[SelectedIndex].item.inHandObject;
    }
    // void OnDisable()
    // {
    //     // playerInventory.OnInventoryChanged -= UpdateInventoryDisplay;
    // }

    public void UpdateInventoryDisplay()
    {
        List<Inventory.InventorySlot> inventory = playerInventory.GetInventory();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            Image slotImage = inventorySlots[i].GetComponent<Image>(); // This is the Image component of the Slot (Slot0, Slot1, etc.)
            GameObject itemImageObject = slotImage.transform.GetChild(0).gameObject; // Get the *GameObject* of the child, which is your item image.  Important!
            Image itemImage = itemImageObject.GetComponent<Image>(); //Get the Image component from the child game object.
            GameObject itemTextObject = slotImage.transform.GetChild(1).gameObject; // Get the *GameObject* of the child, which is your item image.  Important!
            TMP_Text itemText = itemTextObject.GetComponent<TMP_Text>();

            if (i < inventory.Count && inventory[i] != null && inventory[i].item != null)
            {
                Inventory.InventorySlot slotData = inventory[i];

                slotImage.gameObject.SetActive(true); // Make the slot Image visible.  No longer controlling the parent.

                if (itemImage != null)
                {
                    itemImage.sprite = slotData.item.itemIcon;  // Set the sprite of the *child* Image.
                }
                if (itemText != null)
                {
                    itemText.text = slotData.currentStack.ToString();  // Set the sprite of the *child* Image.
                }
            }
            else
            {
                slotImage.gameObject.SetActive(false); // Hide the slot Image.
                if (itemImage != null)
                {
                    itemImage.sprite = playerInventory.defaultItemIcon; // Clear the sprite
                }
                if (itemText != null)
                {
                    itemText.text = "0"; // Clear the sprite
                }
            }
        }
        if(SelectedIndex != -1)
        {
            if(playerInventory.inventorySlots[SelectedIndex] != null)
            {
                Select();
            }
        }
    }
}
