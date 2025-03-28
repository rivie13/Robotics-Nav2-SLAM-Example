# TurtleBot3ManualConfig - Integration Guide

This document provides instructions for integrating the custom TurtleBot3ManualConfig robot model into the Nav2SLAMExampleProject Unity project.

## Overview

The TurtleBot3ManualConfig is a modified version of the standard TurtleBot3 robot, with the following key differences:
- Addition of a pump hose component for water/foam dispensing
- Modified inertial properties
- Simplified collision models
- Custom materials and appearances

## Prerequisites

- Unity 2020.3 or newer (same version as used by the original project)
- Nav2SLAMExampleProject opened in Unity
- The TurtleBot3ManualConfig.urdf file

## Integration Steps

### 1. Import the Custom URDF File

1. Copy the `TurtleBot3ManualConfig.urdf` file to the project directory:
   - Recommended location: `Nav2SLAMExampleProject/Assets/turtlebot3`
   
2. In Unity Editor, navigate to the imported URDF file and select it.

3. Use the "Import Robot from URDF" option in the Unity Robotics URDF Importer.
   - If this option doesn't appear, make sure the Unity Robotics URDF Importer package is installed.
   - You can install it from the Package Manager (Window > Package Manager > Add package from git URL): 
     ```
     https://github.com/Unity-Technologies/URDF-Importer.git
     ```

### 2. Configure the Robot Model

1. During import, set the following options:
   - Link Mesh Import Settings:
     - Convex: True
     - Add Colliders: True
   - Creation Settings:
     - Fix TF Frame Names: True
     - Mesh Decimal Places: 4
     - Use Gravity: True
     - Robot Mass: 1.3729 (match the base_link mass)
     - Controller Type: Position
     - Use Inertia Values from URDF: True

2. For the meshes, you may need to:
   - Use the existing turtlebot3_description meshes in `Nav2SLAMExampleProject/Assets/turtlebot3/turtlebot3_description/meshes`
   - Make sure the material paths in the URDF file correctly reference existing materials or create new ones

### 3. Configure the Physics Materials

1. Apply the `RobotTires.physicMaterial` to the wheel colliders:
   - Find the wheel_left_link and wheel_right_link in the hierarchy
   - Assign the material to their colliders

### 4. Configure the Robot Controller

1. Add the `AGVController` script to the root of the imported robot.

2. Configure the controller with the following settings:
   - Wheel1: Reference to wheel_right_link
   - Wheel2: Reference to wheel_left_link
   - Mode: ROS
   - Max Linear Speed: 2
   - Max Rotational Speed: 1
   - Wheel Radius: 0.033
   - Track Width: 0.288
   - Force Limit: 10
   - Damping: 10
   - ROS Timeout: 0.5

3. Set up the ArticulationBody components on the wheels:
   - Make sure the wheels have ArticulationBody components
   - Set the X Drive to use rotation drive
   - Set Force Limit and Damping

### 5. Add Sensors

1. Add the `LaserScanSensor` script to the base_scan object:
   - Samples: 360
   - Range: 3.5
   - Update Rate: 5
   - Topic Name: "scan"

### 6. Save as Prefab

1. Drag the fully configured robot into the Project panel to create a new prefab
   - Name it "TurtleBot3ManualConfig" to replace the existing prefab
   - Alternatively, create a new prefab with a different name

### 7. Add to Scene

1. Open your desired scene (e.g., `BasicScene` or `SimpleWarehouseScene`)
2. Delete any existing TurtleBot3 robot if present
3. Drag your new TurtleBot3ManualConfig prefab into the scene
4. Position it appropriately, typically at (0, 0, 0) or slightly above ground level

## Testing the Robot

1. Configure ROS network settings via `Robotics > ROS Settings`
2. Enter the ROS Master URI (typically `http://localhost:10000`)
3. Press Play to start the simulation
4. Control the robot in one of two ways:
   - Send ROS commands via the `cmd_vel` topic
   - Switch the controller mode to "Keyboard" for direct keyboard control

## Troubleshooting

### Missing Meshes
- Ensure the mesh paths in the URDF file correctly point to meshes in the project
- If necessary, copy meshes from the original TurtleBot3 model

### Physics Issues
- Adjust mass, inertia, and articulation body properties if the robot behaves unexpectedly
- Check collider shapes if there are collision issues

### Controller Problems
- Verify that the correct wheels are assigned to the controller
- Check that wheel radius and track width match the URDF specifications

### Sensor Problems
- Ensure sensors are properly positioned according to the URDF
- Verify topic names match your ROS configuration

## Custom Modifications

If you need to modify the pump hose component:
1. Locate the pump_hose_link in the robot hierarchy
2. Adjust its position, size, and appearance as needed
3. Ensure its physics properties (mass, inertia) are reasonable

## References

- Original TurtleBot3 documentation: https://emanual.robotis.com/docs/en/platform/turtlebot3/overview/
- Unity Robotics Hub: https://github.com/Unity-Technologies/Unity-Robotics-Hub
- Unity URDF Importer: https://github.com/Unity-Technologies/URDF-Importer