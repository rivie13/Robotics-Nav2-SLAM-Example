import torch
from PIL import Image
import cv2
import numpy as np

model = torch.hub.load("ultralytics/yolov5", "yolov5s", pretrained=True)

#device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
device = torch.device("mps" if torch.backends.mps.is_available() else "cpu")
model.to(device)

#img = Image.open("../test/car.jpg.webp")
#res = model(img)
#res.print()
#res.show()

# open camera
cap = cv2.VideoCapture(0)

if not cap.isOpened(): 
    print("Error: Could not open camera.")
    exit()

while True:
    ret, frame = cap.read()
    if not ret:
        print("Error: Could not read frame.")
        break

    # BGR -> RGB
    frame_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    # numpy to PIL image
    pil_img = Image.fromarray(frame_rgb)
    results = model(pil_img)
    frame_with_detections = results.render()[0]
    # back to BGR for OpenCV display
    frame_with_detections = cv2.cvtColor(frame_with_detections, cv2.COLOR_RGB2BGR)
    cv2.imshow("YOLOv5 Real-Time Detection", frame_with_detections)

    # break loop with q key, temp for testing
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()



