using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using Unity.Robotics.UrdfImporter.Control;
using System.Collections.Generic;

namespace RosSharp.Control
{
    public class AGVController : MonoBehaviour
    {
        public GameObject wheel1;
        public GameObject wheel2;

        private ArticulationBody wA1;
        private ArticulationBody wA2;

        public float maxLinearSpeed = 0.3f;
        public float wheelRadius = 0.033f;
        public float trackWidth = 0.288f;
        public float forceLimit = 10;
        public float damping = 10;
        public float navigationSpeed = 0.1f;
        public float navigationOffset = 0.35f; //changed from 1.0f. Robot way way too far visually from the warehouse fire

        private Transform nozzle_ref;
        private Nozzle nozzle_obj;

        private ROSConnection ros;
        private Queue<Vector3> firePositions = new Queue<Vector3>();
        private Vector3 currentGoal;
        private bool hasGoal = false;
        //private bool inFireTrigger = false;


        void Start()
        {
            wA1 = wheel1.GetComponent<ArticulationBody>();
            wA2 = wheel2.GetComponent<ArticulationBody>();
            if (wA1 == null || wA2 == null) Debug.LogError("Wheels missing ArticulationBody");

            SetParameters(wA1);
            SetParameters(wA2);
            nozzle_ref = GameObject.Find("Nozzle").transform;
            nozzle_obj = nozzle_ref.GetComponent<Nozzle>();
            if (nozzle_ref == null) Debug.LogError("Nozzle not found");
            if (nozzle_obj == null) Debug.LogError("Nozzle Object not found");

            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<Vector3Msg>("/fire_location", FireLocationCallback);
            Debug.Log("AGVController initialized");
        }

        void FireLocationCallback(Vector3Msg msg)
        {
            //robots stops a small distance from the warehouse fire to avoid colliding w/ its sphere collider 
            Vector3 firePos = new Vector3((float)msg.x + navigationOffset, (float)msg.y, (float)msg.z + navigationOffset);
            firePositions.Enqueue(firePos);
            Debug.Log($"Fire location: {firePos}");

            if (!hasGoal)
            {
                ProcessNextGoal();
            }


        }

        void ProcessNextGoal()
        {
            if (firePositions.Count == 0)
            {
                hasGoal = false;
                Debug.Log("No more fires");
                return;
            }
            currentGoal = firePositions.Dequeue();
            Debug.Log($"Next Goal Coordinates = ( {currentGoal.x}, {currentGoal.y}, {currentGoal.z}) ");
            hasGoal = true;
            //inFireTrigger = false;
            Debug.Log($"Navigating to: {currentGoal}");
        }

        void FixedUpdate()
        {
            while (firePositions.Count > 0)
            {
                if (hasGoal == false) //changed from !hasGoal
                {
                    RobotInput(0f);
                    return;
                }

                Vector3 direction = currentGoal - transform.position;
                direction.y = 0;
                float distance = direction.magnitude;

                if (distance > 0.1f)
                {
                    Vector3 moveStep = direction.normalized * navigationSpeed * Time.fixedDeltaTime;
                    transform.position += moveStep;
                    RobotInput(navigationSpeed);
                    //Debug.Log($"Moving to: {currentGoal}, Distance: {distance}");
                }
                else
                {
                    RobotInput(0f);
                    Debug.Log("Reached goal, extinguishing");
                    nozzle_obj.Water();
                    hasGoal = true;//testing this
                    //ProcessNextGoal();
                }
            }


        }


        private void SetParameters(ArticulationBody joint)
        {
            ArticulationDrive drive = joint.xDrive;
            drive.forceLimit = forceLimit;
            drive.damping = damping;
            joint.xDrive = drive;
        }

        private void SetSpeed(ArticulationBody joint, float wheelSpeed)
        {
            ArticulationDrive drive = joint.xDrive;
            drive.targetVelocity = wheelSpeed;
            joint.xDrive = drive;
        }

        private void RobotInput(float speed)
        {
            if (speed > maxLinearSpeed) speed = maxLinearSpeed;
            float wheelRotation = (speed / wheelRadius) * Mathf.Rad2Deg;
            SetSpeed(wA1, wheelRotation);
            SetSpeed(wA2, wheelRotation);
            //Debug.Log($"Speed: {speed}, Wheel rotation: {wheelRotation}");
        }
    }
}