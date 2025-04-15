using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanComponent : MonoBehaviour
{
    public bool isInWater = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool getIsInWater(){
        return isInWater;
    }
    public void setIsInWater(bool flag){
        isInWater = flag;
    }
}
