using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using Unity.Robotics.UrdfImporter.Control;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.Control
{
    public class AGVController : MonoBehaviour
    {
        public GameObject wheel1;
        public GameObject wheel2;

        private ArticulationBody wA1;
        private ArticulationBody wA2;
    
        public float maxLinearSpeed = 0.8f; //from 0.8
        public float wheelRadius = 0.033f;
        public float trackWidth = 0.288f;

        public float forceLimit = 10; // from 10
        public float damping = 10; // from 10
        public float navigationSpeed = 0.3f; // from 0.4
        public float navigationOffset = 0.15f;//from 1.0f


        private Transform nozzle_ref;
        private Nozzle nozzle_obj;

        private ROSConnection ros;
        private Queue<Vector3> firePositions = new Queue<Vector3>();
        private Vector3 currentGoal;
        private bool hasGoal = false;

        void Start()
        {
            wA1 = wheel1.GetComponent<ArticulationBody>();
            wA2 = wheel2.GetComponent<ArticulationBody>();
            if (wA1 == null || wA2 == null) Debug.LogError("Wheels missing ArticulationBody");

            SetParameters(wA1);
            SetParameters(wA2);
            nozzle_ref = GameObject.Find("Nozzle").transform;
            if (nozzle_ref == null) Debug.LogError("Nozzle not found");
            nozzle_obj = nozzle_ref.GetComponent<Nozzle>();
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<Vector3Msg>("/fire_location", FireLocationCallback);
            
        }

        //robots stops a small distance from the warehouse fire to avoid colliding w/ its sphere collider
        void FireLocationCallback(Vector3Msg msg)
        {
            Vector3 firePos = new Vector3((float)msg.x + navigationOffset, (float)msg.y, (float)msg.z + navigationOffset);
            firePositions.Enqueue(firePos);
 
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
            hasGoal = true;
            //I want to execute the code in FixedUpdate() again, since the robot does not TURN and head towards the next fire!
            Debug.Log($"Navigating to: {currentGoal}");
        }


       
        void FixedUpdate()
        {
            if (!hasGoal)
            {
                RobotInput(0f);
                return;
            }

            Vector3 direction = currentGoal - transform.position;
            direction.y = 0;
            float distance = direction.magnitude;

            if (distance > 0.075f)
            {
                Vector3 moveStep = direction.normalized * navigationSpeed * Time.fixedDeltaTime;
                transform.position += moveStep;
                RobotInput(navigationSpeed);
                Debug.Log($"Moving to: {currentGoal}, Distance: {distance}");
            }
            else
            {
                RobotInput(0f);
                Debug.Log("Reached goal, extinguishing");
                nozzle_obj.Water();
                Debug.Log("Finished Extinguishing!...");
                ProcessNextGoal(); // Move to next fire ideally. Currently robot extinguishes 1 fire(the same fire), then moves forward infinitely
            }
        }
        
     
        private void RobotInput(float speed)
        {
            if (speed > maxLinearSpeed) speed = maxLinearSpeed;
            float wheelRotation = (speed / wheelRadius) * Mathf.Rad2Deg;
            SetSpeed(wA1, wheelRotation);
            SetSpeed(wA2, wheelRotation);
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

      
    }
}