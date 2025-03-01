using UnityEngine;

public class HastarBehaviour : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            animator.SetBool("isRunning", true);
        }
        else{
            animator.SetBool("isRunning", false);
        }
    }
}
