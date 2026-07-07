using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    public GameObject enemyPrefab; // Reference to the enemy prefab
    //public GameObject TriggerEntity; // Reference to the trigger entity that will spawn the enemy
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyPrefab.SetActive(true);
            Instantiate(enemyPrefab, transform.position, transform.rotation);
        }
    }
}
