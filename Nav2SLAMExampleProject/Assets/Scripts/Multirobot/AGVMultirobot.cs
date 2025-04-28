using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RosSharp.Control
{
    public class AGVMultirobot : MonoBehaviour
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
        private Vector3 currentGoal;
        private Vector3 actualFirePosition;
        private bool hasGoal = false;

        private float avoidDistance = 8f;
        private float repelForce = 6f;
        private bool isAvoiding = false;
        private Vector3 avoidanceDirection;

        private float rayDistance = 2.75f;
        public Color rayColor = Color.red;
        [SerializeField] private ParticleSystem waterPrefab;

        private bool yoloFireDetected = false;

        private string robotId;

        private float dangerZoneXOffset = 0.5f;
        private float dangerZoneZOffset = 0.5f;

        private GameObject[] otherRobots;
        private bool isAvoidingOtherRobot;

        private Queue<GameObject> waterPool = new Queue<GameObject>();
        private int poolSize = 5;

        private static List<Vector3> availableFirePositions = new List<Vector3>();
        private static readonly object fireAssignmentLock = new object();
        private static bool initialAssignmentDone = false;

        void Awake()
        {
            string name = gameObject.name;
            if (name == "Firefighter Robot")
            {
                robotId = "robot1";
            }
            else if (name == "Firefighter Robot (1)")
            {
                robotId = "robot2";
            }
            else if (name == "Firefighter Robot (2)")
            {
                robotId = "robot3";
            }
            else
            {
                robotId = "robot1";
            }
            Debug.Log($"{gameObject.name} assigned robotId: {robotId}");
        }

        void Start()
        {
            wA1 = wheel1?.GetComponent<ArticulationBody>();
            wA2 = wheel2?.GetComponent<ArticulationBody>();
            if (wA1 == null || wA2 == null)
            {
                Debug.LogError($"{robotId}: Wheels missing ArticulationBody");
                enabled = false;
                return;
            }

            SetParameters(wA1);
            SetParameters(wA2);

            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<Vector3Msg>($"/{robotId}/fire_location", FireLocationCallback);
            ros.Subscribe<StringMsg>($"/{robotId}/stop_robot", StopRobotCallback);
            ros.Subscribe<StringMsg>($"/{robotId}/yolo/classification", YoloClassificationCallback);
            Debug.Log($"{robotId} subscribed to /{robotId}/fire_location, stop_robot, yolo/classification");

            gameObject.tag = "robot";
            otherRobots = GameObject.FindGameObjectsWithTag("robot");

            if (waterPrefab == null)
            {
                Debug.LogError($"{robotId}: waterPrefab not assigned!");
                enabled = false;
                return;
            }
            // same thing as thread pooling like we did in Operating Systems
            for (int i = 0; i < poolSize; i++)
            {
                GameObject water = Instantiate(waterPrefab.gameObject);
                water.SetActive(false);
                waterPool.Enqueue(water);
            }

            // each robot's initial fire to navigate and put out. mutex so no race condition
            lock (fireAssignmentLock)
            {
                if (!initialAssignmentDone)
                {
                    GameObject[] fires = GameObject.FindGameObjectsWithTag("Fire");
                    availableFirePositions.AddRange(System.Array.ConvertAll(fires, fire => fire.transform.position));
                    initialAssignmentDone = true;
                    Debug.Log($"Initialized availableFirePositions with {availableFirePositions.Count} fires");
                }
            }

            StartCoroutine(AvoidOtherRobotsCoroutine());
            ProcessNextGoal();
        }

        // vision transform yolov8 classification from ROS2 workspace
        void YoloClassificationCallback(StringMsg msg)
        {
            yoloFireDetected = (msg.data == "fire");
            Debug.Log($"{robotId}: YOLO classification received: {msg.data}, yoloFireDetected={yoloFireDetected}");
        }

        // Same thing for 1 robot.  3D World position of the warehouse fire
        void FireLocationCallback(Vector3Msg msg)
        {
            if (!hasGoal)
            {
                ProcessNextGoal();
            }
        }
        
        void StopRobotCallback(StringMsg msg)
        {
            Debug.Log($"{robotId}: Received stop message: {msg.data}, but continuing to move toward fires");
        }

        // "Goal" meaning a fire's 3d coordinates to navigate to and extinguish
        void ProcessNextGoal()
        {
            GameObject[] remainingFires = GameObject.FindGameObjectsWithTag("Fire");
            if (remainingFires.Length == 0)
            {
                hasGoal = false;
                Debug.Log($"{robotId}: No more fires in scene, stopping");
                return;
            }

            lock (fireAssignmentLock)
            {
                // Update available FirePositions to match current scene fires
                List<Vector3> currentFirePositions = new List<Vector3>(System.Array.ConvertAll(remainingFires, fire => fire.transform.position));
                availableFirePositions.RemoveAll(pos => !currentFirePositions.Any(scenePos => Vector3.Distance(scenePos, pos) < 0.1f));

                foreach (var firePos in currentFirePositions)
                {
                    if (!availableFirePositions.Contains(firePos))
                    {
                        availableFirePositions.Add(firePos);
                    }
                }

                if (availableFirePositions.Count == 0)
                {
                    hasGoal = false;
                    Debug.Log($"{robotId}: No more fires in availableFirePositions, stopping");
                    return;
                }

                // Assign the first available fire. Robot's navigate close to,but not directly on top of the fire like for 1 robot.
                Vector3 assignedFire = availableFirePositions[0];
                availableFirePositions.RemoveAt(0);
                actualFirePosition = assignedFire;
                currentGoal = actualFirePosition + new Vector3(navigationOffset, 0f, navigationOffset);
                hasGoal = true;
                Debug.Log($"{robotId}: Assigned fire at {actualFirePosition}, navigating to: {currentGoal}, remaining fires: {availableFirePositions.Count}");
            }
        }

        // Avoid Shelf obstacles by any means. These triggers are redundant and not needed, but why not
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
                Debug.Log($"{robotId}: Exited shelf trigger");
            }
        }

        // Same thing for 1 robot. Raycast in many directions constantly to avoid all objects
        void AvoidObstacle()
        {
            if (isAvoidingOtherRobot) return;

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
                            avoidanceDirection += hit.normal * repelForce * distanceFactor;
                            Debug.DrawRay(rayOrigin, direction * avoidDistance, Color.red, 0.1f);
                        }
                    }
                }
            }
            // redundant raycast. Spherecast casts a sphere, that could clip objects that normal raycast beam may not hit
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

            if (isAvoiding)
            {
                avoidanceDirection = (bf.forward + avoidanceDirection).normalized;
                float signedAngle = Vector3.SignedAngle(bf.forward, avoidanceDirection, Vector3.up);
                float rotSpeed = Mathf.Clamp(signedAngle * 0.05f, -maxRotationalSpeed * 0.5f, maxRotationalSpeed * 0.5f);
                RobotInput(navigationSpeed * 0.5f, rotSpeed);
                Debug.Log($"{robotId}: Avoiding obstacle: Angle {signedAngle}, RotSpeed {rotSpeed}");
            }
        }

        // Robots should never collide. A robot is simply a Transform composed of an xyz( y is Unity is up, so it's irrelevant for ground robots)
        // "Danger Zone" is a constant radius around the robot that other robots should not be inside of.
        // In real life robotics the same concept is applied and I've applied it beforehand. This is a simplified version
        IEnumerator AvoidOtherRobotsCoroutine()
        {
            while (true)
            {
                isAvoidingOtherRobot = false;
                Vector3 robotPos = bf.position;
                float minX = robotPos.x - dangerZoneXOffset;
                float maxX = robotPos.x + dangerZoneXOffset;
                float minZ = robotPos.z - dangerZoneZOffset;
                float maxZ = robotPos.z + dangerZoneZOffset;

                foreach (var otherRobot in otherRobots)
                {
                    if (otherRobot == gameObject) continue;

                    Vector3 otherPos = otherRobot.transform.position;
                    if (otherPos.x >= minX && otherPos.x <= maxX &&
                        otherPos.z >= minZ && otherPos.z <= maxZ)
                    {
                        isAvoidingOtherRobot = true;
                        Vector3 directionToOther = (otherPos - robotPos).normalized;
                        Vector3 avoidanceDir = -directionToOther;
                        float signedAngle = Vector3.SignedAngle(bf.forward, avoidanceDir, Vector3.up);
                        float rotSpeed = Mathf.Clamp(signedAngle * 0.05f, -maxRotationalSpeed * 0.5f, maxRotationalSpeed * 0.5f);
                        RobotInput(navigationSpeed * 0.5f, rotSpeed);
                        Debug.Log($"{robotId}: Avoiding robot at {otherPos}");
                        break;
                    }
                }
                yield return new WaitForSeconds(0.2f);
            }
        }

        // Same method for 1 robot. Addition is Object pooling to reduce CPU load. Not at all necessary.
        // Similar to how we can assign a set number of worker threads to 1 client each in Operating Systems
        void FixedUpdate()
        {
            GameObject[] remainingFires = GameObject.FindGameObjectsWithTag("Fire");
            if (remainingFires.Length == 0)
            {
                RobotInput(0f, 0f);
                Debug.Log($"{robotId}: No fires remaining, stopping");
                return;
            }

            if (!hasGoal)
            {
                ProcessNextGoal();
            }

            Vector3 direction = currentGoal - bf.position;
            direction.y = 0;
            float distance = direction.magnitude;

            Vector3 rayOrigin = vision_transform_camera.transform.position;
            Vector3 rayDirection = vision_transform_camera.transform.forward;
            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, rayColor, 0.1f);
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance))
            {
                if (hit.collider.gameObject.name.Contains("Flame") && yoloFireDetected)
                {
                    GameObject fire = hit.collider.gameObject;
                    Vector3 waterSpawnPosition = bf.position + bf.forward * 0.5f;

                    GameObject water = waterPool.Dequeue();
                    water.transform.position = waterSpawnPosition;
                    water.transform.rotation = Quaternion.identity;
                    water.SetActive(true);
                    water.GetComponent<ParticleSystem>().Play();

                    StartCoroutine(DeactivateWaterParticle(water, 1f));

                    Destroy(fire);
                    ProcessNextGoal();
                    Debug.Log($"{robotId}: Extinguished fire at {actualFirePosition}");
                    return;
                }
            }

            AvoidObstacle();
            if (isAvoiding || isAvoidingOtherRobot)
            {
                return;
            }
            // same thing for 1 robot
            if (distance > 0.5f)
            {
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
                Debug.Log($"{robotId}: Moving to: {currentGoal}, Distance: {distance}, RotSpeed: {rotSpeed}");
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
                        bf.rotation = Quaternion.Slerp(bf.rotation, targetRotation, Time.deltaTime * 5f);
                    }
                }
            }
        }

        // Water particle is played in previous function, then calls this function to deactivate it.
        // If it really was object pool, then i'd test >5 robots so the 5 water objects would have to be shared, but works so whatever
        private IEnumerator DeactivateWaterParticle(GameObject water, float delay)
        {
            yield return new WaitForSeconds(delay);
            water.SetActive(false);
            waterPool.Enqueue(water);
        }

        private void RobotInput(float speed, float rotSpeed)
        {
            if (speed > maxLinearSpeed) speed = maxLinearSpeed;
            if (rotSpeed > maxRotationalSpeed) rotSpeed = maxRotationalSpeed;

            float wheel1Rotation = speed / wheelRadius;
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