using Codice.CM.Common;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using System.Collections.Generic;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;
using UnityEditor.WindowsStandalone;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class FireLocations : MonoBehaviour
{
    private ROSConnection ros;
    private string fire_pos_topic = "/fire_location";
    private string fire_count_topic = "/total_warehouse_fires";
    private Vector3Msg vec3msg;
    private List<GameObject> warehouse_fires;
    private static int total_warehouse_fires;

    private void Awake()
    {
        warehouse_fires = GameObject.FindGameObjectsWithTag("Fire").ToList();
        total_warehouse_fires = warehouse_fires.Count();
        Debug.Log($"Published total fires: {total_warehouse_fires}");
    }

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Vector3Msg>(fire_pos_topic);
        ros.RegisterPublisher<Int32Msg>(fire_count_topic); // Register publisher for Int32Msg

        Int32Msg countMsg = new Int32Msg
        {
            data = total_warehouse_fires
        };
        ros.Publish(fire_count_topic, countMsg);

        var positions = warehouse_fires.Select(f => f.transform.position).ToList();
        foreach (var pos in positions)
        {
            vec3msg = new Vector3Msg { x = pos.x, y = pos.y, z = pos.z };
            ros.Publish(fire_pos_topic, vec3msg);
            Debug.Log($"Publishing fire pos: {vec3msg.x}, {vec3msg.y},{vec3msg.z}");
        }
    }
}