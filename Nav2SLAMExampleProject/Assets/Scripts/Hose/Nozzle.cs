using UnityEngine;

public class Nozzle : MonoBehaviour
{
    public float rayDistance = 1f;
    public Color rayColor = Color.red;
    [SerializeField] private ParticleSystem waterPrefab;

    public void Water(Vector3 firePosition)
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        // Calculate direction from nozzle to fire position
        Vector3 rayDirection = (firePosition - rayOrigin).normalized;

        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, rayColor, 50f);
        int fireLayerMask = LayerMask.GetMask("FireLayer");
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, fireLayerMask))
        {
            Debug.Log($"Raycast hit: {hit.transform.name} at {hit.point}, distance: {hit.distance}");
            WarehouseFire target = hit.transform.GetComponent<WarehouseFire>();
            if (target)
            {
                Debug.Log($"Extinguishing {target.name}");
                ParticleSystem waterEffects = Instantiate(waterPrefab, transform.position, Quaternion.LookRotation(rayDirection));
                float waterLifetime = waterPrefab.main.startLifetime.constantMax;
                Destroy(waterEffects.gameObject, waterLifetime);
                target.ExtinguishFire();
            }
            else
            {
                Debug.Log("Hit something but not WarehouseFire");
            }
        }
        else
        {
            Debug.Log($"Raycast missed. Origin: {rayOrigin}, Fire: {firePosition}, Direction: {rayDirection}");
        }
    }
}