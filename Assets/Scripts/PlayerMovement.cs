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
    public GameObject sunLight;
    public float headBobFrequencyMax = 2f; // Adjust to control bobbing speed
    public float headBobAmplitude = 0.25f; // Adjust to control bobbing height
    public float idleBobFactor = 0.5f;
    public float armsRotationSpeed = 2f;
    public Camera playerCamera;
    public GameObject arms;
    public float minSpeed = 0f;
    public float maxSpeed = 5f;
    public float minPitch = 0.8f;
    public float maxPitch = 1.5f;

    private Vector3 lastPosition;
    private Vector3 actualVelocity = Vector3.zero;
    private AudioSource footstepAudio;

    private CharacterController controller;
    private Vector3 moveDirection;
    private float verticalVelocity;
    private Vector3 motionDirection;
    private Vector3 previousPosition;
    private float headBobTime;
    private Vector3 originalCameraPosition;
    private Vector3 originalArmsPosition;
    private float headBobFrequency;
    private GameObject leftArm;

    public float interactionDistance = 2f;
    public TextMeshProUGUI pickupText;
    private IInteractable currentInteractable;
    private MonoBehaviour currentOutlineScript;
    private Inventory inventory;
    private DisplayInventory displayInventory;
    [HideInInspector]
    public ThrowableObject currentItem;

    void Start()
    {
        sunLight.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        previousPosition = transform.position;
        motionDirection = Vector3.zero;
        pickupText.gameObject.SetActive(false);
        originalCameraPosition = playerCamera.transform.localPosition;
        originalArmsPosition = arms.transform.localPosition;
        footstepAudio = GetComponent<AudioSource>();
        lastPosition = transform.position;
        headBobFrequency = headBobFrequencyMax;
        inventory = GetComponent<Inventory>();
        displayInventory = GetComponent<DisplayInventory>();

        leftArm = FindDeepChild(gameObject, "LeftArm");

        PositionArms(false);
    }

    // Update is called once per frame
    void Update()
    {
        currentItem = displayInventory.currentItem;

        float mouseX = Input.GetAxis("Mouse X")*mouseSensitivity*Time.deltaTime;
        float mouseY = -Input.GetAxis("Mouse Y")*mouseSensitivity*Time.deltaTime;

        if(!Input.GetKey(KeyCode.Escape))
        {
            xRotation += mouseY;
            yRotation += mouseX;
        }

        xRotation = Mathf.Clamp(xRotation, -75f, 89f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Grounded check (important for gravity and jumping)
        bool isGrounded = controller.isGrounded;

        // Movement input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate move direction *relative to the sphere's surface*
        Vector3 moveInput = new Vector3(horizontalInput, 0, verticalInput);
        if (moveInput.magnitude > 0)
        {
            // Project forward and right vectors onto the horizontal plane
            Vector3 forwardDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 rightDirection = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

            // Calculate horizontal movement
            moveDirection = (forwardDirection * verticalInput + rightDirection * horizontalInput) * moveSpeed;
        }
        else
        {
            moveDirection = Vector3.zero; // No input, no movement
        }

        if (horizontalInput != 0 || verticalInput != 0)
        {
            ActivateFootstep();
            float speed = actualVelocity.magnitude;
            float speedFactor = (speed/maxSpeed);
            headBobFrequency = headBobFrequencyMax*speedFactor;
            float pitch = maxPitch*speedFactor;
            footstepAudio.pitch = pitch;
        }
        else
        {
            DeactivateFootstep();
            headBobFrequency = headBobFrequencyMax;
        }

        // Head Bobbing
        if (true)//isGrounded)
        {
            headBobTime += Time.deltaTime * headBobFrequency * (moveDirection.magnitude == 0? idleBobFactor : 1f);
            float verticalOffset = Mathf.Sin(headBobTime) * headBobAmplitude * (moveDirection.magnitude == 0? idleBobFactor : 1f);
            float armsOffset = Mathf.Sin(headBobTime-3.14f/6f) * headBobAmplitude/3f * (moveDirection.magnitude == 0? idleBobFactor : 1f);
            playerCamera.transform.localPosition = originalCameraPosition + Vector3.up * verticalOffset;
            arms.transform.localPosition = originalArmsPosition + Vector3.up * armsOffset;
        }
        else
        {
            //playerCamera.transform.localPosition = originalCameraPosition; // Reset position when not moving
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

        // Combine movement and gravity
        Vector3 finalMove = moveDirection + Vector3.up * verticalVelocity;

        // Move the character
        controller.Move(finalMove * Time.deltaTime);

        // Align player up vector with sphere normal
        Vector3 sphereNormal = (transform.position - transform.parent.position).normalized;
        Vector3 downDirection = (-sphereNormal + Vector3.down).normalized;

        if (moveDirection.magnitude == 0)
        {
            float slopeFactor = Vector3.Dot(motionDirection, Vector3.down);
            if(slopeFactor > Mathf.Cos(83f * Mathf.Deg2Rad))
                controller.Move(downDirection * slideForce * slopeFactor * Time.deltaTime);
        }

        // Interaction
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance) && hit.collider.GetComponent<IInteractable>() != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

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

            if (interactable.buttonDown)
            {
                PositionArms(true);
                if(currentItem != null)
                {
                    currentItem.gameObject.SetActive(false);
                }
            }

            if (interactable.buttonPressed)
            {
                float armsYRotation = arms.transform.localEulerAngles.y;
                if(armsYRotation < 60f)
                    arms.transform.localEulerAngles = new Vector3(0, armsYRotation+1f*armsRotationSpeed, 0);
            }

            if (interactable.buttonUp)
            {
                interactable.Interact(gameObject);
                currentInteractable = null;
                pickupText.gameObject.SetActive(false);
                if (currentOutlineScript != null)
                {
                    currentOutlineScript.enabled = false;
                    currentOutlineScript = null;
                }

                if(currentItem != null)
                {
                    currentItem.gameObject.SetActive(true);
                }
            }

            if(currentItem != null)
            {
                currentItem.canThrow = false;
            }
        }
        else
        {
            ClearInteractable();
            
            PositionArms(false);
            if(currentItem != null)
            {
                currentItem.canThrow = true;
            }
        }

        //keep last
        motionDirection = (transform.position - previousPosition).normalized;
        previousPosition = transform.position;
    }

    public void PositionArms(bool Interacting)
    {
        Vector3 leftArmRotation = leftArm.transform.localEulerAngles;
        if(currentItem != null && !Interacting)
        {
            leftArmRotation.x = 45f;
            leftArmRotation.z = -30f;
        }else{
            leftArmRotation.x = 23f;
            leftArmRotation.z = -4f;
        }
        leftArm.transform.localEulerAngles = leftArmRotation;
    }

    void FixedUpdate() // Use FixedUpdate for physics calculations
    {
        // Calculate the change in position
        Vector3 positionChange = transform.position - lastPosition;

        // Calculate the velocity
        actualVelocity = positionChange / Time.fixedDeltaTime;

        // Update the last position
        lastPosition = transform.position;
    }

    GameObject FindDeepChild(GameObject parent, string childName)
    {
        foreach (Transform childTransform in parent.transform)
        {
            GameObject child = childTransform.gameObject;
            if (child.name.ToLower() == childName.ToLower())
            {
                return child;
            }

            GameObject found = FindDeepChild(child, childName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    void ActivateFootstep()
    {
        if (!footstepAudio.enabled) // Avoid redundant activations
        {
            footstepAudio.enabled = true;
        }
    }

    void DeactivateFootstep()
    {
        if (footstepAudio.enabled) // Avoid redundant deactivations
        {
            footstepAudio.enabled = false;
        }
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
        arms.transform.localEulerAngles = new Vector3(0, 0, 0);
    }
}
