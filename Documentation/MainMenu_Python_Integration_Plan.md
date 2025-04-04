# Main Menu & Python Integration Implementation Plan

## Overview
This document outlines a streamlined plan to add a main menu to the Unity project and integrate with Python for external control within a 3-day timeframe.

## Day 1: Main Menu Implementation

### 1. Create Main Menu Scene
- Create a new scene called "MainMenuScene"
- Set up UI canvas with responsive layout (scale with screen size)
- Implement simple background image or color scheme

### 2. Implement Core Menu Elements
- Title/header for the application
- Simulation selection buttons:
  - SimpleWarehouse Simulation
  - Future simulations (placeholder buttons)
- Settings button (opens settings panel)
- Quit button

### 3. Settings Panel Implementation
- Simulation settings:
  - Robot model selection (right now there is only one robot, but there will be multiple...)
  - Environment options
  - Graphics settings

### 4. Navigation & Scene Management
- Create MenuManager script to handle:
  - Button interactions
  - Panel navigation (show/hide)
  - Scene loading
  - Settings persistence using PlayerPrefs

## Day 2: Configuration System & Python Integration Setup

### 1. Configuration System
- Create SimulationConfig scriptable object to store:
  - Robot configuration
  - Environment settings
- Implement JSON serialization for configs

### 2. Python Integration Basic Framework
- Create SocketManager script for TCP/IP communication
- Implement basic message protocol:
  - Command messages (start/stop/configure)
  - Status messages (simulation state)
  - Data messages (sensor/robot data)
- Add Python socket server template scripts

### 3. Menu-Python Connection
- Connect menu options to configuration system
- Implement configuration export functionality
- Add socket connection status indicators

## Day 3: Python GUI Implementation & Testing

### 1. Python GUI Implementation
- Create simple Python GUI using Tkinter:
  - Connection panel
  - Simulation control panel
  - Basic visualization area
- Implement socket client code
- Add configuration import/export

### 2. Build Configuration
- Set up scene management (main menu as starting scene)
- Configure build settings for standalone application
- Test headless operation mode

### 3. Final Testing & Documentation
- Test full workflow:
  - Menu → Configuration → Simulation
  - Python control of simulation
  - Data visualization
- Document usage instructions
- Create quick reference guide

## Implementation Notes

### Priority Features
1. Main menu with navigation to simulation
2. Basic configuration saving/loading
3. Simple Python control of simulation

### Deferred Features (Future Implementation)
1. Advanced visualization in Python
2. Multiple robot support
3. Complex simulation parameters
4. User accounts/profiles

## Python Integration Guide

### Communication Protocol
```
{
  "type": "command|status|data",
  "content": {
    // Message-specific content
  }
}
```

### Basic Python Example
```python
import socket
import json
import tkinter as tk

# Connect to Unity
client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client.connect(("localhost", 10000))

# Send command
def send_command(cmd, params={}):
    msg = {
        "type": "command",
        "content": {
            "command": cmd,
            "parameters": params
        }
    }
    client.send(json.dumps(msg).encode('utf-8'))
    
# Example GUI
root = tk.Tk()
root.title("Unity Simulation Control")

# Add buttons
start_btn = tk.Button(root, text="Start Simulation", 
                     command=lambda: send_command("start"))
start_btn.pack()

# Add more controls as needed
root.mainloop()
``` 