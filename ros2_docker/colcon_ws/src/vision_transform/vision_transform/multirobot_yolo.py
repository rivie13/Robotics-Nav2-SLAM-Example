#!/usr/bin/env python3
import rclpy
from rclpy.node import Node
from sensor_msgs.msg import Image, RegionOfInterest
from std_msgs.msg import String
from cv_bridge import CvBridge
import cv2
import torch
import os
from ultralytics import YOLO

class MultiYoloNode(Node):
    def __init__(self):
        super().__init__('yolo_node')
        self.model = YOLO('/mnt/c/Users/jasea/OneDrive - Temple University/Desktop/helios/Robotics-Nav2-SLAM-Example/ML/yolov8n.pt')
        self.bridge = CvBridge()
        self.robot_ids = ["robot1", "robot2", "robot3"]
        self.subscribers = {}
        self.class_pubs = {}
        self.bbox_pubs = {}
        self.last_processed = {}

        for rid in self.robot_ids:
            self.subscribers[rid] = self.create_subscription(Image, f"/{rid}/camera/image_raw", lambda msg, r=rid: self.image_callback(msg, r), 10)
            self.class_pubs[rid] = self.create_publisher(String, f"/{rid}/yolo/classification", 10)
            self.bbox_pubs[rid] = self.create_publisher(RegionOfInterest, f"/{rid}/yolo/bbox", 10)
            self.last_processed[rid] = 0.0

    def image_callback(self, msg, robot_id):
        current_time = self.get_clock().now().to_msg().sec + self.get_clock().now().to_msg().nanosec * 1e-9
        if current_time - self.last_processed[robot_id] < 0.2:
            return
        self.last_processed[robot_id] = current_time

        cv_image = self.bridge.imgmsg_to_cv2(msg, 'bgr8')
        scale_factor = 0.5
        cv_image = cv2.resize(cv_image, (0, 0), fx=scale_factor, fy=scale_factor)
        results = self.model(cv_image, verbose=False)

        for result in results:
            if result.boxes is not None and len(result.boxes) > 0:
                conf = result.boxes.conf[0].item()
                if conf > 0.5:
                    class_id = int(result.boxes.cls[0].item())
                    class_name = result.names[class_id]
                    if class_name == "orange":
                        class_name = "fire"
                        self.class_pubs[robot_id].publish(String(data=class_name))
                    else:
                        continue
                    box = result.boxes.xyxy[0].cpu().numpy()
                    bbox_msg = RegionOfInterest()
                    bbox_msg.x_offset = int(box[0] / scale_factor)
                    bbox_msg.y_offset = int(box[1] / scale_factor)
                    bbox_msg.width = int((box[2] - box[0]) / scale_factor)
                    bbox_msg.height = int((box[3] - box[1]) / scale_factor)
                    self.bbox_pubs[robot_id].publish(bbox_msg)
                    break

def main(args=None):
    rclpy.init(args=args)
    node = MultiYoloNode()
    rclpy.spin(node)
    node.destroy_node()
    rclpy.shutdown()

if __name__ == '__main__':
    main()
