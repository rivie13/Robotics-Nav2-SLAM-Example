using Codice.CM.Common;
using RosMessageTypes.Geometry;
using System.Collections.Generic;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;
using UnityEditor.WindowsStandalone;
using UnityEngine;

public class FireLocations : MonoBehaviour
{
    private ROSConnection ros;
    private string fire_pos_topic = "/fire_location";
    private Vector3Msg vec3msg;
    private List<GameObject> warehouse_fires;
    private static int count_warehousefires;

    private void Awake()
    {
        warehouse_fires = GameObject.FindGameObjectsWithTag("Fire").ToList();
    }

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Vector3Msg>(fire_pos_topic);

        var positions = warehouse_fires.Select(f => f.transform.position).ToList();
        foreach (var pos in positions)
        {
            vec3msg = new Vector3Msg { x = pos.x, y = pos.y, z = pos.z };
            count_warehousefires++;
            Debug.Log("Total fires = " + count_warehousefires);
            ros.Publish(fire_pos_topic, vec3msg);
            Debug.Log($"Publishing fire pos: {vec3msg.x}, {vec3msg.y},{vec3msg.z}");
            Debug.Log("Publishing fire positons to topic names = " + fire_pos_topic);
        }
    }
}