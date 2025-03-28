# Using the Custom TurtleBot3 Robot

This guide will walk you through the process of spawning and using the custom TurtleBot3ManualConfig robot in the Unity project. The TurtleBot3ManualConfig is a modified version of the standard TurtleBot3 robot with an additional pump hose component for water/foam dispensing applications, making it suitable for firefighting simulations.

## Prerequisites

Before proceeding with this guide, ensure you have:
- Completed the [Development Environment Setup](dev_env_setup.md)
- Set up the [Unity Project](unity_project.md)

## Importing the Custom TurtleBot3 URDF

1. First, locate the `TurtleBot3ManualConfig.urdf` file in the project's `Assets/turtlebot3` directory.

2. In the Unity Editor, navigate to the Project window and select the URDF file.

3. In the Inspector window, look for the "Import Robot from URDF" option. If you don't see this option, make sure the Unity Robotics URDF Importer package is installed.

4. Configure the import settings:
   - **Select Axis Type**: Choose "Y Axis" from the dropdown
   - **Select Convex Decomposer**: Choose "VHACD" from the dropdown
   - Check "Overwrite Existing Prefabs" if you're reimporting the robot

5. Click "Import URDF" to start the import process.

## Configuring the Robot

After importing the robot, you'll need to configure its components:

### 1. Configure the Wheel Articulation Bodies

1. In the Hierarchy window, locate the imported robot model.

2. Find and select the `wheel_left_link` and `wheel_right_link` objects.

3. For each wheel, check the ArticulationBody component and set:
   - Drive Type: X-Drive
   - Drive Mode: Force
   - Damping: 10
   - Force Limit: 10

4. Apply the RobotTires.physicMaterial to the wheel colliders:
   - Find the `RobotTires.physicMaterial` in `Assets/turtlebot3`
   - Drag it onto each wheel's collider component

### 2. Add the AGVController Script

1. Select the root of the imported robot model.

2. Add the AGVController component:
   - In the Inspector, click "Add Component"
   - Search for "AGVController"
   - Configure with the following settings:
     ```
     Wheel1: [Reference to wheel_right_link]
     Wheel2: [Reference to wheel_left_link]
     Mode: ROS
     Max Linear Speed: 2
     Max Rotational Speed: 1
     Wheel Radius: 0.033
     Track Width: 0.288
     Force Limit: 10
     Damping: 10
     ROS Timeout: 0.5
     ```

3. Drag the wheel_right_link and wheel_left_link GameObjects from the Hierarchy into the corresponding Wheel1 and Wheel2 fields.

### 3. Add the Laser Scan Sensor

1. Find the `base_scan` object in the robot hierarchy.

2. Add the LaserScanSensor component:
   - Click "Add Component"
   - Search for "LaserScanSensor"
   - Configure with:
     ```
     Samples: 360
     Min Angle: 0
     Max Angle: 360
     Range Min: 0.12
     Range Max: 3.5
     Update Rate: 5
     Topic Name: "scan"
     ```

### 4. Configure Custom Materials

For the new pump_hose_link:
1. Create a green material:
   - In the Project window, right-click and select "Create > Material"
   - Name it "HoseMaterial"
   - Set the Albedo color to green (RGB: 0, 128, 0)
2. Apply this material to the pump_hose_link object in the robot hierarchy

## Creating a Prefab

To make the robot reusable:

1. With the fully configured robot selected in the Hierarchy, drag it into the Project window.
2. Name the prefab "TurtleBot3ManualConfig".
3. You can delete the instance in the scene after creating the prefab.

## Spawning the Robot in a Scene

1. Open your desired scene (e.g., `BasicScene` or `SimpleWarehouseScene`).
2. If there's an existing TurtleBot3 robot, you can delete it.
3. Drag the TurtleBot3ManualConfig prefab from the Project window into the scene.
4. Position it appropriately, typically at the origin (0, 0, 0) or slightly above the ground.

## Testing the Robot

1. Configure ROS network settings:
   - Go to `Robotics > ROS Settings` in the Unity menu
   - Set ROS IP Address: 127.0.0.1 (or your ROS machine's IP)
   - Set ROS Port: 10000 (or your chosen port)
   - Click "Save"

2. Enter Play mode in Unity.

3. Test the robot using one of the following methods:
   - Send a test command to the `cmd_vel` topic from your ROS system:
     ```bash
     rostopic pub -1 /cmd_vel geometry_msgs/Twist "linear:
       x: 0.1
       y: 0.0
       z: 0.0
     angular:
       x: 0.0
       y: 0.0
       z: 0.0"
     ```
   - For direct keyboard control, change the controller mode to "Keyboard" in the AGVController component.

4. Verify the LiDAR is working by checking the scan topic:
   ```bash
   rostopic echo /scan
   ```

## Troubleshooting

### Robot Not Moving
- Ensure wheel references in the AGVController are correct
- Check that ArticulationBody components are properly configured
- Verify ROS connection settings are correct

### Missing/Incorrect Materials
- Check if materials are applied correctly to each component
- For the pump hose, make sure the green material is applied

### Physics Issues
- Adjust the mass and inertia properties if the robot behaves unrealistically
- Check collision geometries if there are weird interactions with the environment

### LiDAR Not Working
- Ensure the LaserScanSensor component is correctly configured
- Check that the base_scan transform is at the expected position

## Using the Pump Hose Feature

The TurtleBot3ManualConfig includes a pump hose for water/foam dispensing. In this simulation, the hose is represented as a green cylinder. For advanced simulations, you might want to:

1. Add a particle system to the end of the hose to simulate water/foam spray
2. Create a custom script to control the particle emission based on ROS commands
3. Add sensors to detect "fire" objects in the scene

## Next Steps

After successfully spawning the custom robot, you can:
- Follow the [Running the Example](run_example.md) guide to use it with Navigation2 and SLAM
- Explore the [Unity Visualization](unity_viz.md) options to visualize sensor data
- Extend the robot's capabilities with additional sensors or actuators 