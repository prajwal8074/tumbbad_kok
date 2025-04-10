using UnityEngine;

public class DollBehaviour : MonoBehaviour, IInteractable
{
    private bool EUp = false;
    private bool EDown = false;

    public string InteractionPrompt => "Press E";
    public bool buttonDown => EDown;
    public bool buttonUp => EUp;

    private InventoryItem itemComponent;

    public void Interact(GameObject interactor)
    {
        // ... (pickup logic)
        //Debug.Log(interactor.GetComponent<Inventory>().IsInventoryEmpty());
        interactor.GetComponent<Inventory>().AddItem(new Inventory.InventoryItem(itemComponent.itemName, itemComponent.itemIcon, itemComponent.stackSize, itemComponent.inHandObject));
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        itemComponent = GetComponent<InventoryItem>();
    }

    // Update is called once per frame
    void Update()
    {
        EDown = Input.GetKey(KeyCode.E);
        EUp = Input.GetKeyUp(KeyCode.E);
    }
}
