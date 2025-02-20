using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private CharacterController controller;
    private Vector3 moveDirection;
    private float verticalVelocity;
    private Vector3 motionDirection;
    private Vector3 previousPosition;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        previousPosition = transform.position;
        motionDirection = Vector3.zero;
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

        //keep last
        motionDirection = (transform.position - previousPosition).normalized;
        previousPosition = transform.position;
    }
}
