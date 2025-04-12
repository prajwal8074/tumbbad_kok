using UnityEngine;

public class DollBehaviour : MonoBehaviour
{
	public GameObject Hastar;

    // Start is called before the first frame update
    void Start()
    {
    	Hastar.GetComponent<HastarBehaviour>().targetObject = null;
    }

    void OnEnable()
    {
        Hastar.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
