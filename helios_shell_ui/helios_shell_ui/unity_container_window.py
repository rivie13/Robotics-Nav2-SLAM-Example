import tkinter as tk
from tkinter import ttk
import subprocess

class UnityContainerWindow(tk.Frame):
    def __init__(self, parent, switch_callback):
        super().__init__(parent)
        self.switch_callback = switch_callback
        self.unity_process = None

        # Unity Container Window UI
        ttk.Label(self, text="Unity Container Window").pack(pady=10)
        ttk.Button(self, text="Start Unity Simulation", command=self.start_unity).pack(pady=5)
        ttk.Button(self, text="Stop Unity Simulation", command=self.stop_unity).pack(pady=5)
        ttk.Button(self, text="Back to Main", command=lambda: self.switch_callback("main")).pack(pady=5)

    def start_unity(self):
        print("Starting Unity simulation...")
        # Path to unity_control.py (the Python executable)
        self.unity_process = subprocess.Popen(["python", "unity_control.py"])  # Replace with actual path

    def stop_unity(self):
        print("Stopping Unity simulation...")
        if self.unity_process:
            self.unity_process.terminate()
            self.unity_process = None