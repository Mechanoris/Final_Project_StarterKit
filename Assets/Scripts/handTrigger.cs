using Unity.VisualScripting;
using UnityEngine;

public class handTrigger : MonoBehaviour
{
    public GameObject jumpscarePrefab; // Assign the jumpscare prefab in the Inspector

    private int unit = 0; // Counter for the number of times the hand has moved
    public float speed;
    public bool Triggered = false;
    Vector3 originalPos;
    Vector3 lastPos;
    Vector3 currentPos;
    public Vector3 currentVelocity;

    public FirstPersonController playerController; // Reference to the player's controller script
    public bool isMoving = false; // Flag to indicate if the hand is currently moving
    void Start()
    {
        originalPos = transform.parent.position;
        lastPos = transform.parent.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Triggered)
        {
            moveForward();
            ResetFallingRespawn();
        }
    }

    private void FixedUpdate()
    {
        if (Triggered)
        {
            currentPos = transform.parent.position;
            currentVelocity = (currentPos - lastPos) / Time.fixedDeltaTime;
            lastPos = currentPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jumpscarePrefab.SetActive(false);
            Triggered = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Triggered = false;
            transform.parent.position = originalPos; // Reset the hand's position to the original position
            unit = 0; // Reset the unit counter
        }
    }

    void moveForward()
    {
        if (!Triggered)
        {
            return;
        }
        unit++;
        if (unit >= 2000 && unit <= 9000)
        {
            transform.parent.position += new Vector3(-0.03f, 0f, 0f); // Move the hand forward by 5 units
        }
    }

    void ResetFallingRespawn()
    {
        if (unit >= 10)
        {
            playerController.jumpscaring = false; // Reset the jumpscaring flag in the player's controller
        }
    }
}
