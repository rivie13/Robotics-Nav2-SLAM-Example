using UnityEngine;

public class Hose : MonoBehaviour
{
    public float rayDistance = 100f; 
    public Color rayColor = Color.red; 
    private Nozzle nozzle;

    void Start()
    {
        Transform noz = transform.Find("Nozzle");
        if (noz != null)
        {
            nozzle = noz.GetComponent<Nozzle>();
            if (nozzle == null) Debug.LogError("Nozzle component not found!");
        }
        else
        {
            Debug.LogError("Nozzle child not found in Hose!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && nozzle != null)
        {
            Debug.Log("Spacebar Pressed! Watering...");
            nozzle.Water();
        }
    }
}