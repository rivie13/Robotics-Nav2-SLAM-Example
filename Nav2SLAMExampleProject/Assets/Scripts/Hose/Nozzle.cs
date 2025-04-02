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
        hose_ref = this.gameObject.GetComponentInParent<Hose>().transform;
        Assert.IsNotNull(hose_ref);
        Debug.Log("Hose Ref Parent Prefab = " + hose_ref.name.ToString());
    }
    
    public void Water()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = -transform.forward;

        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, rayColor, 100f);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            WarehouseFire target = hit.transform.GetComponent<WarehouseFire>();
            if (target != null)
            {
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