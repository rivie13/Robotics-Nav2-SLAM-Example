using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using System.Collections.Generic;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

public class MultiFireLocation : MonoBehaviour
{
    private ROSConnection ros;
    private string fire_pos_topic = "/{0}/fire_location";
    private string fire_count_topic = "/total_warehouse_fires";
    private Vector3Msg vec3msg;
    private List<GameObject> warehouse_fires;
    private static int total_warehouse_fires;

    private List<string> robotIds = new List<string>(3) { "robot1", "robot2", "robot3" };

    private float fireCheckInterval = 0.3f;
    private float lastFireCheckTime;

    private void Awake()
    {
        warehouse_fires = new List<GameObject>(GameObject.FindGameObjectsWithTag("Fire"));
        total_warehouse_fires = warehouse_fires.Count;
    }

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Int32Msg>(fire_count_topic);

        foreach (var robotId in robotIds)
        {
            ros.RegisterPublisher<Vector3Msg>(string.Format(fire_pos_topic, robotId));
        }

        Int32Msg countMsg = new Int32Msg { data = total_warehouse_fires };
        ros.Publish(fire_count_topic, countMsg);

        var positions = warehouse_fires.Select(f => f.transform.position).ToList();
        foreach (var pos in positions)
        {
            vec3msg = new Vector3Msg { x = pos.x, y = pos.y, z = pos.z };
            foreach (var robotId in robotIds)
            {
                ros.Publish(string.Format(fire_pos_topic, robotId), vec3msg);
            }
        }
    }
    // update fire list and publish remaining fire positions
    void Update()
    {
        //check fire every 0.3 seconds. if hasn't been 0.3 seconds do nothing. reduces cpu load
        if (Time.time - lastFireCheckTime < fireCheckInterval) return;
        lastFireCheckTime = Time.time;

        warehouse_fires.RemoveAll(fire => fire == null);
        if (warehouse_fires.Count == 0)
        {
            enabled = false; // disable updates when no fires remain
            return;
        }
        // publish remaining fire positions
        var positions = warehouse_fires.Select(f => f.transform.position).ToList();
        foreach (var pos in positions)
        {
            vec3msg = new Vector3Msg { x = pos.x, y = pos.y, z = pos.z };
            foreach (var robotId in robotIds)
            {
                ros.Publish(string.Format(fire_pos_topic, robotId), vec3msg);
            }
        }
    }
}