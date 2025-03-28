# TurtleBot3ManualConfig Implementation Guide

This guide provides step-by-step instructions to implement the custom TurtleBot3ManualConfig robot in the Nav2SLAMExampleProject Unity project.

## Step 1: Import the URDF File

1. Copy the `TurtleBot3ManualConfig.urdf` file to the `Nav2SLAMExampleProject/Assets/turtlebot3` directory.

2. Open the Unity project in the Unity Editor.

3. In the Project window, navigate to the turtlebot3 folder and locate the imported URDF file.

4. Right-click in the Project window and select `Import New Asset` if the file is not automatically detected.

## Step 2: Import URDF Using Unity Robotics URDF Importer

1. Make sure the Unity Robotics URDF Importer package is installed:
   - Go to `Window > Package Manager`
   - Click the `+` button and select `Add package from git URL`
   - Enter: `https://github.com/Unity-Technologies/URDF-Importer.git`
   - Click `Add`

2. With the URDF file selected in the Project window, look for the "Import Robot from URDF" option in the Inspector panel.

3. Configure the import settings:
   ```
   Link Mesh Import Settings:
   - Convex: True
   - Add Colliders: True
   
   Creation Settings:
   - Fix TF Frame Names: True
   - Mesh Decimal Places: 4
   - Use Gravity: True
   - Robot Mass: 1.3729
   - Controller Type: Position
   - Use Inertia Values from URDF: True
   ```

4. Click `Import URDF` to start the import process.

## Step 3: Fix Mesh References

1. After import, check the console for any errors related to missing mesh files.

2. For any missing meshes, you'll need to do the following:
   - Locate the corresponding mesh in `Nav2SLAMExampleProject/Assets/turtlebot3/turtlebot3_description/meshes`
   - Update the material references in the hierarchy

3. For the new pump_hose_link, create a green material:
   - In the Project window, right-click and select `Create > Material`
   - Name it "HoseMaterial"
   - Set the Albedo color to green (RGB: 0, 128, 0)
   - Apply this material to the pump_hose_link object

## Step 4: Configure ArticulationBody Components

1. Select the imported robot model in the Hierarchy window.

2. For the wheel links, locate the `wheel_left_link` and `wheel_right_link` objects.

3. On each wheel, check for an ArticulationBody component and configure it:
   - Set Drive Type to `X-Drive`
   - Set Drive Mode to `Force`
   - Set Damping to `10`
   - Set Force Limit to `10`

4. Apply the RobotTires.physicMaterial to the wheel colliders:
   - Locate the `RobotTires.physicMaterial` in `Nav2SLAMExampleProject/Assets/turtlebot3`
   - Drag it onto each wheel's collider component

## Step 5: Add the AGVController Script

1. Select the root of the imported robot model.

2. In the Inspector, click `Add Component` and search for "AGVController".

3. Configure the controller with the following parameters:
   ```
   Wheel1: [Reference to wheel_right_link GameObject]
   Wheel2: [Reference to wheel_left_link GameObject]
   Mode: ROS
   Max Linear Speed: 2
   Max Rotational Speed: 1
   Wheel Radius: 0.033
   Track Width: 0.288
   Force Limit: 10
   Damping: 10
   ROS Timeout: 0.5
   ```

4. Drag the wheel_right_link and wheel_left_link GameObjects from the Hierarchy into the corresponding fields.

## Step 6: Add the Laser Scan Sensor

1. Locate the `base_scan` object in the robot hierarchy.

2. Add the LaserScanSensor component:
   - Click `Add Component`
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

## Step 7: Configure Transform Hierarchy

1. Check the position and rotation of each link to ensure it matches the URDF specification.

2. Make sure the robot's root (base_footprint) is at the correct position (typically at world origin or slightly above ground).

3. Verify that all joint connections are correct, especially focusing on the new pump_hose_link.

## Step 8: Create a Prefab

1. Drag the fully configured robot from the Hierarchy to the Project window to create a prefab.

2. Name it "TurtleBot3ManualConfig" to match the existing naming convention.

3. You can delete the original instance from the scene after creating the prefab.

## Step 9: Replace in Scene

1. Open your target scene (BasicScene or SimpleWarehouseScene).

2. Delete any existing TurtleBot3 robot.

3. Drag the TurtleBot3ManualConfig prefab from the Project window into the scene.

4. Position it appropriately (check the original robot's position for reference).

## Step 10: Configure ROS Connection

1. Go to `Robotics > ROS Settings` in the Unity menu.

2. Configure the ROS connection settings:
   ```
   ROS IP Address: 127.0.0.1 (or your ROS machine's IP)
   ROS Port: 10000 (or your chosen port)
   ```

3. Click "Save".

## Step 11: Test the Robot

1. Enter Play mode in Unity.

2. The robot should appear in the scene with proper physics properties.

3. Send a test command to the `cmd_vel` topic from your ROS system:
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

4. Verify that the robot moves forward slowly.

5. If you prefer keyboard control for testing, change the controller mode to "Keyboard" in the AGVController component.

## Step 12: Verify All Components

1. Check that the pump_hose_link is visible and properly positioned.

2. Verify that the LiDAR sensor is functioning by checking the scan topic:
   ```bash
   rostopic echo /scan
   ```

3. Test navigation capabilities with ROS navigation stack if needed.

## Troubleshooting Common Issues

### Issue: Missing Mesh References
- Solution: Check file paths in the URDF and ensure all meshes are properly imported.

### Issue: Robot Physics Behavior Incorrect
- Solution: Adjust ArticulationBody parameters or check if the center of mass is properly set.

### Issue: ROS Communication Not Working
- Solution: Verify IP/Port settings and ensure the Unity project can connect to your ROS system.

### Issue: Robot Not Moving
- Solution: Check that the AGVController is properly configured and wheel references are correct.

### Issue: Visualization Issues with Pump Hose
- Solution: Manually create or adjust the material for the pump_hose_link.

## Next Steps

After successful implementation, you can:

1. Extend the robot's capabilities with additional scripts
2. Configure the pump hose with particle effects for visual feedback
3. Integrate with ROS navigation and SLAM systems
4. Test in more complex environments