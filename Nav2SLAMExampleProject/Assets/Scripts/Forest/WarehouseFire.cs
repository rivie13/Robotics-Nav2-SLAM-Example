using UnityEngine;

public class WarehouseFire : MonoBehaviour
{
    private ParticleSystem flamePrefab;
    private SphereCollider triggerCollider;

    private void Awake()
    {
        
        flamePrefab = GetComponent<ParticleSystem>();
        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = 3.5f;

        
        if (flamePrefab == null) Debug.LogError("Flame ParticleSystem not found!", this);
    }

    private void Update()
    {
        
        // Debug.Log($"Fire active on: {gameObject.name}");
    }

    public void ExtinguishFire()
    {
        Debug.Log("EXTINGUISHING FIRE!");
        if (flamePrefab != null)
        {
            flamePrefab.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            float flameLifetime = flamePrefab.main.startLifetime.constantMax;
            Destroy(gameObject, flameLifetime); 
        }
    }
}