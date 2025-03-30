#!/bin/bash

python3 -m venv yolo_env || { echo "Failed to create venv"; exit 1; }
source yolo_env/bin/activate || { echo "Failed to activate venv"; exit 1; }
pip install --upgrade pip

if [ -f "requirements.txt" ]; then
    pip install -r requirements.txt || { echo "Failed to install local requirements"; exit 1; }
else
    echo "Warning: local requirements.txt not found, skipping"
fi

pip install -r https://raw.githubusercontent.com/ultralytics/yolov5/master/requirements.txt || { echo "Failed to install YOLOv5 requirements"; exit 1; }
echo "setup completed successfully"
