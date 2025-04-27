#!/usr/bin/env python3
import rclpy
from rclpy.node import Node
from sensor_msgs.msg import Image, RegionOfInterest
from std_msgs.msg import String, Bool
from cv_bridge import CvBridge
from ultralytics import YOLO

class YoloNode(Node):
    def __init__(self):
        super().__init__('yolo_node')
        self.model = YOLO('/home/jaych/robotics/Robotics-Nav2-SLAM-Example/ML/yolov8n.pt')
        self.bridge = CvBridge()
        self.class_pub = self.create_publisher(String, '/yolo/classification', 10)
        self.bbox_pub = self.create_publisher(RegionOfInterest, '/yolo/bbox', 10)
        self.subscription = self.create_subscription(Image, '/camera/image_raw', self.image_callback, 10)
        

    def image_callback(self, msg):
        cv_image = self.bridge.imgmsg_to_cv2(msg, desired_encoding='bgr8')
        results = self.model(cv_image)

        for result in results:
            if result.boxes is not None and len(result.boxes) > 0:
                conf = result.boxes.conf[0].item()
                if conf > 0.5:
                    class_id = int(result.boxes.cls[0].item())
                    class_name = result.names[class_id]
                    if class_name == "orange":
                        class_name = "fire"
                        #self.class_pub.publish(String(data=class_name))
                    else:
                        class_name == "ignore"
                        #self.class_pub.publish(String(data=class_name))
                    self.class_pub.publish(String(data=class_name))
                    box = result.boxes.xyxy[0].cpu().numpy()
                    bbox_msg = RegionOfInterest()
                    bbox_msg.x_offset = int(box[0])
                    bbox_msg.y_offset = int(box[1])
                    bbox_msg.width = int(box[2] - box[0])
                    bbox_msg.height = int(box[3] - box[1])
                    self.bbox_pub.publish(bbox_msg)
                    break

def main(args=None):
    rclpy.init(args=args)
    node = YoloNode()
    rclpy.spin(node)
    node.destroy_node()
    rclpy.shutdown()

if __name__ == '__main__':
    main()
