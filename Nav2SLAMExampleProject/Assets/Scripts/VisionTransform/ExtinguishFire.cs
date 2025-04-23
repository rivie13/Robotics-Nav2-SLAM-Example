using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;
using UnityEngine.Assertions;
using RosMessageTypes.Sensor;

public class ExtinguishFire : MonoBehaviour
{
    private Transform nozzle_ref;
    public ParticleSystem waterJetPrefab;
    private ParticleSystem waterJetInstance;
    public float extinguishDistance = 1.0f;
    public float sprayDuration = 3.0f;
    private ROSConnection ros;
    private string extinguish_topic = "/extinguish_fire";
    private string yolo_topic_bbox = "/yolo/bbox";
    private Rect latest_bbox;
    public Camera targetCamera; // Assign in Unity Editor
    private bool isExtinguishing = false;
    private float sprayTimer = 0.0f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Vector3Msg>(extinguish_topic, ExtinguishCallback);
        ros.Subscribe<RegionOfInterestMsg>(yolo_topic_bbox, BoundingBoxCallback);

        Transform[] allTransforms = GetComponentsInChildren<Transform>();
        nozzle_ref = System.Array.Find(allTransforms, t => t.name == "Nozzle");
        Assert.IsNotNull(nozzle_ref, "Nozzle transform not found");
        Assert.IsNotNull(waterJetPrefab, "WaterJetPrefab not assigned");
        Assert.IsNotNull(targetCamera, "TargetCamera not assigned");

        waterJetInstance = Instantiate(waterJetPrefab, nozzle_ref.position, nozzle_ref.rotation);
        waterJetInstance.transform.SetParent(nozzle_ref);
        waterJetInstance.Stop();
        Debug.Log(nozzle_ref.name);
        Debug.Log(targetCamera.name);
        Debug.Log("YOLO BOUNDING BOX SUBSCRIBE UNITY = " +yolo_topic_bbox);
        Debug.Log("FIRE EXTINGUISH TOPIC SUBSCRIBE UNITY = " + extinguish_topic);


    }

    void BoundingBoxCallback(RegionOfInterestMsg msg)
    {
        latest_bbox = new Rect(msg.x_offset, msg.y_offset, msg.width, msg.height);
    }

    void ExtinguishCallback(Vector3Msg msg)
    {
        if (latest_bbox != null)
        {
            isExtinguishing = true;
            waterJetInstance.Play();
            sprayTimer = 0.0f;
            Debug.Log($"Extinguish request received for fire at x={msg.x}, y={msg.y}, z={msg.z}");
        }
    }

    void Update()
    {
        if (isExtinguishing)
        {
            waterJetInstance.transform.position = nozzle_ref.position;
            waterJetInstance.transform.rotation = nozzle_ref.rotation;

            sprayTimer += Time.deltaTime;

            if (latest_bbox != null)
            {
                float centerX = latest_bbox.x + latest_bbox.width / 2;
                float centerY = latest_bbox.y + latest_bbox.height / 2;
                Ray ray = targetCamera.ScreenPointToRay(new Vector3(centerX, centerY, 0));
                if (Physics.Raycast(ray, out RaycastHit hit, 200f))
                {
                    if (hit.collider.gameObject.CompareTag("Fire"))
                    {
                        float distance = Vector3.Distance(transform.position, hit.collider.gameObject.transform.position);
                        if (distance < extinguishDistance)
                        {
                            Debug.Log($"Extinguishing fire at {hit.point}");
                            Destroy(hit.collider.gameObject);
                            waterJetInstance.Stop();
                            isExtinguishing = false;
                        }
                    }
                }
            }

            if (sprayTimer >= sprayDuration)
            {
                waterJetInstance.Stop();
                isExtinguishing = false;
                Debug.Log("Spray duration exceeded, stopping water jet");
            }
        }
    }
}