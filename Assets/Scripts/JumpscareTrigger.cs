using UnityEngine;

public class JumpscareTrigger : MonoBehaviour
{
    public GameObject jumpscarePrefab; // Assign the jumpscare prefab in the Inspector
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public FirstPersonController playerController; // Reference to the player's controller script
    //public GameObject jumpscarePlayer;
    public AudioSource jumpscareSound; // Reference to the AudioSource component for the jumpscare music
    void Start()
    {
        jumpscarePrefab.SetActive(false); // Ensure the jumpscare is initially inactive
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jumpscarePrefab.SetActive(true);
            playerController.jumpscaring = true; // Set the jumpscaring flag to true in the player's controller
            //jumpscareMusic.Play(); // Play the jumpscare music
            jumpscareSound.Play(); // Play the jumpscare music
        }

    }


}
