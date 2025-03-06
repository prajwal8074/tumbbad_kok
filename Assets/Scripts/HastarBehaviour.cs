using UnityEngine;
using System.Linq;

public class HastarBehaviour : MonoBehaviour
{
    public Transform sphereTransform;
    public float gravityStrength = 10f;
    public float maxSpeed = 5f;
    public float rotationSpeed = 0.4f;
    public float raycastDistance = 1f;
    public Transform playerTransform;
    public float minSpeed = 1f;
    public float targetSpeed = 10f;
    public float jumpForce = 5f;
    public float jumpAngle = 45f;
    public GameObject upperBound;
    public GameObject lowerBound;

    private Rigidbody rb;
    private Animator animator;
    private Transform targetObject;
    private Vector3 projectedDirection;
    private float stuckTimeElapsed = 0f;
    private bool isJumping = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        FindTargetObject();
    }

    void Update()
    {
        if (targetObject != null)
        {
            // 1. Get Rigidbody Velocity
            Vector3 velocity = rb.velocity;

            // 2. Calculate Speed
            float speed = velocity.magnitude;
            //Debug.Log($"speed: {speed}");

            Vector3 groundNormal = transform.up;
            Vector3 lookDirection = (targetObject.position - transform.position).normalized;
            // Project the Target Direction
            projectedDirection = Vector3.ProjectOnPlane(lookDirection, groundNormal).normalized;

            if (speed > (minSpeed + stuckTimeElapsed) && !isJumping)
            {
                // Rotate the Player
                Quaternion targetRotation = Quaternion.LookRotation(projectedDirection, transform.up);
                transform.rotation = targetRotation;
            }

            MoveForward();

            if(speed < targetSpeed && speed > minSpeed)
            {
                /*transform.Rotate(transform.right * -rotationSpeed * stuckTimeElapsed * Time.deltaTime);
                if(stuckTimeElapsed == 0f)
                    rb.AddForce(transform.up * jumpImpulse, ForceMode.Impulse);*/
                stuckTimeElapsed += Time.deltaTime;
            }else{
                stuckTimeElapsed = 0f;
            }
            //Debug.Log($"stuck time: {stuckTimeElapsed}");

            // 3. Map Speed to Animation Speed Multiplier
            float speedMultiplier = Mathf.Clamp01(speed / maxSpeed);

            // 4. Set Animator Speed Parameter
            animator.SetFloat("speedFactor", speedMultiplier);

            animator.SetFloat("moveSpeed", speed);
        }
        else
        {
            FindTargetObject(); // Try to find the target again if it's lost.
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(!isJumping)
        {
            if (other.gameObject == upperBound)
            {
                rb.angularVelocity = Vector3.zero;
                rb.AddForce((transform.up + transform.forward/2f) * jumpForce * 2f, ForceMode.Impulse);
                transform.Rotate(-jumpAngle, 0, 0, Space.Self);
                isJumping = true;
                Debug.Log("colUp");
            }else
            if (other.gameObject == lowerBound)
            {
                rb.angularVelocity = Vector3.zero;
                transform.Rotate(-jumpAngle, 0, 0, Space.Self);
                rb.AddForce((transform.forward) * jumpForce, ForceMode.Impulse);
                isJumping = true;
                Debug.Log("colDown");
            }
            Invoke("toggleJump", 3f);
        }
    }

    void FixedUpdate()
    {
        if(rb.velocity.magnitude > (minSpeed + stuckTimeElapsed))
        {
            ApplyCustomGravity();
            AlignToSphereSurface();
        }
    }

    void toggleJump()
    {
        isJumping = false;
    }

    void ApplyCustomGravity()
    {
        RaycastHit hit;
        Vector3 groundNormal = (sphereTransform.position - transform.position); // Default to up if no ground is hit

        if (Physics.Raycast(transform.position, -transform.up, out hit, raycastDistance))
        {
            if(hit.collider.gameObject.transform == sphereTransform)
            {
                // Get the normal from the hit
                groundNormal = hit.normal;
                //Debug.Log("hit");
            }
        }else{
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
            transform.rotation = targetRotation;
        }
        Vector3 gravityDirection = -groundNormal.normalized;
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);
    }

    void AlignToSphereSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, rb.velocity.normalized, out hit, raycastDistance))
        {
            if(hit.collider.gameObject.transform == sphereTransform)
            {
                //Debug.DrawRay(transform.position, rb.velocity.normalized * raycastDistance, Color.red);

                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 360));
            }
        }
    }

    void FindTargetObject()
    {
        targetObject = null;
        WheatObject[] wheatObjects = GameObject.FindObjectsOfType<WheatObject>();
        if (wheatObjects.Length > 0)
        {
            foreach (WheatObject wheatObject in wheatObjects)
            {
                if (true)//nearest wheat object logic
                {
                    targetObject = wheatObject.transform;
                }
            }
        }else{
            targetObject = playerTransform;
        }
        
        return; // Found the target, exit the loop
    }

    void MoveForward()
    {
        /*Vector3 moveDirection = (targetObject.position - transform.position).normalized;
        moveDirection = Vector3.ProjectOnPlane(moveDirection, (transform.position - sphereTransform.position).normalized);
        moveDirection = moveDirection.normalized;
        Debug.Log($"moveDirection: {moveDirection}");*/
        if(rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * (maxSpeed - rb.velocity.magnitude) * Time.deltaTime, ForceMode.VelocityChange);
        }
    }
}