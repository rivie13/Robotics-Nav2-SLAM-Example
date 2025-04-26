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

        public float maxLinearSpeed = 0.8f;
        public float wheelRadius = 0.033f;
        public float trackWidth = 0.288f;
        public float maxRotationalSpeed = 1;

        public float forceLimit = 10;
        public float damping = 10;
        public float navigationSpeed = 0.3f;
        public float navigationOffset = 0.05f;
        public Transform bf;
        [SerializeField] private Camera vision_transform_camera;

        private ROSConnection ros;
        private Queue<Vector3> firePositions = new Queue<Vector3>();
        private Vector3 currentGoal;
        private Vector3 actualFirePosition;
        private bool hasGoal = false;

        public float avoidDistance = 12f; //was 4f
        public float repelForce = 20f; // was 6f then 10f(hit box)
        private bool isAvoiding = false;
        private Vector3 avoidanceDirection;

        public float rayDistance = 100f;
        public Color rayColor = Color.red;
        [SerializeField] private ParticleSystem waterPrefab;

        void Start()
        {
            wA1 = wheel1.GetComponent<ArticulationBody>();
            wA2 = wheel2.GetComponent<ArticulationBody>();
            if (wA1 == null || wA2 == null) Debug.LogError("Wheels missing ArticulationBody");

            SetParameters(wA1);
            SetParameters(wA2);
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<Vector3Msg>("/fire_location", FireLocationCallback);
        }

        void FireLocationCallback(Vector3Msg msg)
        {
            Vector3 fire = new Vector3((float)msg.x, (float)msg.y, (float)msg.z);
            firePositions.Enqueue(fire);
            Debug.Log($"Received fire position: {fire}");

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
                //Debug.Log("Exited shelf trigger");
            }
        }

        void AvoidObstacle()
        {
            if (Physics.Raycast(transform.position, bf.forward, out RaycastHit hit, avoidDistance))
            {
                if (hit.collider.CompareTag("Shelf") ||
                    hit.collider.gameObject.name == "WallPanel01" ||
                    hit.collider.GetComponent<BoxCollider>() != null ||
                    hit.collider.gameObject.name.ToLower().Contains("box")
                    || hit.collider.gameObject.name.Contains("Rack") )
                {
                    string obstacleType = hit.collider.CompareTag("Shelf") ? "Shelf" :
                                         (hit.collider.gameObject.name == "WallPanel01" ? "WallPanel" : "Box");
                    isAvoiding = true;
                    avoidanceDirection = bf.forward + hit.normal * repelForce;
                    Debug.DrawRay(transform.position, avoidanceDirection * avoidDistance, Color.red);
                    //Debug.Log($"Avoiding {obstacleType} at {hit.distance}m");
                }
                else
                {
                    isAvoiding = false;
                }
            }
            else
            {
                isAvoiding = false;
            }
        }

        void FixedUpdate()
        {
            if (!hasGoal)
            {
                RobotInput(0f, 0f);
                return;
            }

            Vector3 direction = currentGoal - bf.position;
            direction.y = 0;
            float distance = direction.magnitude;

            // camera always points in robots heading direction, so raycast in that direction
            Vector3 rayOrigin = vision_transform_camera.transform.position;
            Vector3 rayDirection = vision_transform_camera.transform.forward;
            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, rayColor, 50f);
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance))
            {
                //Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
                if (hit.collider.gameObject.name.Contains("Flame"))
                {
                    GameObject fire = hit.collider.gameObject;
                    GameObject water = Instantiate(waterPrefab.gameObject, bf.position, Quaternion.identity);
                    water.GetComponent<ParticleSystem>().Play();
                    Destroy(water, 1f);
                    Destroy(fire);
                    ProcessNextGoal();
                    return;
                }
            }

            // avoid obstacles (shelves, wall panels, and boxes)
            AvoidObstacle();
            if (isAvoiding)
            {
                //robot has turned via avoid obstacle function, now move away from avoided obstacle
                float signedAngle = Vector3.SignedAngle(bf.forward, avoidanceDirection, Vector3.up);
                float rotSpeed = signedAngle > 0 ? maxRotationalSpeed : -maxRotationalSpeed;
                RobotInput(navigationSpeed, rotSpeed);
                //Debug.Log($"Avoiding: Direction {avoidanceDirection}, RotSpeed {rotSpeed}");
                return;
            }

            if (distance > 1f)
            {
                //Dir to goal
                //higher forward.dot is numerically, means robot is traveleing directly toward the fire
                
                float rotSpeed = 0f;
                float forwardDot = Vector3.Dot(bf.forward, direction.normalized);
                float signedAngle = Vector3.SignedAngle(bf.forward, direction, Vector3.up); //vector rotated about the y axis. aka y axis is not changed

                if (forwardDot > 0.1f)
                {
                    if (signedAngle > 20 && signedAngle <= 90)
                    {
                        rotSpeed = maxRotationalSpeed;
                        //Debug.Log($"F RIGHT: Dist: {distance}");
                    }
                    else if (signedAngle < -20 && signedAngle >= -90)
                    {
                        rotSpeed = -maxRotationalSpeed;
                        //Debug.Log($"F LEFT: Dist: {distance}");
                    }
                }
                else if (forwardDot < -0.1f)
                {
                    rotSpeed = signedAngle > 0 ? maxRotationalSpeed : -maxRotationalSpeed;
                }

                RobotInput(navigationSpeed, rotSpeed);
                Debug.Log($"Moving to: {currentGoal}, Distance: {distance}");
            }
            else
            {
                RobotInput(0f, 0f);
                if (actualFirePosition != Vector3.zero)
                {
                    Vector3 lookDirection = actualFirePosition - bf.position;
                    lookDirection.y = 0;
                    if (lookDirection != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                        bf.rotation = targetRotation;
                    }
                }
            }
        }

        //"track width"? Need to confirm the meaning, since there's many names used to describe the exact same thing for Differential Drive Robots
        private void RobotInput(float speed, float rotSpeed)
        {
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

        private void SetSpeed(ArticulationBody joint, float wheelSpeed)
        {
            ArticulationDrive drive = joint.xDrive;
            drive.targetVelocity = wheelSpeed;
            joint.xDrive = drive;
        }
    }
}