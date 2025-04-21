using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.UrdfImporter;
using Unity.Robotics.UrdfImporter.Control;

public class ArmController : MonoBehaviour
{
    // Articulation Body References
    public ArticulationBody joint1;
    public ArticulationBody joint2;
    public ArticulationBody gripperBaseJoint;
    public ArticulationBody leftFingerJoint;
    public ArticulationBody rightFingerJoint;
    
    // ROS Parameters
    [Header("ROS Settings")]
    public string armCommandTopic = "bomb_robot/arm_command";
    public string gripperCommandTopic = "bomb_robot/gripper_command";
    public string armStateTopic = "bomb_robot/arm_state";
    private ROSConnection ros;
    
    // Movement parameters
    [Header("Control Parameters")]
    public float jointSpeed = 0.5f;
    public float gripperSpeed = 0.1f;
    public float positionTolerance = 0.01f;
    
    // Target positions
    private float joint1Target = 0f;
    private float joint2Target = 0f;
    private float gripperBaseTarget = 0f;
    private float gripperOpenAmount = 0f; // 0 = closed, 1 = open
    
    // Control mode
    public enum ControlMode { ROS, Keyboard };
    public ControlMode controlMode = ControlMode.ROS;
    
    // Preset positions
    [Header("Preset Positions")]
    public Vector3 homePosition = new Vector3(0f, 0f, 0f); // Joint1, Joint2, GripperBase
    public Vector3 transportPosition = new Vector3(0f, 0.7f, 0f);
    public Vector3 operatingPosition = new Vector3(0.5f, 1.0f, 0f);
    
    // Initialize
    void Start()
    {
        // Find articulation bodies if not set
        if (joint1 == null) joint1 = transform.Find("arm_segment_1")?.GetComponent<ArticulationBody>();
        if (joint2 == null) joint2 = transform.Find("arm_segment_1/arm_segment_2")?.GetComponent<ArticulationBody>();
        if (gripperBaseJoint == null) gripperBaseJoint = transform.Find("arm_segment_1/arm_segment_2/gripper_base")?.GetComponent<ArticulationBody>();
        if (leftFingerJoint == null) leftFingerJoint = transform.Find("arm_segment_1/arm_segment_2/gripper_base/gripper_finger_left")?.GetComponent<ArticulationBody>();
        if (rightFingerJoint == null) rightFingerJoint = transform.Find("arm_segment_1/arm_segment_2/gripper_base/gripper_finger_right")?.GetComponent<ArticulationBody>();
        
        // Set up ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<Vector3Msg>(armCommandTopic, OnArmCommandReceived);
        ros.Subscribe<Float32Msg>(gripperCommandTopic, OnGripperCommandReceived);
        
        // Initialize articulation bodies
        ConfigureArticulationBodies();
        
        // Move to home position
        MoveToPresetPosition(homePosition);
    }
    
    void ConfigureArticulationBodies()
    {
        // Configure drive parameters for joint1
        if (joint1 != null)
        {
            var drive = joint1.xDrive;
            drive.stiffness = 10000f;
            drive.damping = 1000f;
            drive.forceLimit = 1000f;
            joint1.xDrive = drive;
        }
        
        // Configure drive parameters for joint2
        if (joint2 != null)
        {
            var drive = joint2.xDrive;
            drive.stiffness = 10000f;
            drive.damping = 1000f;
            drive.forceLimit = 1000f;
            joint2.xDrive = drive;
        }
        
        // Configure drive parameters for gripperBaseJoint
        if (gripperBaseJoint != null)
        {
            var drive = gripperBaseJoint.xDrive;
            drive.stiffness = 10000f;
            drive.damping = 1000f;
            drive.forceLimit = 1000f;
            gripperBaseJoint.xDrive = drive;
        }
        
        // Configure drive parameters for gripper fingers
        if (leftFingerJoint != null)
        {
            var drive = leftFingerJoint.xDrive;
            drive.stiffness = 10000f;
            drive.damping = 100f;
            drive.forceLimit = 100f;
            leftFingerJoint.xDrive = drive;
        }
        
        if (rightFingerJoint != null)
        {
            var drive = rightFingerJoint.xDrive;
            drive.stiffness = 10000f;
            drive.damping = 100f;
            drive.forceLimit = 100f;
            rightFingerJoint.xDrive = drive;
        }
    }
    
    void Update()
    {
        if (controlMode == ControlMode.Keyboard)
        {
            HandleKeyboardInput();
        }
        
        // Move joints to target positions
        MoveJoints();
        
        // Publish arm state
        PublishArmState();
    }
    
    void HandleKeyboardInput()
    {
        // Joint 1 control
        if (Input.GetKey(KeyCode.W))
        {
            joint1Target += jointSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            joint1Target -= jointSpeed * Time.deltaTime;
        }
        
        // Joint 2 control
        if (Input.GetKey(KeyCode.A))
        {
            joint2Target += jointSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            joint2Target -= jointSpeed * Time.deltaTime;
        }
        
        // Gripper base control
        if (Input.GetKey(KeyCode.Q))
        {
            gripperBaseTarget += jointSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            gripperBaseTarget -= jointSpeed * Time.deltaTime;
        }
        
        // Gripper open/close
        if (Input.GetKey(KeyCode.Z))
        {
            gripperOpenAmount = Mathf.Clamp01(gripperOpenAmount + gripperSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.X))
        {
            gripperOpenAmount = Mathf.Clamp01(gripperOpenAmount - gripperSpeed * Time.deltaTime);
        }
        
        // Preset positions
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            MoveToPresetPosition(homePosition);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            MoveToPresetPosition(transportPosition);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            MoveToPresetPosition(operatingPosition);
        }
        
        // Enforce joint limits
        joint1Target = Mathf.Clamp(joint1Target, -1.57f, 1.57f);
        joint2Target = Mathf.Clamp(joint2Target, -1.57f, 1.57f);
        gripperBaseTarget = Mathf.Clamp(gripperBaseTarget, -3.14f, 3.14f);
    }
    
    void MoveJoints()
    {
        if (joint1 != null)
        {
            var drive = joint1.xDrive;
            drive.target = joint1Target * Mathf.Rad2Deg;
            joint1.xDrive = drive;
        }
        
        if (joint2 != null)
        {
            var drive = joint2.xDrive;
            drive.target = joint2Target * Mathf.Rad2Deg;
            joint2.xDrive = drive;
        }
        
        if (gripperBaseJoint != null)
        {
            var drive = gripperBaseJoint.xDrive;
            drive.target = gripperBaseTarget * Mathf.Rad2Deg;
            gripperBaseJoint.xDrive = drive;
        }
        
        // Move gripper fingers
        if (leftFingerJoint != null)
        {
            var drive = leftFingerJoint.xDrive;
            drive.target = gripperOpenAmount * 0.02f * 1000f; // Convert from meters to mm
            leftFingerJoint.xDrive = drive;
        }
        
        if (rightFingerJoint != null)
        {
            var drive = rightFingerJoint.xDrive;
            drive.target = gripperOpenAmount * 0.02f * 1000f; // Convert from meters to mm
            rightFingerJoint.xDrive = drive;
        }
    }
    
    void OnArmCommandReceived(Vector3Msg msg)
    {
        if (controlMode == ControlMode.ROS)
        {
            joint1Target = (float)msg.x;
            joint2Target = (float)msg.y;
            gripperBaseTarget = (float)msg.z;
            
            // Enforce joint limits
            joint1Target = Mathf.Clamp(joint1Target, -1.57f, 1.57f);
            joint2Target = Mathf.Clamp(joint2Target, -1.57f, 1.57f);
            gripperBaseTarget = Mathf.Clamp(gripperBaseTarget, -3.14f, 3.14f);
        }
    }
    
    void OnGripperCommandReceived(Float32Msg msg)
    {
        if (controlMode == ControlMode.ROS)
        {
            gripperOpenAmount = Mathf.Clamp01((float)msg.data);
        }
    }
    
    void PublishArmState()
    {
        // Create arm state message
        Vector3Msg armStateMsg = new Vector3Msg(
            joint1Target,
            joint2Target,
            gripperBaseTarget
        );
        
        // Publish message
        ros.Publish(armStateTopic, armStateMsg);
    }
    
    public void MoveToPresetPosition(Vector3 position)
    {
        joint1Target = position.x;
        joint2Target = position.y;
        gripperBaseTarget = position.z;
    }
    
    public void SetGripperOpenAmount(float amount)
    {
        gripperOpenAmount = Mathf.Clamp01(amount);
    }
} 