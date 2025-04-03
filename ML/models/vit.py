import cv2
import torch
import torchvision.transforms.v2 as v2
from torch.utils.data import Dataset, DataLoader
from typing import Tuple, Set
from torchvision.models import vit_b_16

class RealTimeCameraDataset(Dataset):
    def __init__(self, 
                 device: str = "cuda" if torch.cuda.is_available() else "cpu",
                 target_size: Tuple[int, int] = (224, 224),
                 fps: int = 30):
        self.device = torch.device(device)
        self.target_size = target_size
        self.fps = fps
        
        self.cap = cv2.VideoCapture(0)
        if not self.cap.isOpened():
            raise RuntimeError("Error: Could not open camera.")
            
        self.cap.set(cv2.CAP_PROP_FPS, fps)
        self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, target_size[0])
        self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, target_size[1])
        
        self.transform = v2.Compose([
            v2.ToImage(),
            v2.Resize(size=target_size, antialias=True),
            v2.ToDtype(torch.float32, scale=True),
            v2.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
            v2.RandomHorizontalFlip(p=0.5),
            v2.RandomRotation(degrees=15),
        ])

    def __len__(self) -> int:
        return 1000  

    def __getitem__(self, idx: int) -> torch.Tensor:
        ret, frame = self.cap.read()
        if not ret:
            raise RuntimeError("Error: Could not read frame from camera.")
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        tensor = self.transform(frame)
        return tensor.to(self.device)

    def release(self):
        self.cap.release()

class RealTimeClassifier:
    def __init__(self, 
                 target_classes: Set[int],
                 batch_size: int = 1,
                 target_size: Tuple[int, int] = (224, 224),
                 fps: int = 30):
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        
        # load model, use torch.compile (read about this if you want to know more, it kind of gets complex, but we def want to do this)
        self.model = vit_b_16(pretrained=True).to(self.device)
        self.model.eval()
        if hasattr(torch, "compile"):
            self.model = torch.compile(self.model)
        
        self.target_classes = target_classes
        
        # init dataset and dataloader
        self.dataset = RealTimeCameraDataset(
            device=self.device,
            target_size=target_size,
            fps=fps
        )

        self.dataloader = DataLoader(
            self.dataset,
            batch_size=batch_size,
            num_workers=0,
            drop_last=True
        )

    def predict(self) -> str:
        try:
            for batch in self.dataloader:
                with torch.no_grad():
                    predictions = self.model(batch)  # (batch_size, num_classes)
                    predicted_class = torch.argmax(predictions, dim=1)  # (batch_size,)
                    class_idx = predicted_class.item()  # single value for batch_size=1
                    return str(class_idx)  # convert to string
        except Exception as e:
            return f"Error: {str(e)}"
        finally:
            self.dataset.release()


if __name__ == "__main__":
    # choose which targets we want to be looking for
    # target_classes = {0, 1, 2} -> 
    classifier = RealTimeClassifier(
        target_classes=target_classes,
        batch_size=1,
        target_size=(224, 224),
        fps=30
    )
    
    result = classifier.predict()
    print(f"Predicted class index: {result}")
