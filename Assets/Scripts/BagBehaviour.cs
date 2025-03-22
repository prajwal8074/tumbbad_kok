using UnityEngine;

public class BagBehaviour : MonoBehaviour, IInteractable
{
    public GameObject itemPrefab; // Assign the item prefab in the Inspector
    public int itemCount = 10; // Number of items to spawn
    public float throwForce = 1f; // Force applied to items

    private bool LMBUp = false;
    private bool LMBDown = false;

    public string InteractionPrompt => $"{gameObject.name}";
    public bool buttonDown => LMBDown;
    public bool buttonUp => LMBUp;

    public void Interact(GameObject interactor)
    {
        // ... (pickup logic)
        itemPrefab.GetComponent<CoinBehaviour>().SpawnItems(transform, itemCount, throwForce);
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        //Interact(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        LMBDown = Input.GetMouseButton(0);
        LMBUp = Input.GetMouseButtonUp(0);
    }
}
