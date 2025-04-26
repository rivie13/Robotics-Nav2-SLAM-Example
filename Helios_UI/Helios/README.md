# 🌍 Helios Simulation Dashboard

Helios is a desktop application built with Python and PyQt5 that embeds Unity-based simulation environments, such as wildfire, flood, and earthquake scenarios. It allows you to launch and interact with Unity simulation builds from a unified dashboard interface.

---

# ✅ How to Run

## 1. Install Dependencies
Make sure Python 3 is installed on your machine. Then, install the required libraries:

```bash
pip install PyQt5 matplotlib
```

## 2. Run the App
Navigate to your project directory and run:

```bash
python main.py
```

> **Note:** If you have a different version of Python installed, you may need to use `python3` or the full path to your `python.exe`.

---

# 🛠️ Important Setup

## Update Build Paths
Ensure that all paths to your Unity build files are correct for your device.

## Update the CSV Path
In both `insert_data.py` and `table.py`, set the CSV path to:

```python
CSV_PATH = r"C:\Users\mikeg\Documents\Helios_Without_UnityProjectFiles\dashboard_data.csv"
csv_path = r"C:\Users\mikeg\Documents\Helios_Without_UnityProjectFiles\dashboard_data.csv"
```

---

# 🧹 Adding a Unity Simulation

To add a new Unity world to the simulation menu:

### Step 1: Build the Unity Project
- Open your Unity scene.
- Go to **File > Build Settings**, select **Windows platform**, and build your `.exe` file.

### Step 2: Update the Config
- In `main.py`, locate the `__init__` method and find the `self.simulations_config` dictionary.
- Add a new entry in the following format:

```python
"my_custom_sim": {
    "exe_path": r"C:\\Full\\Path\\To\\Your\\UnityBuild.exe",
    "title": "My Custom Scenario",
    "hwnd_title": "UnityBuild"
}
```

- `exe_path`: Full file path to your Unity `.exe`.
- `title`: Display name that appears in the UI.
- `hwnd_title`: Must match the **window title** of your Unity build.

### Step 3: Restart the App
After updating, restart the app — your new Unity world will appear as an option in the simulation dashboard.

---

# 📦 Preconfigured Simulations

The following simulations are already included:

```python
self.simulations_config = {
    "wildfire": {
        "exe_path": r"C:\\Users\\mikeg\\Documents\\Helios_Without_UnityProjectFiles\\Helios_Without_UnityProjectFiles\\Helios\\build_warehouse\\RoboticsNav2SLAMExample.exe",
        "title": "Wild Fire | Multi-Robot",
        "hwnd_title": "RoboticsNav2SLAMExample"
    },
    "earthquake": {
        "exe_path": os.path.abspath("build/UnityHelios.exe"),
        "title": "Earthquake | Single-Robot",
        "hwnd_title": "UnityHelios"
    },
    "flood": {
        "exe_path": os.path.abspath(r"C:\\Users\\mikeg\\Documents\\Helios_Without_UnityProjectFiles\\Helios_Without_UnityProjectFiles\\Helios\\build_robot\\Helios.exe"),
        "title": "Flood | Single-Robot",
        "hwnd_title": "Helios"
    },
    "tornado": {
        "exe_path": None,
        "title": "Tornado | Multi-Robot",
        "hwnd_title": None
    },
    "search_rescue": {
        "exe_path": None,
        "title": "Search & Rescue | Multi-Robot",
        "hwnd_title": None
    },
    "hazmat": {
        "exe_path": None,
        "title": "Hazmat | Multi-Robot",
        "hwnd_title": None
    }
}
```

---

# 📊 Features

- Frameless PyQt5 GUI with a custom title bar
- Dynamically loaded Unity scenarios
- PDF export with graph visualizations (temperature, humidity, battery, position)
- Socket communication with Unity for live sensor data

---

# 🔗 TCP Server and Sensor Data Setup

When you run `main.py`, it **starts a TCP server**.

To send sensor data:

1. **Create a C# script** in your Unity project that establishes a socket connection to the server.
2. **Transmit sensor data** (e.g., LIDAR, battery level, temperature, position) through this socket.
3. The TCP server in `main.py` will **receive and display** this data live in the dashboard.

> **Note:** You’ll need to modify or create scripts based on your robot files to properly send the required sensor data.

---

# 💡 Final Tip

Run `main.py` to start the dashboard and connect your Unity worlds and sensors seamlessly!

