using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float mouseSensitivity = 500f;
    float xRotation = 0f;
    float yRotation = 0f;

    public float moveSpeed = 5f;
    public float gravity = 10f;
    public float jumpForce = 8f;
    public float slideForce = 2f;
    public float slideFriction = 0.95f; // Adjust this for "slipperiness"
    public Light sunLight;
    public float headBobFrequency = 2f; // Adjust to control bobbing speed
    public float headBobAmplitude = 0.25f; // Adjust to control bobbing height
    public Transform camera;

    private CharacterController controller;
    private Vector3 moveDirection;
    private float verticalVelocity;
    private Vector3 motionDirection;
    private Vector3 previousPosition;
    private float headBobTime;
    private Vector3 originalCameraPosition;

    public float interactionDistance = 2f;
    public TextMeshProUGUI pickupText;
    private IInteractable currentInteractable;
    private MonoBehaviour currentOutlineScript;

    void Start()
    {
        sunLight.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        previousPosition = transform.position;
        motionDirection = Vector3.zero;
        pickupText.gameObject.SetActive(false);
        originalCameraPosition = camera.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X")*mouseSensitivity*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y")*mouseSensitivity*Time.deltaTime;

        xRotation += mouseY;
        yRotation += mouseX;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Grounded check (important for gravity and jumping)
        bool isGrounded = controller.isGrounded;

        // Movement input
        float horizontalInput = -Input.GetAxis("Horizontal");
        float verticalInput = -Input.GetAxis("Vertical");

        // Calculate move direction *relative to the sphere's surface*
        Vector3 moveInput = new Vector3(horizontalInput, 0, verticalInput);
        if (moveInput.magnitude > 0)
        {
            moveDirection = transform.TransformDirection(moveInput) * moveSpeed;
        }
        else
        {
            moveDirection = Vector3.zero; // No input, no movement
        }

        // Head Bobbing
        if (isGrounded)
        {
            headBobTime += Time.deltaTime * headBobFrequency;
            float verticalOffset = Mathf.Sin(headBobTime) * headBobAmplitude;
            camera.localPosition = originalCameraPosition + Vector3.up * verticalOffset;
        }
        else
        {
            //camera.localPosition = originalCameraPosition; // Reset position when not moving
        }

        // Apply gravity
        if (!isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        else
        {
            verticalVelocity = 0; // Reset on ground
        }

        // Sliding (Slipperiness)
        if (!isGrounded && moveDirection.magnitude > 0)
        {
            //moveDirection *= slideFriction; // Reduce velocity for sliding
        }

        // Combine movement and gravity
        Vector3 finalMove = moveDirection + Vector3.up * verticalVelocity;

        // Move the character
        controller.Move(finalMove * Time.deltaTime);

        // Align player up vector with sphere normal
        if (isGrounded)
        {
            Vector3 sphereNormal = (transform.position - transform.parent.position).normalized;
            Vector3 downDirection = (-sphereNormal + Vector3.down).normalized;

            if (moveDirection.magnitude == 0)
            {
                controller.Move(downDirection * slideForce * Vector3.Dot(motionDirection, Vector3.down) * Time.deltaTime);
            }
        }

        // Interaction
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                currentInteractable = interactable;
                pickupText.text = interactable.InteractionPrompt;
                pickupText.gameObject.SetActive(true);

                // Outline Logic
                MonoBehaviour outlineScript = hit.collider.GetComponent<Outline>(); // Getting the script
                // Check if the script exists and if it derives from the outline script
                if (outlineScript != null)
                {
                    currentOutlineScript = outlineScript;
                    currentOutlineScript.enabled = true; // Enable the outline script
                }

                if (interactable.buttonClicked)
                {
                    interactable.Interact(gameObject);
                    currentInteractable = null;
                    pickupText.gameObject.SetActive(false);
                    if (currentOutlineScript != null)
                    {
                        currentOutlineScript.enabled = false;
                        currentOutlineScript = null;
                    }
                }
            }
            else
            {
                ClearInteractable();
            }
        }
        else
        {
            ClearInteractable();
        }

        //keep last
        motionDirection = (transform.position - previousPosition).normalized;
        previousPosition = transform.position;
    }

    private void ClearInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable = null;
            pickupText.gameObject.SetActive(false);
            if (currentOutlineScript != null)
            {
                currentOutlineScript.enabled = false;
                currentOutlineScript = null;
            }
        }
    }
}
