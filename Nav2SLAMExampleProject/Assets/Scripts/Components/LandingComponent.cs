using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingComponent : MonoBehaviour
{
    private int[] slots;
    private int count;
    // Start is called before the first frame update
    void Start()
    {
        count = transform.childCount;
        slots = new int[count];
        for (int i = 0; i < count; i++)
        {
            slots[i] = 0;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void add(int i, GameObject person){
        person.transform.SetParent(transform);
        person.transform.position = transform.GetChild(i).transform.position;
        person.transform.rotation = transform.rotation;
        slots[i] = 1;
    }
    public int getFreeSlot(){
        for (int i = 0; i < count; i++)
        {
            if(slots[i] ==0){
                return i;
            }
        }
        return -1;
    }
 
}
