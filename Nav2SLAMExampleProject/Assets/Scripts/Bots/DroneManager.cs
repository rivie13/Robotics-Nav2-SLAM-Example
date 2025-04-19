using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    // Start is called before the first frame update
    public int numDrones = 3;
    private int[] standbyDrones;
    private LandingComponent[] landings;
    void Start()
    {
        standbyDrones = new int[numDrones];
        for (int i = 0; i < numDrones; i++)
        {
            standbyDrones[i] = 1;
        }

        landings = GameObject.FindObjectsOfType<LandingComponent>();

        for (int i = 0; i < landings.Length; i++)  //find all landings in the world
        {
            if (landings[i] != null)  //if there is a landing
            {
                for (int j = 0; j < numDrones; j++) //find standby drone
                {
                    if(standbyDrones[j] == 1){
                        standbyDrones[j] = 0;
                        DroneAuto component = transform.GetChild(j).GetComponent<DroneAuto>();
                        if(component){
                            component.StartDelivery(landings[i].transform.position);  //start delivery
                        }
                        break;
                    }
                }
            }
            landings[i] = null;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
