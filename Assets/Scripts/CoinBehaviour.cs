using UnityEngine;

public class CoinBehaviour : MonoBehaviour, IInteractable
{
    private Transform sphereTransform;
    private Rigidbody rb;
    private MeshCollider sphereMeshCollider;
    private BoxCollider itemMeshCollider;
    private Vector3 fallingVelocity; // Store falling velocity
    private float tolerance = 0.001f; // Adjust this value

    private bool LMBUp = false;
    private bool LMBDown = false;

    public float vDamping = 0.95f;
    public float bounce = 0.5f;

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
        rb = GetComponent<Rigidbody>();
        sphereTransform = transform.parent.parent;
        sphereMeshCollider = sphereTransform.GetComponent<MeshCollider>();
        itemMeshCollider = GetComponent<BoxCollider>();
    }

    void Update() // Check for containment every frame
    {
        LMBDown = Input.GetMouseButton(0);
        LMBUp = Input.GetMouseButtonUp(0);

        Vector3 sphereCenter = sphereTransform.position;
        Vector3 direction = (transform.position - sphereCenter).normalized;

        if (IsOutsideSphere())
        {
            fallingVelocity = rb.velocity;
            ContainItem();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
    }

    void OnCollisionExit(Collision collision)
    {
    }

    bool IsOutsideSphere()
    {
        if (sphereMeshCollider == null || itemMeshCollider == null) return false;

        Bounds sphereBounds = sphereMeshCollider.bounds;
        Bounds itemBounds = itemMeshCollider.bounds;

        // Check if item's bounds are fully outside sphere's bounds.
        if (!sphereBounds.Contains(itemBounds.center))
        {
            return true;
        }

        // More detailed check: consider the extents of the item's bounds.
        Vector3 itemMin = itemBounds.min;
        Vector3 itemMax = itemBounds.max;

        // Added tolerance
        if (!sphereBounds.Contains(itemMin - Vector3.one * tolerance) ||
            !sphereBounds.Contains(itemMax + Vector3.one * tolerance))
        {
            return true;
        }

        return false;
    }

    void ContainItem()
    {
        Vector3 sphereCenter = sphereTransform.position;
        Vector3 direction = (sphereCenter - transform.position).normalized;

        // Apply force in opposite direction of falling velocity
        //rb.AddForce(-fallingVelocity, ForceMode.Force); // Impulse for immediate effect
        transform.position += -fallingVelocity * Time.deltaTime;
        rb.velocity = -fallingVelocity*bounce;

        // Damping
        rb.velocity *= vDamping; // Adjust 0.95f for damping strength
    }
}