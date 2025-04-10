using UnityEngine;

public class CoinBehaviour : MonoBehaviour, IInteractable
{
    private bool LMBUp = false;
    private bool LMBDown = false;

    public string InteractionPrompt => $"{gameObject.name}";
    public bool buttonDown => LMBDown;
    public bool buttonUp => LMBUp;

    public void Interact(GameObject interactor)
    {
        // ... (pickup logic)
        Destroy(gameObject);
    }

    void Start()
    {
    }

    void Update() // Check for containment every frame
    {
        LMBDown = Input.GetMouseButton(0);
        LMBUp = Input.GetMouseButtonUp(0);
    }

    public void SpawnItems(Transform spawnTransform, int itemCount, float throwForce)
    {
        for (int i = 0; i < itemCount; i++)
        {
            GameObject newItem = Instantiate(gameObject, spawnTransform.position, Quaternion.identity);
            newItem.name = gameObject.name;
            newItem.SetActive(true);
            Rigidbody rb = newItem.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Apply a random force
                Vector3 randomDirection = Random.insideUnitSphere;
                rb.AddForce(randomDirection * throwForce, ForceMode.Impulse);
            }
        }
    }
}