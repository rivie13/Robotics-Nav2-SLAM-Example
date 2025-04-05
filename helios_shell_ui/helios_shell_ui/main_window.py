import tkinter as tk
from tkinter import ttk

class MainWindow(tk.Frame):
    def __init__(self, parent, switch_callback):
        super().__init__(parent)
        self.switch_callback = switch_callback  # Function to switch windows

        # Main Window UI
        ttk.Label(self, text="Main Window").pack(pady=10)
        ttk.Button(self, text="Go to Settings", command=lambda: self.switch_callback("settings")).pack(pady=5)
        ttk.Button(self, text="Go to Dashboard", command=lambda: self.switch_callback("dashboard")).pack(pady=5)
        ttk.Button(self, text="Open Unity Container", command=lambda: self.switch_callback("unity_container")).pack(pady=5)