import tkinter as tk
from tkinter import ttk

class SettingsWindow(tk.Frame):
    def __init__(self, parent, switch_callback):
        super().__init__(parent)
        self.switch_callback = switch_callback

        # Settings Window UI
        ttk.Label(self, text="Settings Window").pack(pady=10)
        ttk.Label(self, text="ROS IP Address:").pack(pady=5)
        self.ros_ip_entry = ttk.Entry(self)
        self.ros_ip_entry.insert(0, "127.0.1")
        self.ros_ip_entry.pack(pady=5)
        ttk.Button(self, text="Save Settings", command=self.save_settings).pack(pady=5)
        ttk.Button(self, text="Back to Main", command=lambda: self.switch_callback("main")).pack(pady=5)

    def save_settings(self):
        print(f"Saving ROS IP: {self.ros_ip_entry.get()}")
        # Add logic to save settings (e.g., to a file or database)