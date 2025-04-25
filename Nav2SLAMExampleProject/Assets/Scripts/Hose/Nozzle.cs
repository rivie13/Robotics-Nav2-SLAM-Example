using UnityEngine;
using UnityEngine.Assertions;

public class Nozzle : MonoBehaviour
{
    
    public float rayDistance = 200f;
    public Color rayColor = Color.red;
    [SerializeField] private ParticleSystem waterPrefab;
    Transform hose_ref;

    private void Start()
    {
        hose_ref = this.gameObject.GetComponentInParent<Transform>();
        Assert.IsNotNull(hose_ref);
        //Debug.Log("Inside Nozzle Script. Hose Ref Parent Prefab = " + hose_ref.name.ToString());
        
    }
    
    public void Water()
    {
        
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = Quaternion.Euler(90, 0, 0) * transform.forward;

        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, rayColor, 50f);
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            WarehouseFire target = hit.transform.GetComponent<WarehouseFire>();
            if (target)
            {
                Debug.Log("Extinguishing " + target.name.ToString());
                Quaternion waterRotation = Quaternion.LookRotation(rayDirection);
                ParticleSystem waterEffects = Instantiate(waterPrefab, hose_ref.position, waterRotation);
                float waterLifetime = waterPrefab.main.startLifetime.constantMax;
                Destroy(waterEffects.gameObject, waterLifetime);
                target.ExtinguishFire();
            }
            else
            {
                Debug.Log($"Raycast hit: {hit.transform.name}");
            }
        }
    }
    
    

  



}