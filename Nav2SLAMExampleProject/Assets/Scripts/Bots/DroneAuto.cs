using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAuto : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 initialPos;
    private GameObject supply;
    public GameObject supplyPrefab;
    private Rigidbody cubeRigidbody;

    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.position;
    }

    public void StartDelivery(Vector3 targetPos)
    {
        //spawn supply
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y - .5f, transform.position.z);
        supply = Instantiate(supplyPrefab, spawnPos, Quaternion.identity);
        supply.transform.SetParent(transform);
        cubeRigidbody = supply.AddComponent<Rigidbody>();
        cubeRigidbody.isKinematic = true;
        cubeRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        //starts the action sequence: move -> drop -> return
        //needed because moving is a coroutine
        StartCoroutine(ActionSequence(targetPos));
        

    }

    //slowly move to target position
    private IEnumerator MoveOverTime(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 currentPos = transform.position;
            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, (elapsed + Time.deltaTime) / duration);
            Vector3 direction = (nextPos - currentPos).normalized;

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);

            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }

    private void DropSupply()
    {
        supply.transform.parent = null;

        // disables kinematic, meaning enabling the supply to be affected by physics
        cubeRigidbody.isKinematic = false;
    }

    private IEnumerator ActionSequence(Vector3 targetPos)
    {
        //move to target
        float distance = Vector3.Distance(transform.position, targetPos);
        float duration = distance / speed;
        yield return StartCoroutine(MoveOverTime(targetPos + new Vector3(0,5,0), duration));

        //drop supply
        yield return new WaitForSeconds(0.5f);
        DropSupply();
        yield return new WaitForSeconds(0.5f);

        //return to initial position
        yield return StartCoroutine(MoveOverTime(initialPos, duration));

    }
}
