# Building a Bomb Defusing Robot for Unity Simulation

This guide outlines the process of creating a bomb defusing robot based on the TurtleBot3 platform in the Nav2 SLAM Example Unity project. The bomb defusing robot will include specialized components like a robotic arm, camera, and additional sensors to simulate bomb detection and defusal tasks.

## Prerequisites

Before starting this project, ensure you have:
- Completed the [Development Environment Setup](../readmes/dev_env_setup.md)
- Set up the [Unity Project](../readmes/unity_project.md)
- Familiarity with ROS2 and Unity integration

## Robot Design Overview

The bomb defusing robot will be built upon the TurtleBot3 base platform with the following additional components:

1. **Robotic Arm** - A multi-jointed arm for manipulating objects
2. **Gripper** - End effector for precise handling of components
3. **Cameras** - For visual inspection and navigation
4. **Specialized Sensors** - For detecting explosive materials and wires
5. **Protective Shield** - A front-facing shield for blast protection

## Step 1: Creating the URDF File

Start by creating a new URDF file based on the TurtleBot3 URDF, adding the bomb defusing components:

1. Copy the existing `TurtleBot3ManualConfig.urdf` file to a new file named `BombDefusingRobot.urdf` in the `Nav2SLAMExampleProject/Assets/turtlebot3/` directory.

2. Modify the URDF to include the new components. Here's a sample structure with key additions:

```xml
<?xml version="1.0" encoding="utf-8"?>
<robot name="BombDefusingRobot">
  <!-- Include existing TurtleBot3 materials -->
  <material name="light_black">
    <color rgba="0.1698 0.169 0.169 1" />
  </material>
  <material name="dark">
    <color rgba="0.3 0.3 0.3 1" />
  </material>
  
  <!-- Add new materials for bomb defusing components -->
  <material name="arm_material">
    <color rgba="0.7 0.7 0.7 1.0" />
  </material>
  <material name="gripper_material">
    <color rgba="0.5 0.5 0.5 1.0" />
  </material>
  <material name="shield_material">
    <color rgba="0.2 0.2 0.8 0.7" /> <!-- Transparent blue for shield -->
  </material>
  
  <!-- Base components (same as TurtleBot3) -->
  <link name="base_footprint" />
  <link name="base_link">
    <!-- Same as TurtleBot3 -->
  </link>
  
  <!-- Existing sensors and wheels (same as TurtleBot3) -->
  <link name="base_scan">
    <!-- Same as TurtleBot3 -->
  </link>
  <link name="imu_link" />
  <link name="wheel_right_link">
    <!-- Same as TurtleBot3 -->
  </link>
  <link name="wheel_left_link">
    <!-- Same as TurtleBot3 -->
  </link>
  
  <!-- New components for bomb defusing robot -->
  
  <!-- Robotic Arm Base -->
  <link name="arm_base_link">
    <inertial>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <mass value="0.2" />
      <inertia ixx="0.001" ixy="0" ixz="0" iyy="0.001" iyz="0" izz="0.001" />
    </inertial>
    <visual>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <geometry>
        <cylinder length="0.05" radius="0.04" />
      </geometry>
      <material name="arm_material" />
    </visual>
    <collision>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <geometry>
        <cylinder length="0.05" radius="0.04" />
      </geometry>
    </collision>
  </link>
  
  <!-- Arm Segment 1 -->
  <link name="arm_segment_1">
    <inertial>
      <origin rpy="0 0 0" xyz="0 0 0.1" />
      <mass value="0.1" />
      <inertia ixx="0.0008" ixy="0" ixz="0" iyy="0.0008" iyz="0" izz="0.0001" />
    </inertial>
    <visual>
      <origin rpy="0 0 0" xyz="0 0 0.1" />
      <geometry>
        <box size="0.02 0.02 0.2" />
      </geometry>
      <material name="arm_material" />
    </visual>
    <collision>
      <origin rpy="0 0 0" xyz="0 0 0.1" />
      <geometry>
        <box size="0.02 0.02 0.2" />
      </geometry>
    </collision>
  </link>
  
  <!-- Arm Segment 2 -->
  <link name="arm_segment_2">
    <inertial>
      <origin rpy="0 0 0" xyz="0 0 0.1" />
      <mass value="0.1" />
      <inertia ixx="0.0008" ixy="0" ixz="0" iyy="0.0008" iyz="0" izz="0.0001" />
    </inertial>
    <visual>
      <origin rpy="0 0 0" xyz="0 0 0.075" />
      <geometry>
        <box size="0.02 0.02 0.15" />
      </geometry>
      <material name="arm_material" />
    </visual>
    <collision>
      <origin rpy="0 0 0" xyz="0 0 0.075" />
      <geometry>
        <box size="0.02 0.02 0.15" />
      </geometry>
    </collision>
  </link>
  
  <!-- Gripper Base -->
  <link name="gripper_base">
    <inertial>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <mass value="0.05" />
      <inertia ixx="0.0001" ixy="0" ixz="0" iyy="0.0001" iyz="0" izz="0.0001" />
    </inertial>
    <visual>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <geometry>
        <box size="0.03 0.03 0.03" />
      </geometry>
      <material name="gripper_material" />
    </visual>
    <collision>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <geometry>
        <box size="0.03 0.03 0.03" />
      </geometry>
    </collision>
  </link>
  
  <!-- Gripper Left Finger -->
  <link name="gripper_finger_left">
    <inertial>
      <origin rpy="0 0 0" xyz="0 0.01 0.015" />
      <mass value="0.01" />
      <inertia ixx="0.00001" ixy="0" ixz="0" iyy="0.00001" iyz="0" izz="0.00001" />
    </inertial>
    <visual>
      <origin rpy="0 0 0" xyz="0 0.01 0.015" />
      <geometry>
        <box size="0.01 0.02 0.03" />
      </geometry>
      <material name="gripper_material" />
    </visual>
    <collision>
      <origin rpy="0 0 0" xyz="0 0.01 0.015" />
      <geometry>
        <box size="0.01 0.02 0.03" />
      </geometry>
    </collision>
  </link>
  
  <!-- Gripper Right Finger -->
  <link name="gripper_finger_right">
    <inertial>
      <origin rpy="0 0 0" xyz="0 -0.01 0.015" />
      <mass value="0.01" />
      <inertia ixx="0.00001" ixy="0" ixz="0" iyy="0.00001" iyz="0" izz="0.00001" />
    </inertial>
    <visual>
      <origin rpy="0 0 0" xyz="0 -0.01 0.015" />
      <geometry>
        <box size="0.01 0.02 0.03" />
      </geometry>
      <material name="gripper_material" />
    </visual>
    <collision>
      <origin rpy="0 0 0" xyz="0 -0.01 0.015" />
      <geometry>
        <box size="0.01 0.02 0.03" />
      </geometry>
    </collision>
  </link>
  
  <!-- Camera Link -->
  <link name="camera_link">
    <inertial>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <mass value="0.05" />
      <inertia ixx="0.0001" ixy="0" ixz="0" iyy="0.0001" iyz="0" izz="0.0001" />
    </inertial>
    <visual>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <geometry>
        <box size="0.02 0.05 0.02" />
      </geometry>
      <material name="dark" />
    </visual>
    <collision>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <geometry>
        <box size="0.02 0.05 0.02" />
      </geometry>
    </collision>
  </link>
  
  <!-- Bomb Sensor Link -->
  <link name="bomb_sensor_link">
    <inertial>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <mass value="0.02" />
      <inertia ixx="0.0001" ixy="0" ixz="0" iyy="0.0001" iyz="0" izz="0.0001" />
    </inertial>
    <visual>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <geometry>
        <cylinder length="0.02" radius="0.015" />
      </geometry>
      <material name="light_black" />
    </visual>
    <collision>
      <origin rpy="0 0 0" xyz="0 0 0" />
      <geometry>
        <cylinder length="0.02" radius="0.015" />
      </geometry>
    </collision>
  </link>
  
  <!-- Protective Shield -->
  <link name="shield_link">
    <inertial>
      <origin rpy="0 0 0" xyz="0.1 0 0" />
      <mass value="0.5" />
      <inertia ixx="0.001" ixy="0" ixz="0" iyy="0.001" iyz="0" izz="0.001" />
    </inertial>
    <visual>
      <origin rpy="0 0 0" xyz="0.1 0 0" />
      <geometry>
        <box size="0.02 0.25 0.2" />
      </geometry>
      <material name="shield_material" />
    </visual>
    <collision>
      <origin rpy="0 0 0" xyz="0.1 0 0" />
      <geometry>
        <box size="0.02 0.25 0.2" />
      </geometry>
    </collision>
  </link>
  
  <!-- Joints -->
  <!-- Base joints (same as TurtleBot3) -->
  <joint name="base_joint" type="fixed">
    <origin xyz="0 0 0.01" />
    <parent link="base_footprint" />
    <child link="base_link" />
  </joint>
  <joint name="scan_joint" type="fixed">
    <origin xyz="-0.064 0 0.122" />
    <parent link="base_link" />
    <child link="base_scan" />
  </joint>
  <joint name="imu_joint" type="fixed">
    <origin xyz="0 0 0.068" />
    <parent link="base_link" />
    <child link="imu_link" />
  </joint>
  <joint name="wheel_right_joint" type="continuous">
    <origin rpy="-1.57 0 0" xyz="0 -0.144 0.023" />
    <parent link="base_link" />
    <child link="wheel_right_link" />
    <axis xyz="0 1 0" />
  </joint>
  <joint name="wheel_left_joint" type="continuous">
    <origin rpy="-1.57 0 0" xyz="0 0.144 0.023" />
    <parent link="base_link" />
    <child link="wheel_left_link" />
    <axis xyz="0 1 0" />
  </joint>
  
  <!-- Arm joints -->
  <joint name="arm_base_joint" type="fixed">
    <parent link="base_link" />
    <child link="arm_base_link" />
    <origin xyz="0.05 0 0.1" />
  </joint>
  
  <joint name="arm_joint_1" type="revolute">
    <parent link="arm_base_link" />
    <child link="arm_segment_1" />
    <origin xyz="0 0 0.025" />
    <axis xyz="0 1 0" />
    <limit effort="1000.0" lower="-1.57" upper="1.57" velocity="0.5" />
  </joint>
  
  <joint name="arm_joint_2" type="revolute">
    <parent link="arm_segment_1" />
    <child link="arm_segment_2" />
    <origin xyz="0 0 0.2" />
    <axis xyz="0 1 0" />
    <limit effort="1000.0" lower="-1.57" upper="1.57" velocity="0.5" />
  </joint>
  
  <joint name="gripper_base_joint" type="revolute">
    <parent link="arm_segment_2" />
    <child link="gripper_base" />
    <origin xyz="0 0 0.15" />
    <axis xyz="0 0 1" />
    <limit effort="1000.0" lower="-3.14" upper="3.14" velocity="0.5" />
  </joint>
  
  <joint name="gripper_finger_left_joint" type="prismatic">
    <parent link="gripper_base" />
    <child link="gripper_finger_left" />
    <origin xyz="0 0 0" />
    <axis xyz="0 1 0" />
    <limit effort="100.0" lower="0" upper="0.02" velocity="0.1" />
  </joint>
  
  <joint name="gripper_finger_right_joint" type="prismatic">
    <parent link="gripper_base" />
    <child link="gripper_finger_right" />
    <origin xyz="0 0 0" />
    <axis xyz="0 -1 0" />
    <limit effort="100.0" lower="0" upper="0.02" velocity="0.1" />
  </joint>
  
  <joint name="camera_joint" type="fixed">
    <parent link="gripper_base" />
    <child link="camera_link" />
    <origin xyz="0 0 0.03" />
  </joint>
  
  <joint name="bomb_sensor_joint" type="fixed">
    <parent link="gripper_base" />
    <child link="bomb_sensor_link" />
    <origin xyz="0.02 0 0" />
  </joint>
  
  <joint name="shield_joint" type="fixed">
    <parent link="base_link" />
    <child link="shield_link" />
    <origin xyz="0.1 0 0.1" />
  </joint>
  
  <!-- Gazebo Plugins (Preserved for Movement) -->
  <!-- Include TurtleBot3 gazebo plugins -->
</robot>
```

## Step 2: Import the Robot into Unity

Once the URDF file is ready, follow these steps to import the robot into Unity:

1. In the Unity Editor, navigate to the Project window

2. Select `Assets > Import New Asset` and browse to your `BombDefusingRobot.urdf` file

3. Configure the import settings:
   - **Select Axis Type**: Choose "Y Axis" from the dropdown
   - **Select Convex Decomposer**: Choose "VHACD" from the dropdown
   - Check "Overwrite Existing Prefabs" if reimporting

4. Click "Import URDF" to start the import process

## Step 3: Configuring the Robot Components

After importing the robot, configure its components:

### Base Configuration (Similar to TurtleBot3)

1. Configure the wheel Articulation Bodies as per the TurtleBot3 configuration
   - Drive Type: X-Drive
   - Drive Mode: Force
   - Damping: 10
   - Force Limit: 10

2. Add the AGVController component to the root object and configure it with:
   ```
   Wheel1: [Reference to wheel_right_link]
   Wheel2: [Reference to wheel_left_link]
   Mode: ROS
   Max Linear Speed: 1.5 (slower than TurtleBot for precision)
   Max Rotational Speed: 0.8
   Wheel Radius: 0.033
   Track Width: 0.288
   Force Limit: 10
   Damping: 10
   ROS Timeout: 0.5
   ```

3. Configure the LaserScanSensor on the `base_scan` object, similar to TurtleBot3

### Robotic Arm Configuration

1. Add ArticulationBody components to all arm segments and joints:
   
   For `arm_joint_1` and `arm_joint_2`:
   - Drive Type: X-Drive
   - Drive Mode: Force
   - Stiffness: 10000
   - Damping: 1000
   - Force Limit: 1000
   - Lower Limit: -1.57
   - Upper Limit: 1.57

2. Add an ArmController script to the `arm_base_link`:
   ```csharp
   public class ArmController : MonoBehaviour
   {
       public ArticulationBody joint1;
       public ArticulationBody joint2;
       public ArticulationBody gripperJoint;
       public ArticulationBody leftFingerJoint;
       public ArticulationBody rightFingerJoint;
       
       // ROS topic for arm control
       private string armCommandTopic = "bomb_robot/arm_command";
       
       // Add implementation for subscribing to ROS topics
       // and controlling arm movements
   }
   ```

### Camera Configuration

1. Add a Camera component to the `camera_link` GameObject
2. Configure resolution and field of view
3. Add a ROS publisher script to send camera images to a ROS topic:
   ```csharp
   public class CameraPublisher : MonoBehaviour
   {
       public string topicName = "bomb_robot/camera";
       private Camera cam;
       
       // Implement methods to capture camera data
       // and publish to ROS
   }
   ```

### Bomb Sensor Configuration

1. Add a SensorController script to the `bomb_sensor_link`:
   ```csharp
   public class BombSensorController : MonoBehaviour
   {
       public string sensorTopic = "bomb_robot/bomb_detector";
       public float detectionRadius = 0.5f;
       
       // Implementation for detecting objects with "Explosive" tag
       // and publishing detection data to ROS
   }
   ```

## Step 4: Creating Materials

Create custom materials for the robot components:

1. In the Project window, right-click and select `Create > Material`
2. Create the following materials:
   - `ArmMaterial` - Silver/gray metallic
   - `GripperMaterial` - Dark gray
   - `ShieldMaterial` - Semi-transparent blue
   - `SensorMaterial` - Black with emission

3. Apply these materials to the corresponding robot components

## Step 5: Create a Prefab

Once all components are configured:

1. Drag the fully configured robot from the Hierarchy to the Project window
2. Name it "BombDefusingRobot"
3. Save it in the appropriate prefabs folder

## Step 6: ROS Integration

### ROS Messages and Services

Create custom ROS messages for the bomb defusing robot:

1. Add message definitions to your ROS workspace:
   - `ArmCommand.msg` - For controlling arm position
   - `GripperCommand.msg` - For controlling gripper state
   - `BombDetection.msg` - For bomb detection data

2. Create services:
   - `DisarmSequence.srv` - For executing a disarm sequence

### ROS Nodes

Develop ROS nodes to control the robot:

1. `arm_controller_node` - Receives commands and controls the arm
2. `bomb_detection_node` - Processes sensor data for bomb detection
3. `disarm_sequence_node` - Manages the disarm sequence workflow

## Step 7: Testing the Robot

1. Set up a test scene with simulated bombs
2. Configure ROS network settings in Unity
3. Test basic movement, arm control, and bomb detection
4. Create test scenarios for different bomb types

## Step 8: Building a UI for Robot Control

Create a UI for controlling the bomb defusing robot:

1. Design panels for:
   - Robot navigation
   - Arm control
   - Camera feed
   - Sensor readings
   - Disarm sequence controls

2. Implement keyboard shortcuts for common operations

## Advanced Features

Consider these advanced features for your bomb defusing robot:

1. **Procedural Bomb Generation** - Create a system to generate different types of bombs with varying difficulty
2. **Physics-Based Wire Cutting** - Implement realistic wire-cutting mechanics
3. **Timer Simulation** - Add countdown timers to bombs for increased tension
4. **Autonomous Mode** - Implement autonomous navigation and bomb detection
5. **Multi-Robot Coordination** - Enable cooperation between the bomb robot and other robots

## Troubleshooting Common Issues

### Robot Not Moving
- Check wheel ArticulationBody configuration
- Verify ROS connections are working properly

### Arm Not Responding
- Check joint limits and drive settings
- Ensure ROS topics are correctly set up

### Sensor Not Detecting
- Verify sensor range and position
- Check if layers and tags are properly configured

## Conclusion

By following this guide, you can create a bomb defusing robot simulation based on the TurtleBot3 platform. This simulation can be expanded with additional sensors, more complex arm mechanics, and advanced bomb defusing scenarios to create a comprehensive training tool for bomb disposal technicians. 