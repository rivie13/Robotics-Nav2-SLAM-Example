using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.UrdfImporter.Control;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RosSharp.Control
{
    public class ROSAGVControl : MonoBehaviour
    {
        public GameObject wheel1;
        public GameObject wheel2;

        private ArticulationBody wA1;
        private ArticulationBody wA2;

        public float maxLinearSpeed = 0.8f;
        public float wheelRadius = 0.033f;
        public float trackWidth = 0.288f;
        private float maxRotationalSpeed = 1.25f;

        private float forceLimit = 10f;
        private float damping = 10f;
        private float navigationSpeed = 0.35f;
        private float navigationOffset = 0.15f;
        public Transform bf;
        [SerializeField] private Camera vision_transform_camera;

        private ROSConnection ros;
        private Queue<Vector3> firePositions = new Queue<Vector3>();
        private Vector3 currentGoal;
        private Vector3 actualFirePosition;
        private bool hasGoal = false;
        private bool stopMovement = false;

        private float avoidDistance = 8f;
        private float repelForce = 6f;
        private bool isAvoiding = false;
        private Vector3 avoidanceDirection;

        private float rayDistance = 2.75f;
        public Color rayColor = Color.red;
        [SerializeField] private ParticleSystem waterPrefab;

        private bool yoloFireDetected = false;


        private float lastCmdReceived = 0f;
        private float rosLinear = 0f;
        private float rosAngular = 0f;
        public float ROSTimeout = 0.5f;
        private RotationDirection directionros;
        void Start()
        {
            wA1 = wheel1.GetComponent<ArticulationBody>();
            wA2 = wheel2.GetComponent<ArticulationBody>();
            if (wA1 == null || wA2 == null) Debug.LogError("Wheels missing ArticulationBody");

            SetParameters(wA1);
            SetParameters(wA2);
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<Vector3Msg>("/fire_location", FireLocationCallback);
            ros.Subscribe<StringMsg>("/stop_robot", StopRobotCallback);
            ros.Subscribe<StringMsg>("/yolo/classification", YoloClassificationCallback);

            ros.Subscribe<TwistMsg>("/cmd_vel", Command_Velocity_Callback);
        }

        private void Command_Velocity_Callback(TwistMsg cmdVel)
        {
            rosLinear = (float)cmdVel.linear.x;
            rosAngular = (float)cmdVel.angular.z;
            lastCmdReceived = Time.time;
        }


        //Listens to yolo v8 vision transform classification
        void YoloClassificationCallback(StringMsg msg)
        {
            yoloFireDetected = (msg.data == "fire");
            Debug.Log($"YOLO classification: {msg.data}, Fire detected: {yoloFireDetected}");
        }

        void FireLocationCallback(Vector3Msg msg)
        {
            if (stopMovement) return;

            Vector3 fire = new Vector3((float)msg.x, (float)msg.y, (float)msg.z);
            firePositions.Enqueue(fire);
            Debug.Log($"Received fire position: {fire}");

            if (!hasGoal)
            {
                ProcessNextGoal();
            }
        }

        // No more fires = Robot stops moving. Mission accomplished
        void StopRobotCallback(StringMsg msg)
        {
            stopMovement = true;
            hasGoal = false;
            firePositions.Clear();
            RobotInput(0f, 0f);
            Debug.Log("Received stop message: " + msg.data);
        }


        // "Goal" meaning a 3D Vector of the Warehouse fire location
        void ProcessNextGoal()
        {
            if (firePositions.Count == 0)
            {
                hasGoal = false;
                Debug.Log("No more fires");
                return;
            }

            actualFirePosition = firePositions.Dequeue();
            currentGoal = actualFirePosition + new Vector3(navigationOffset, 0f, navigationOffset);
            hasGoal = true;
            Debug.Log($"Navigating to: {currentGoal}, actual fire at: {actualFirePosition}");
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Shelf"))
            {
                AvoidObstacle();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Shelf"))
            {
                isAvoiding = false;
                Debug.Log("Exited shelf trigger");
            }
        }

        /*
         * Robot shoots multiple raycasts continuously to avoid scene obstacles
         * sphereCast for redudancy to catch objects that the regular raycast beam may miss
         */
        void AvoidObstacle()
        {
            isAvoiding = false;
            avoidanceDirection = Vector3.zero;

            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

            Vector3[] rayDirections = {
                bf.forward,
                Quaternion.Euler(0, -30, 0) * bf.forward,   // raycast 30 deg left
                Quaternion.Euler(0, 30, 0) * bf.forward,    // raycast 30 deg right
                Quaternion.Euler(0, -45, 0) * bf.forward,   // raycast 45 deg left
                Quaternion.Euler(0, 45, 0) * bf.forward     // raycast 45 deg right
            };

            foreach (Vector3 direction in rayDirections)
            {
                if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, avoidDistance))
                {
                    if (hit.collider.CompareTag("Shelf") ||
                        hit.collider.gameObject.name == "WallPanel01" ||
                        hit.collider.GetComponent<BoxCollider>() != null ||
                        hit.collider.gameObject.name.ToLower().Contains("box") ||
                        hit.collider.gameObject.name.Contains("ShelvingRackRandom"))
                    {
                        if (hit.distance < avoidDistance * 0.75f)
                        {
                            isAvoiding = true;
                            float distanceFactor = 1f - (hit.distance / avoidDistance);
                            avoidanceDirection += hit.normal * repelForce * distanceFactor; // move perpendicular "normal" to object hit to avoid
                            Debug.DrawRay(rayOrigin, direction * avoidDistance, Color.red, 0.1f);
                        }
                    }
                }
            }

            if (Physics.SphereCast(rayOrigin, 0.2f, bf.forward, out RaycastHit sphereHit, avoidDistance))
            {
                if (sphereHit.collider.CompareTag("Shelf") ||
                    sphereHit.collider.gameObject.name == "WallPanel01" ||
                    sphereHit.collider.GetComponent<BoxCollider>() != null ||
                    sphereHit.collider.gameObject.name.ToLower().Contains("box") ||
                    sphereHit.collider.gameObject.name.Contains("ShelvingRackRandom"))
                {
                    if (sphereHit.distance < avoidDistance * 0.75f)
                    {
                        isAvoiding = true;
                        float distanceFactor = 1f - (sphereHit.distance / avoidDistance);
                        avoidanceDirection += sphereHit.normal * repelForce * distanceFactor;
                        Debug.DrawRay(rayOrigin, bf.forward * avoidDistance, Color.yellow, 0.1f);
                    }
                }
            }
            // If robot raycasts hit an object, avoid it by rotating the robot's "heading" or direction of travel
            if (isAvoiding)
            {
                avoidanceDirection = (bf.forward + avoidanceDirection).normalized; // calc the direction to steer toward for avoidance
                float signedAngle = Vector3.SignedAngle(bf.forward, avoidanceDirection, Vector3.up); //angle to rotate the robot about y axis(up) in Unity
                float rotSpeed = Mathf.Clamp(signedAngle * 0.05f, -maxRotationalSpeed * 0.5f, maxRotationalSpeed * 0.5f);
                RobotInput(navigationSpeed * 0.5f, rotSpeed); // robot slows down to safely rotate and avoid obstacle
                Debug.Log($"Avoiding obstacle: Angle {signedAngle}, RotSpeed {rotSpeed}");
            }
        }

        void FixedUpdate()
        {
            if (stopMovement || !hasGoal)
            {
                RobotInput(0f, 0f);
                return;
            }
            // Robot to Goal is a vector. vectors defined by magnitude(distance) and direction
            Vector3 direction = currentGoal - bf.position;
            direction.y = 0;
            float distance = direction.magnitude;

            // raycast in the direction that the onboard camera points
            Vector3 rayOrigin = vision_transform_camera.transform.position;
            Vector3 rayDirection = vision_transform_camera.transform.forward;
            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, rayColor, 0.1f);
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance))
            {
                if (hit.collider.gameObject.name.Contains("Flame") && yoloFireDetected) //uses vision_transform
                {
                    GameObject fire = hit.collider.gameObject;
                    Vector3 waterSpawnPosition = bf.position + bf.forward * 0.5f; // visually try and spawn water from robot's current location to the fire
                    GameObject water = Instantiate(waterPrefab.gameObject, waterSpawnPosition, Quaternion.identity);
                    water.GetComponent<ParticleSystem>().Play(); //Play attached water prefab for visual effect for 1s, then destroy the fire
                    Destroy(water, 1f);
                    Destroy(fire);
                    ProcessNextGoal();
                    return;
                }
            }

            AvoidObstacle();
            if (isAvoiding)
            {
                return;
            }
            //if robot has a goal(fire to put out) and isn't near, move towards it
            if (distance > 0.5f)
            {
                // (+) dot product = vectors point in same direction and < 90 deg angle between them. (+) signed angle = rotate to the right
                float rotSpeed = 0f;
                float forwardDot = Vector3.Dot(bf.forward, direction.normalized);
                float signedAngle = Vector3.SignedAngle(bf.forward, direction, Vector3.up);

                if (forwardDot > 0.1f)
                {
                    if (signedAngle > 10f)
                    {
                        rotSpeed = maxRotationalSpeed * 0.5f;
                    }
                    else if (signedAngle < -10f)
                    {
                        rotSpeed = -maxRotationalSpeed * 0.5f;
                    }
                }
                else if (forwardDot < -0.1f)
                {
                    rotSpeed = signedAngle > 0 ? maxRotationalSpeed * 0.5f : -maxRotationalSpeed * 0.5f;
                }

                RobotInput(navigationSpeed, rotSpeed);
                Debug.Log($"Moving to: {currentGoal}, Distance: {distance}, RotSpeed: {rotSpeed}");
            }
            else // robot is next to the fire
            {
                //robot briefly stops movement & turns to look at the fire
                RobotInput(0f, 0f);
                if (actualFirePosition != Vector3.zero)
                {
                    Vector3 lookDirection = actualFirePosition - bf.position;
                    lookDirection.y = 0;
                    if (lookDirection != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                        bf.rotation = Quaternion.Slerp(bf.rotation, targetRotation, Time.deltaTime * 5f);
                    }
                }
            }
        }

        // kinematics for differential-drive turtlebot3
        private void RobotInput(float speed, float rotSpeed)
        {
            // upward bounds 
            if (speed > maxLinearSpeed) speed = maxLinearSpeed;
            if (rotSpeed > maxRotationalSpeed) rotSpeed = maxRotationalSpeed;


            float wheel1Rotation = (speed / wheelRadius);
            float wheel2Rotation = wheel1Rotation;
            float wheelSpeedDiff = (rotSpeed * trackWidth) / wheelRadius;

            if (rotSpeed != 0)
            {
                wheel1Rotation = (wheel1Rotation + wheelSpeedDiff) * Mathf.Rad2Deg;
                wheel2Rotation = (wheel2Rotation - wheelSpeedDiff) * Mathf.Rad2Deg;
            }
            else
            {
                wheel1Rotation *= Mathf.Rad2Deg;
                wheel2Rotation *= Mathf.Rad2Deg;
            }

            SetSpeed(wA1, wheel1Rotation);
            SetSpeed(wA2, wheel2Rotation);
        }

        private void SetParameters(ArticulationBody joint)
        {
            ArticulationDrive drive = joint.xDrive;
            drive.forceLimit = forceLimit;
            drive.damping = damping;
            joint.xDrive = drive;
        }
        private void SetSpeed(ArticulationBody joint, float wheelSpeed = float.NaN)
        {
            ArticulationDrive drive = joint.xDrive;
            if (float.IsNaN(wheelSpeed))
            {
                drive.targetVelocity = ((2 * maxLinearSpeed) / wheelRadius) * Mathf.Rad2Deg * (int)directionros;
            }
            else
            {
                drive.targetVelocity = wheelSpeed;
            }
            joint.xDrive = drive;
        }


    }
}