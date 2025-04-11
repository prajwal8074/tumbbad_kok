using UnityEngine;
using System.Collections.Generic;

public class ThrowableObject : MonoBehaviour
{
    public float throwForce = 10f;
    public float predictionTime = 2f;
    public int predictionSegments = 30;
    public float trajectoryLineWidth = 0.1f;
    public Material trajectoryLineMaterial;
    public GameObject ThrownObject;
    public GameObject InventoryHolder;

    public bool canThrow = false;

    private Rigidbody rb;
    private LineRenderer trajectoryRenderer;
    private bool isChargingThrow = false;
    private Vector3 throwDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("ThrowableObject requires a Rigidbody component.");
            enabled = false;
        }

        trajectoryRenderer = gameObject.AddComponent<LineRenderer>();
        trajectoryRenderer.enabled = false;
        trajectoryRenderer.material = trajectoryLineMaterial;
        trajectoryRenderer.widthMultiplier = trajectoryLineWidth;
        trajectoryRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if(canThrow)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartChargeThrow();
            }

            if (isChargingThrow)
            {
                UpdateThrowDirection();
                PredictTrajectory();
            }

            if (Input.GetKeyUp(KeyCode.E))
            {
                ReleaseThrow();
            }
        }else{
            isChargingThrow = false;
            trajectoryRenderer.enabled = false;
        }
    }

    void StartChargeThrow()
    {
        isChargingThrow = true;
        trajectoryRenderer.enabled = true;
        // Optionally disable gravity and kinematic on the rigidbody while charging
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void UpdateThrowDirection()
    {
        // You'll need to define how the throw direction is determined.
        // This could be based on the camera's forward direction,
        // the player's facing direction, or some other input.

        // Example: Throw in the camera's forward direction
        if (Camera.main != null)
        {
            throwDirection = Camera.main.transform.forward;
        }
        else
        {
            Debug.LogWarning("No Main Camera found. Throwing forward.");
            throwDirection = transform.forward;
        }

        // Optionally allow the player to influence the throw direction while charging
        // Example: Using mouse movement
        float horizontalInput = Input.GetAxis("Mouse X");
        float verticalInput = Input.GetAxis("Mouse Y");
        Vector3 mouseDelta = new Vector3(horizontalInput, verticalInput, 0f) * 0.1f; // Adjust sensitivity
        if (Camera.main != null)
        {
            Vector3 worldDelta = Camera.main.transform.TransformDirection(mouseDelta);
            // Project onto a plane (e.g., horizontal) if needed
            // worldDelta.y = 0f;
            throwDirection = (throwDirection + worldDelta).normalized;
        }
        else
        {
            throwDirection = (throwDirection + new Vector3(mouseDelta.x, mouseDelta.y, 0f)).normalized;
        }
    }

    void PredictTrajectory()
    {
        if (rb == null || trajectoryRenderer == null) return;

        List<Vector3> points = new List<Vector3>();
        Vector3 startPosition = transform.position;
        Vector3 startVelocity = throwDirection * throwForce;
        float timeStep = predictionTime / predictionSegments;

        for (int i = 0; i < predictionSegments + 1; i++)
        {
            float time = i * timeStep;
            Vector3 point = startPosition + startVelocity * time + 0.5f * Physics.gravity * time * time;
            points.Add(point);
        }

        trajectoryRenderer.positionCount = points.Count;
        trajectoryRenderer.SetPositions(points.ToArray());
    }

    void ReleaseThrow()
    {
        isChargingThrow = false;
        trajectoryRenderer.enabled = false;

        GameObject thrownObject = Instantiate(ThrownObject);
        InventoryHolder.GetComponent<Inventory>().RemoveItem(ThrownObject.GetComponent<InventoryItem>().itemName);
        InventoryHolder.GetComponent<PlayerMovement>().PositionArms(false);
        thrownObject.transform.position = transform.position;
        thrownObject.transform.rotation = transform.rotation;
        thrownObject.name = gameObject.name.Replace("InHand", "");
        thrownObject.SetActive(true);
        gameObject.SetActive(false);
        Rigidbody rb1 = thrownObject.GetComponent<Rigidbody>();
        if (rb1 != null)
        {
            rb1.isKinematic = false;
            rb1.useGravity = true;
            rb1.velocity = throwDirection * throwForce;
        }

        // Optionally detach the object from the player if it was held
        // transform.SetParent(null);
    }
}