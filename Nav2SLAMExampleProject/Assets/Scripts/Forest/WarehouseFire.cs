using UnityEngine;
using UnityEngine.Assertions;

public class WarehouseFire : MonoBehaviour
{
    private ParticleSystem flamePrefab;
    private SphereCollider triggerCollider;

    private void Awake()
    {

        flamePrefab = GetComponent<ParticleSystem>();
        triggerCollider = gameObject.AddComponent<SphereCollider>();
        Assert.IsNotNull(triggerCollider);
        Assert.IsNotNull(flamePrefab);
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 3.5f;
        
    }

    public void ExtinguishFire()
    {
        Debug.Log("EXTINGUISHING " + this.gameObject.name.ToString());
        Destroy(this.gameObject);
    }
}