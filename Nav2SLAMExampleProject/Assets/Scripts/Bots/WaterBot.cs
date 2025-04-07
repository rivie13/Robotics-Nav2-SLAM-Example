using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBot : MonoBehaviour
{
    public float moveSpeed = 5f;    // Speed at which the player moves
    public float rotateSpeed = 100f;
    public float detectionRadius = 5f; // Radius of the spherical area
    public Transform center;
    public Transform slots;
    private int[] slotsArr;
    private GameObject[] peopleArr;
    private int slotsCount;
    // Start is called before the first frame update
    void Start()
    {
        slotsCount = slots.childCount;

        // Initialize an array with the size equal to the number of children
        slotsArr = new int[slotsCount];
        peopleArr = new GameObject[slotsCount];

        // Loop through the children and assign them to the array
        for (int i = 0; i < slotsCount; i++)
        {
            slotsArr[i] = 0;
            peopleArr[i] = null;
        }
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
            collect();
            land();
        }
    }
    void collect()
    {
        // Get all colliders within the detection radius
        Collider[] collidersInRange = Physics.OverlapSphere(center.position, detectionRadius);

        // Check if any object is within the sphere
        if (collidersInRange.Length > 0)
        {
            foreach (Collider col in collidersInRange)
            {
                HumanComponent component = col.gameObject.GetComponent<HumanComponent>();
                if (component != null)
                {
                    if (component.getIsInWater())
                    {
                        for (int i = 0; i < slotsCount; i++)
                        {
                            if (slotsArr[i] == 0)
                            {
                                slotsArr[i] = 1;
                                peopleArr[i] = col.gameObject;
                                col.gameObject.transform.position = slots.GetChild(i).transform.position;
                                col.gameObject.transform.SetParent(transform);
                                component.setIsInWater(false);
                                break;
                            }
                        }
                    }
                }

            }
        }

    }

    void land()
    {
        // Get all colliders within the detection radius
        Collider[] collidersInRange = Physics.OverlapSphere(center.position, detectionRadius);

        // Check if any object is within the sphere
        if (collidersInRange.Length > 0)
        {
            foreach (Collider col in collidersInRange)
            {
                LandingComponent component = col.gameObject.GetComponent<LandingComponent>();
                if (component != null)
                {
                    int freeSlot = component.getFreeSlot();
                    while (freeSlot != -1 && hasPerson())
                    {
                        for (int i = 0; i < slotsCount; i++)
                        {
                            if (slotsArr[i] == 1)
                            {
                                component.add(freeSlot, peopleArr[i]);
                                slotsArr[i] = 0;
                                peopleArr[i] = null;
                                break;
                            }
                        }
                        freeSlot = component.getFreeSlot();
                    }
                }

            }
        }
    }

    // Visualize the detection radius in the editor (optional)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, detectionRadius);
    }
    bool hasPerson()
    {
        for (int i = 0; i < slotsCount; i++)
        {
            if (slotsArr[i] == 1)
            {
                return true;
            }
        }
        return false;
    }

}
