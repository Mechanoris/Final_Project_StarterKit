using System;
using Unity.VisualScripting;
using UnityEngine;

public class Airplane : MonoBehaviour
{

    public int unit;
    public float speed;
    public bool Triggered = false;
    Vector3 originalPos;
    Vector3 lastPos;
    Vector3 currentPos;
    public Vector3 currentVelocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPos = transform.parent.position;
        lastPos = transform.parent.position;
    }

    // Update is called once per frame
    void Update()
    {
        moveForward();
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
            Triggered = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Triggered = false;
            transform.parent.position = originalPos;
            unit = 0;
        }

    }

    void moveForward()
    {
        if (!Triggered)
        {
            return;
        }
        if (unit <= 3500)
        {
            transform.parent.position -= Vector3.up * speed/100 * Time.deltaTime;
            transform.parent.position -= Vector3.right * speed * Time.deltaTime;
            unit++;
            //speed+=0.01f;
        }
    }
}
