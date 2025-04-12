using System.Collections.Generic;
using UnityEngine;

// Base class for all inventory items
[System.Serializable]
public class InventoryItem : MonoBehaviour, IInteractable
{
    public string itemName;
    public Sprite itemIcon;
    public int stackSize = 1; // How many of this item can be in one slot
    public ThrowableObject inHandObject;

    private bool EUp = false;
    private bool EPressed = false;
    private bool EDown = false;

    public string InteractionPrompt => itemName;
    public bool buttonDown => EDown;
    public bool buttonPressed => EPressed;
    public bool buttonUp => EUp;

    public InventoryItem(string itemName, Sprite itemIcon, int stackSize, ThrowableObject inHandObject)
    {
        this.itemName = itemName;
        this.itemIcon = itemIcon;
        this.stackSize = stackSize;
        this.inHandObject = inHandObject;
    }

    public void Interact(GameObject interactor)
    {
        // ... (pickup logic)
        //Debug.Log(interactor.GetComponent<Inventory>().IsInventoryEmpty());
        if(interactor.GetComponent<Inventory>().AddItem(new Inventory.InventoryItem(itemName, itemIcon, stackSize, inHandObject)))
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        EDown = Input.GetKeyDown(KeyCode.E);
        EPressed = Input.GetKey(KeyCode.E);
        EUp = Input.GetKeyUp(KeyCode.E);
    }

    // Override this method for specific item actions (e.g., using an item)
    public virtual void Use()
    {
    }
}
