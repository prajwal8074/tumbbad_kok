using UnityEngine;

public class CoinBehaviour : MonoBehaviour
{
    public void SpawnItems(Transform spawnTransform, int itemCount, float throwForce)
    {
        for (int i = 0; i < itemCount; i++)
        {
            GameObject newItem = Instantiate(gameObject, spawnTransform.position, Quaternion.identity);
            newItem.name = gameObject.name.Replace("Reference", "");
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