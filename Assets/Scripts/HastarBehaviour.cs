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
    public float jumpForce = 5f;
    public float jumpAngle = 45f;
    public GameObject upperBound;
    public GameObject lowerBound;
    public float playerHastarYDiff = 0.1f;
    public Transform centerTransform;
    public GameObject lastCol;
    public float groundToWallSpeedRatio = 1.5f;
    public GameObject WheatDoll;

    private Rigidbody rb;
    private Animator animator;
    [HideInInspector]
    public Transform targetObject;
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

            if (speed > (minSpeed + stuckTimeElapsed) && !isJumping)
            {
                lookTowards(targetObject.position - transform.position);
            }
            if((targetObject.position - transform.position).magnitude > 5f)
                MoveForward();
            else
                rb.velocity = Vector3.zero;

            /*if(speed < targetSpeed && speed > minSpeed)
            {
                /*transform.Rotate(transform.right * -rotationSpeed * stuckTimeElapsed * Time.deltaTime);
                if(stuckTimeElapsed == 0f)
                    rb.AddForce(transform.up * jumpImpulse, ForceMode.Impulse);
                //stuckTimeElapsed += Time.deltaTime;
            }else{
                stuckTimeElapsed = 0f;
            }*/
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
            if (other.gameObject == upperBound && (targetObject != playerTransform || playerTransform.position.y - playerHastarYDiff < transform.position.y))
            {
                lookTowards(Vector3.down);
                rb.angularVelocity = Vector3.zero;
                rb.velocity = Vector3.zero;
                rb.AddForce((transform.up + transform.forward/5f) * jumpForce * 2f, ForceMode.Impulse);
                lastCol = upperBound;
                //Debug.Log("colUp");
                toggleJump();
                Invoke("toggleJump", 3f);
            }else
            if (other.gameObject == lowerBound && (targetObject != playerTransform || playerTransform.position.y - playerHastarYDiff > transform.position.y))
            {
                if(targetObject == playerTransform)
                {
                    lookTowards(transform.position - centerTransform.position);
                    rb.angularVelocity = Vector3.zero;
                    rb.velocity = Vector3.zero;
                    transform.Rotate(Vector3.right * -jumpAngle);
                    rb.AddForce((transform.forward) * jumpForce, ForceMode.Impulse);
                    lastCol = lowerBound;
                    //Debug.Log("colDown");
                    toggleJump();
                    Invoke("toggleJump", 3f);
                }
            }
        }
    }

    void lookTowards(Vector3 lookDirection)
    {
        Vector3 groundNormal = transform.up;
        lookDirection = lookDirection.normalized;
        // Project the Target Direction
        projectedDirection = Vector3.ProjectOnPlane(lookDirection, groundNormal).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(projectedDirection, transform.up);
        transform.rotation = targetRotation;
    }

    void rotateJump()
    {
        transform.Rotate(Vector3.right * -jumpAngle);
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
        if(isJumping)
        {
            if(lastCol == upperBound)
            {
                minSpeed *= groundToWallSpeedRatio;
                maxSpeed *= groundToWallSpeedRatio;
            }
            else
            if(lastCol == lowerBound)
            {
                minSpeed *= 1/groundToWallSpeedRatio;
                maxSpeed *= 1/groundToWallSpeedRatio;
            }
        }

        isJumping = !isJumping;
    }

    void ApplyCustomGravity()
    {
        RaycastHit hit;
        Vector3 groundNormal = (centerTransform.position - transform.position); // Default to up if no ground is hit

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
        if (checkWheatDolls())
        {
            targetObject = FindNearestWheatDoll(transform.position).transform;
        }else{
            targetObject = playerTransform;
        }Debug.Log(targetObject.name);
        
        return; // Found the target, exit the loop
    }

    // Finds all active GameObjects with the exact name "WheatDoll"
    public static bool checkWheatDolls()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "WheatDoll")
            {
                return true;
            }
        }
        return false;
    }

    // Finds the nearest GameObject with the name "WheatDoll" to a given origin
    public static GameObject FindNearestWheatDoll(Vector3 origin)
    {
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPosition = origin;

        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "WheatDoll")
            {
                float distance = Vector3.Distance(currentPosition, obj.transform.position);
                if (distance < minDistance)
                {
                    nearest = obj;
                    minDistance = distance;
                }
            }
        }

        return nearest;
    }

    void MoveForward()
    {
        /*Vector3 moveDirection = (targetObject.position - transform.position).normalized;
        moveDirection = Vector3.ProjectOnPlane(moveDirection, (transform.position - centerTransform.position).normalized);
        moveDirection = moveDirection.normalized;
        Debug.Log($"moveDirection: {moveDirection}");*/
        if(rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.forward * (maxSpeed - rb.velocity.magnitude) * Time.deltaTime, ForceMode.VelocityChange);
        }
    }
}