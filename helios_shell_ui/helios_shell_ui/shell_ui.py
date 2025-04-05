import tkinter as tk
from tkinter import ttk
from main_window import MainWindow
from settings_window import SettingsWindow
from dashboard_window import DashboardWindow
from unity_container_window import UnityContainerWindow

class HeliosShellUI:
    def __init__(self, root):
        self.root = root
        self.root.title("Helios Shell UI")
        self.root.geometry("800x600")

        # Set a different Tkinter theme
        style = ttk.Style()
        style.theme_use("clam")  # Try "clam", "alt", or "default"

        # Create a tabbed interface (Notebook)
        self.notebook = ttk.Notebook(root)
        self.notebook.pack(fill="both", expand=True)

        # Create windows (frames) for each tab
        self.windows = {}
        self.windows["main"] = MainWindow(self.notebook, self.switch_window)
        self.windows["settings"] = SettingsWindow(self.notebook, self.switch_window)
        self.windows["dashboard"] = DashboardWindow(self.notebook, self.switch_window)
        self.windows["unity_container"] = UnityContainerWindow(self.notebook, self.switch_window)

        # Add windows to the notebook (tabs)
        for name, window in self.windows.items():
            self.notebook.add(window, text=name.capitalize())

        # Force UI update
        self.notebook.update()

        # Start on the Main window
        self.switch_window("main")

    def switch_window(self, window_name):
        # Select the tab
        self.notebook.select(self.windows[window_name])
        self.notebook.update()

if __name__ == "__main__":
    root = tk.Tk()
    app = HeliosShellUI(root)
    root.mainloop()