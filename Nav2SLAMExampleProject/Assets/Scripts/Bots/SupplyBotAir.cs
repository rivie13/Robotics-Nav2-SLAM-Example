using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyBotAir : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;
    public float detectionRadius = 5f;
    public bool hasSupply = true;
    private GameObject supply;
    public Transform hook;
    private Rigidbody cubeRigidbody;
    void Start()
    {
        if (hasSupply)
        {
            supply = GameObject.CreatePrimitive(PrimitiveType.Cube);

            supply.transform.position = hook.position - new Vector3(0, .5f, 0);
            supply.transform.SetParent(hook);
            cubeRigidbody = supply.AddComponent<Rigidbody>();
            cubeRigidbody.isKinematic = true;
            cubeRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            supply.transform.localScale = new Vector3(50f, 10f, 50f);

        }
    }

    void Update()
    {   
        float moveDirection = 0f;
        if (Input.GetKey(KeyCode.W)) // Forward 
        {
            moveDirection = 1f;
        }
        else if (Input.GetKey(KeyCode.S)) // Backward 
        {
            moveDirection = -1f;
        }

        transform.Translate(Vector3.forward * moveDirection * moveSpeed * Time.deltaTime);

        float vertDirection = 0f;
        if (Input.GetKey(KeyCode.Q)) // Up 
        {
            moveDirection = 1f;
        }
        else if (Input.GetKey(KeyCode.E)) // Down 
        {
            moveDirection = -1f;
        }

        transform.Translate(Vector3.up * vertDirection * moveSpeed * Time.deltaTime);

        float rotation = 0f;
        if (Input.GetKey(KeyCode.A)) // Rotate left
        {
            rotation = -1f;
        }
        else if (Input.GetKey(KeyCode.D)) // Rotate right
        {
            rotation = 1f;
        }

        transform.Rotate(Vector3.up * rotation * rotateSpeed * Time.deltaTime);

        //Activate function
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Drop();
        }
    }
    void Drop()
    {
        supply.transform.parent = null;

        // disables kinematic, meaning enabling the supply to be affected by physics
        cubeRigidbody.isKinematic = false;
    }
}
