using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Trapdoor : MonoBehaviour
{

    [Header("Trapdoor Settings")]
    public FirstPersonController firstPersonController;
    public float direction;
    public GameObject parent;
    public GameObject adjacentDoorParent;
    public HUDManager hud;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        parent.transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, 0), 9 * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var spawnPoint = firstPersonController.spawnPosition;
            spawnPoint = new Vector3(33,1.15f,-522);
            firstPersonController.spawnPosition = spawnPoint;
            rotateFast(direction);
            hud.DoScreenFlash(new Color(0f, 0f, 0f, 1f), 6f);
        }

    }

    void rotateFast(float direction)
    {
        parent.transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(direction * 90, 0, 0), 36000 * Time.deltaTime);
        //adjacentDoorParent.transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(direction * 90, 0, 0), 360 * Time.deltaTime);
    }

}