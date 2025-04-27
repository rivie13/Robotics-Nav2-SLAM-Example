Prerequisites
- colcon build or ./build.sh in "/colcon_ws"
- source install/setup.bash if you didn't run colcon build



1. In 1 terminal, in the "/colcon_ws" path
- source ~/yolo_env/bin/activate
- ros2 run vision_transform yolo_node


2. In a 2nd terminal in "/colcon_ws" path
- ros2 launch unity_slam_example unity_slam_example.py
- This starts RVIZ on your PC!


3. In a 3rd terminal "/colcon_ws" path
- ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=127.0.0.1
- The Unity port under "Robotics" in Unity connects to the IP


4. Press the play button to start the Unity Simulation

