using UnityEngine;

public class JumpscareSpin : MonoBehaviour
{

    public GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        transform.Rotate(Vector3.up * 36f * Time.deltaTime, Space.World);
        float scale = Mathf.Sin(Time.time * Mathf.PI * 0.1f) * 5f + 12f;
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
