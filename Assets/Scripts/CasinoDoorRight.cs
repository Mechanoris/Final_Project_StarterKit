using UnityEngine;

public class CasinoDoorRight : MonoBehaviour
{

    //private GameObject doorHinge; // Reference to the door GameObject
    bool isOpen = false; // Track the state of the door
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isOpen = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Open the door
            Debug.Log("Door opened!");
            // You can add your door opening logic here, such as playing an animation or changing the door's state
            if (isOpen)
            {
                transform.parent.Rotate(90f, 0f, 0f); // Example: Rotate the door to open it
            }
            else
            {
                transform.parent.Rotate(-90f, 0f, 0f); // Example: Rotate the door to close it
            }

            isOpen = !isOpen;
        }

    }
}
