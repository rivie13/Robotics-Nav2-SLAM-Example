# TurtleBot3 Model Comparison

This document provides a technical comparison between the original TurtleBot3 model included in the Unity project and the custom TurtleBot3ManualConfig model.

## Model Overview

| Feature | Original TurtleBot3 | TurtleBot3ManualConfig |
|---------|---------------------|------------------------|
| Robot Type | TurtleBot3 Waffle | Modified TurtleBot3 |
| Primary Function | Navigation & SLAM | Navigation, SLAM & Firefighting |
| Additional Components | Camera | Pump Hose |
| Structure | 3-wheel base (2 drive, 1 caster) | 2-wheel differential drive |

## URDF Structure Comparison

### Links

| Original TurtleBot3 | TurtleBot3ManualConfig | Notes |
|---------------------|------------------------|-------|
| base_footprint | base_footprint | Root frame, no changes |
| base_link | base_link | Main body, similar inertial properties |
| wheel_left_link | wheel_left_link | Similar structure |
| wheel_right_link | wheel_right_link | Similar structure |
| caster_back_left_link | *Not Present* | Removed in custom model |
| caster_back_right_link | *Not Present* | Removed in custom model |
| imu_link | imu_link | Sensor frame, unchanged |
| base_scan | base_scan | LiDAR sensor, unchanged |
| camera_link | *Not Present* | Camera not included in custom model |
| *Not Present* | pump_hose_link | New component for water/foam dispensing |

### Key Differences in URDF Elements

#### Materials:
- Original: Defines many materials (black, dark, light_black, blue, green, grey, orange, brown, red, white)
- Custom: Simplified to only three materials (light_black, dark, hose_material)

#### Pump Hose Component (New in TurtleBot3ManualConfig):
```xml
<link name="pump_hose_link">
  <inertial>
    <origin rpy="0 0 0" xyz="0 0 0" />
    <mass value="0.05" /> <!-- Lightweight to minimize impact on dynamics -->
    <inertia ixx="0.0001" ixy="0" ixz="0" iyy="0.0001" iyz="0" izz="0.0001" />
  </inertial>
  <visual>
    <origin rpy="0 0 0" xyz="0 0 0" />
    <geometry>
      <cylinder length="0.2" radius="0.01" /> <!-- Thin, long cylinder to represent a hose -->
    </geometry>
    <material name="hose_material">
      <color rgba="0.0 0.5 0.0 1.0" /> <!-- Green color for the hose -->
    </material>
  </visual>
  <collision>
    <origin rpy="0 0 0" xyz="0 0 0" />
    <geometry>
      <cylinder length="0.2" radius="0.01" />
    </geometry>
  </collision>
</link>

<joint name="pump_hose_joint" type="fixed">
  <parent link="base_link" />
  <child link="pump_hose_link" />
  <origin rpy="0 0 0" xyz="0.1 0 0.1" /> <!-- Positioned on top of base_link -->
</joint>
```

#### Collision Models:
- Original: Complex mesh colliders for most components
- Custom: Simplified box and cylinder colliders for better physics performance

#### Mesh References:
- Original: References detailed STL files from turtlebot3_description/meshes/
- Custom: Similar references but might need adaptation for the custom model

## Physics Properties Comparison

| Property | Original TurtleBot3 | TurtleBot3ManualConfig |
|----------|---------------------|------------------------|
| Base Mass | 1.3729096 kg | 1.3729095459 kg |
| Wheel Mass | 0.0284989402 kg | 0.0284989402 kg (unchanged) |
| Pump Hose Mass | N/A | 0.05 kg |
| Wheel Radius | 0.033 m | 0.033 m (unchanged) |
| Track Width | 0.287 m | 0.288 m (slightly adjusted) |

## Controller Configuration

Both models use the same `AGVController` script for control, but may require different configurations:

```csharp
public class AGVController : MonoBehaviour
{
    public GameObject wheel1; // Right wheel
    public GameObject wheel2; // Left wheel
    public ControlMode mode = ControlMode.ROS;
    
    // Physics parameters
    public float maxLinearSpeed = 2; //  m/s
    public float maxRotationalSpeed = 1;
    public float wheelRadius = 0.033f; //meters
    public float trackWidth = 0.288f; // meters Distance between tyres
    public float forceLimit = 10;
    public float damping = 10;
    
    // ROS integration
    public float ROSTimeout = 0.5f;
    // ... (remaining controller code)
}
```

## Sensor Configuration

### LiDAR Scanner
Both models include the same LiDAR scanner configuration, which uses the `LaserScanSensor` component:

```xml
<gazebo reference="base_scan">
  <material>Gazebo/FlatBlack</material>
  <sensor name="lds_lfcd_sensor" type="ray">
    <pose>0 0 0 0 0 0</pose>
    <visualize>false</visualize>
    <update_rate>5</update_rate>
    <ray>
      <scan>
        <horizontal>
          <samples>360</samples>
          <resolution>1</resolution>
          <min_angle>0.0</min_angle>
          <max_angle>6.28319</max_angle>
        </horizontal>
      </scan>
      <range>
        <min>0.120</min>
        <max>3.5</max>
        <resolution>0.015</resolution>
      </range>
      <noise>
        <type>gaussian</type>
        <mean>0.0</mean>
        <stddev>0.01</stddev>
      </noise>
    </ray>
    <plugin filename="libgazebo_ros_laser.so" name="gazebo_ros_lds_lfcd_controller">
      <topicName>scan</topicName>
      <frameName>base_scan</frameName>
    </plugin>
  </sensor>
</gazebo>
```

### Camera
The original model includes a camera, while the custom model does not:

```xml
<!-- Only in original model -->
<gazebo reference="camera_rgb_frame">
  <sensor name="realsense_R200" type="depth">
    <!-- Camera configuration -->
  </sensor>
</gazebo>
```

## ROS Integration

Both models use the same ROS communication setup:
- Standard cmd_vel topic for velocity commands
- Odometry published on the odom topic
- Sensor data published on appropriate topics (scan, imu)

The custom model should maintain compatibility with the existing ROS integration in the project.

## Adaptation Considerations

When using the TurtleBot3ManualConfig model in place of the original TurtleBot3:

1. Ensure the URDF file is correctly imported with all references resolved
2. Adjust the AGVController parameters if needed (particularly wheel references)
3. Create and configure the new pump_hose_link material and visual properties
4. Test the robot's physics behavior due to the changes in mass distribution
5. Verify that ROS communication works correctly with the modified robot

## Visualization Differences

| Component | Original TurtleBot3 | TurtleBot3ManualConfig |
|-----------|---------------------|------------------------|
| Base Color | Dark Grey | Light Black |
| Wheels | Dark/Black | Dark |
| Sensors | Various | Same as original |
| Pump Hose | Not present | Green cylindrical component |