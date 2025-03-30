from ultralytics import YOLO

def handle_detections(detections):
    for detection in detections:
        if detection["class"] == "fire" and detection["confidence"] > 0.7:
            print("fire detected")
        elif detection["class"] == "water" and detection["confidence"] > 0.7:
            print("flood detected")

if __name__ == "__main__":
    model = YOLO('yolo11n.pt')
    results = model(0, show=True)

    for result in results:
        frame_detections = [
            {"class": model.names[int(box.cls)], "confidence": float(box.conf), "bbox": box.xyxy[0].tolist()}
            for box in result.boxes
        ]
        # handle_detections(frame_detections) , this is where we handle such detections

