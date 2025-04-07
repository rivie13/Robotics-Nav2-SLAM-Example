using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombDefuserBot : MonoBehaviour
{
    public float moveSpeed = 5f;    // Speed at which the player moves
    public float rotateSpeed = 100f;

    public GameObject bomb;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Movement Input
        float moveDirection = 0f;
        if (Input.GetKey(KeyCode.W)) // Forward movement
        {
            moveDirection = 1f;
        }
        else if (Input.GetKey(KeyCode.S)) // Backward movement
        {
            moveDirection = -1f;
        }

        // Apply forward/backward movement
        transform.Translate(Vector3.forward * moveDirection * moveSpeed * Time.deltaTime);

        // Rotation Input
        float rotation = 0f;
        if (Input.GetKey(KeyCode.A)) // Rotate left
        {
            rotation = -1f;
        }
        else if (Input.GetKey(KeyCode.D)) // Rotate right
        {
            rotation = 1f;
        }

        // Apply rotation
        transform.Rotate(Vector3.up * rotation * rotateSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space)) // Rotate left
        {
            Defuse();
        }
    }

    void Defuse(){
        BombComponent component = bomb.GetComponent<BombComponent>();
        if(component != null){
            component.Defused();
        }
    }
}
