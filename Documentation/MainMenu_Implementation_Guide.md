# Main Menu & Python Integration Implementation Guide

This guide provides practical code samples and step-by-step instructions for implementing a main menu and Python integration within 3 days.

## Day 1: Main Menu Implementation

### Step 1: Create Main Menu Scene

1. In Unity, go to File > New Scene and save it as "MainMenuScene" in the Assets/Scenes folder
2. Add the scene to Build Settings (File > Build Settings > Add Open Scenes)

### Step 2: Create UI Canvas

```csharp
// Create a new Canvas GameObject
// GameObject > UI > Canvas

// Add a Canvas Scaler component and configure it:
// UI Scale Mode: Scale With Screen Size
// Reference Resolution: 1920x1080
// Screen Match Mode: Match Width Or Height
// Match: 0.5 (blend of width and height)
```

### Step 3: Create Menu Manager Script

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject simulationSelectPanel;
    
    [Header("Settings Fields")]
    public InputField rosIPAddressField;
    public Dropdown rosProtocolDropdown;
    public Toggle connectOnStartupToggle;
    
    [Header("Simulation Settings")]
    public Dropdown robotModelDropdown;
    
    // Currently selected simulation
    private string selectedSimulation = "SimpleWarehouseScene";
    
    void Start()
    {
        // Load saved settings
        LoadSettings();
        
        // Show main menu panel
        ShowPanel(mainMenuPanel);
    }
    
    public void ShowPanel(GameObject panelToShow)
    {
        // Hide all panels
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        simulationSelectPanel.SetActive(false);
        
        // Show the selected panel
        panelToShow.SetActive(true);
    }
    
    public void SelectSimulation(string simulationName)
    {
        selectedSimulation = simulationName;
        Debug.Log($"Selected simulation: {simulationName}");
    }
    
    public void StartSimulation()
    {
        // Save current settings before starting
        SaveSettings();
        
        // Load the selected simulation scene
        SceneManager.LoadScene(selectedSimulation);
    }
    
    public void QuitApplication()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.SetString("RosIPAddress", rosIPAddressField.text);
        PlayerPrefs.SetInt("RosProtocol", rosProtocolDropdown.value);
        PlayerPrefs.SetInt("ConnectOnStartup", connectOnStartupToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("RobotModel", robotModelDropdown.value);
        PlayerPrefs.Save();
    }
    
    public void LoadSettings()
    {
        rosIPAddressField.text = PlayerPrefs.GetString("RosIPAddress", "127.0.0.1");
        rosProtocolDropdown.value = PlayerPrefs.GetInt("RosProtocol", 1); // Default to ROS2
        connectOnStartupToggle.isOn = PlayerPrefs.GetInt("ConnectOnStartup", 1) == 1;
        robotModelDropdown.value = PlayerPrefs.GetInt("RobotModel", 0);
    }
}
```

### Step 4: Create Menu UI Layout

1. Create panels for each menu section
2. Add the following to the main menu:
   - Title Text
   - "Start Simulation" Button
   - "Settings" Button
   - "Quit" Button
3. Add the following to the settings panel:
   - ROS IP Address Input Field
   - ROS Protocol Dropdown
   - Connect on Startup Toggle
   - "Save" Button
   - "Back" Button

## Day 2: Configuration & Python Integration

### Step 1: Create Simulation Configuration System

```csharp
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class SimulationConfig
{
    // ROS Connection Settings
    public string rosIPAddress = "127.0.0.1";
    public int rosProtocol = 1; // 0 = ROS1, 1 = ROS2
    public bool connectOnStartup = true;
    
    // Robot Settings
    public int robotModel = 0; // 0 = Default TurtleBot
    
    // Environment Settings
    public string environmentName = "SimpleWarehouse";
    
    // Export to JSON
    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }
    
    // Import from JSON
    public static SimulationConfig FromJson(string json)
    {
        return JsonUtility.FromJson<SimulationConfig>(json);
    }
    
    // Save to file
    public void SaveToFile(string filename)
    {
        File.WriteAllText(filename, ToJson());
    }
    
    // Load from file
    public static SimulationConfig LoadFromFile(string filename)
    {
        if (File.Exists(filename))
        {
            return FromJson(File.ReadAllText(filename));
        }
        return new SimulationConfig();
    }
}
```

### Step 2: Create Socket Communication Manager

```csharp
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class SocketManager : MonoBehaviour
{
    public int port = 10000;
    public bool startOnAwake = true;
    
    private TcpListener server;
    private Thread listenerThread;
    private List<TcpClient> clients = new List<TcpClient>();
    private bool isRunning = false;
    
    void Awake()
    {
        if (startOnAwake)
            StartServer();
    }
    
    void OnDestroy()
    {
        StopServer();
    }
    
    public void StartServer()
    {
        if (isRunning) return;
        
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            listenerThread = new Thread(new ThreadStart(ListenForClients));
            listenerThread.IsBackground = true;
            listenerThread.Start();
            isRunning = true;
            Debug.Log($"Socket server started on port {port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error starting server: {e.Message}");
        }
    }
    
    public void StopServer()
    {
        if (!isRunning) return;
        
        isRunning = false;
        
        foreach (var client in clients)
        {
            if (client != null && client.Connected)
                client.Close();
        }
        
        clients.Clear();
        
        if (server != null)
            server.Stop();
        
        if (listenerThread != null && listenerThread.IsAlive)
            listenerThread.Abort();
        
        Debug.Log("Socket server stopped");
    }
    
    private void ListenForClients()
    {
        server.Start();
        
        while (isRunning)
        {
            try
            {
                TcpClient client = server.AcceptTcpClient();
                clients.Add(client);
                
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.IsBackground = true;
                clientThread.Start(client);
                
                Debug.Log("Client connected");
            }
            catch (Exception e)
            {
                if (isRunning)
                    Debug.LogError($"Error accepting client: {e.Message}");
            }
        }
    }
    
    private void HandleClientComm(object clientObj)
    {
        TcpClient client = (TcpClient)clientObj;
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[4096];
        int bytesRead;
        
        while (isRunning && client.Connected)
        {
            bytesRead = 0;
            
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                break;
            }
            
            if (bytesRead == 0)
                break;
            
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            
            // Process message (will implement in Day 3)
            ProcessMessage(message, client);
        }
        
        client.Close();
        clients.Remove(client);
        Debug.Log("Client disconnected");
    }
    
    private void ProcessMessage(string messageJson, TcpClient client)
    {
        // Will implement message processing logic in Day 3
        Debug.Log($"Received message: {messageJson}");
        
        // For now, just echo back the message
        SendToClient(messageJson, client);
    }
    
    public void SendToClient(string message, TcpClient client)
    {
        if (client == null || !client.Connected)
            return;
            
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending to client: {e.Message}");
        }
    }
    
    public void SendToAllClients(string message)
    {
        foreach (var client in clients)
        {
            if (client != null && client.Connected)
                SendToClient(message, client);
        }
    }
}
```

## Day 3: Python GUI & Integration

### Step 1: Python GUI Implementation

Create a file named `unity_control.py`:

```python
import socket
import json
import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import threading

class UnityControlGUI:
    def __init__(self, root):
        self.root = root
        self.root.title("Unity Simulation Control")
        self.root.geometry("800x600")
        
        self.connected = False
        self.client_socket = None
        self.receive_thread = None
        
        # Create frames
        self.connection_frame = ttk.LabelFrame(root, text="Connection")
        self.connection_frame.pack(fill="x", padx=10, pady=10)
        
        self.control_frame = ttk.LabelFrame(root, text="Simulation Control")
        self.control_frame.pack(fill="x", padx=10, pady=10)
        
        self.config_frame = ttk.LabelFrame(root, text="Configuration")
        self.config_frame.pack(fill="x", padx=10, pady=10)
        
        self.status_frame = ttk.LabelFrame(root, text="Status")
        self.status_frame.pack(fill="both", expand=True, padx=10, pady=10)
        
        # Connection controls
        ttk.Label(self.connection_frame, text="IP Address:").grid(row=0, column=0, padx=5, pady=5)
        self.ip_var = tk.StringVar(value="localhost")
        ttk.Entry(self.connection_frame, textvariable=self.ip_var).grid(row=0, column=1, padx=5, pady=5)
        
        ttk.Label(self.connection_frame, text="Port:").grid(row=0, column=2, padx=5, pady=5)
        self.port_var = tk.IntVar(value=10000)
        ttk.Entry(self.connection_frame, textvariable=self.port_var, width=6).grid(row=0, column=3, padx=5, pady=5)
        
        self.connect_button = ttk.Button(self.connection_frame, text="Connect", command=self.toggle_connection)
        self.connect_button.grid(row=0, column=4, padx=5, pady=5)
        
        # Simulation controls
        ttk.Button(self.control_frame, text="Start Simulation", command=lambda: self.send_command("start")).grid(row=0, column=0, padx=5, pady=5)
        ttk.Button(self.control_frame, text="Stop Simulation", command=lambda: self.send_command("stop")).grid(row=0, column=1, padx=5, pady=5)
        ttk.Button(self.control_frame, text="Pause Simulation", command=lambda: self.send_command("pause")).grid(row=0, column=2, padx=5, pady=5)
        ttk.Button(self.control_frame, text="Resume Simulation", command=lambda: self.send_command("resume")).grid(row=0, column=3, padx=5, pady=5)
        
        # Configuration controls
        ttk.Button(self.config_frame, text="Load Config", command=self.load_config).grid(row=0, column=0, padx=5, pady=5)
        ttk.Button(self.config_frame, text="Save Config", command=self.save_config).grid(row=0, column=1, padx=5, pady=5)
        ttk.Button(self.config_frame, text="Send Config", command=self.send_config).grid(row=0, column=2, padx=5, pady=5)
        
        # Status display
        self.status_text = tk.Text(self.status_frame, height=15, wrap=tk.WORD)
        self.status_text.pack(fill="both", expand=True, padx=5, pady=5)
        
        # Disable closing when connected
        self.root.protocol("WM_DELETE_WINDOW", self.on_closing)
    
    def toggle_connection(self):
        if not self.connected:
            self.connect()
        else:
            self.disconnect()
    
    def connect(self):
        try:
            ip = self.ip_var.get()
            port = self.port_var.get()
            
            self.client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.client_socket.connect((ip, port))
            
            self.connected = True
            self.connect_button.config(text="Disconnect")
            
            # Start receive thread
            self.receive_thread = threading.Thread(target=self.receive_messages)
            self.receive_thread.daemon = True
            self.receive_thread.start()
            
            self.log_status(f"Connected to {ip}:{port}")
        except Exception as e:
            messagebox.showerror("Connection Error", str(e))
            self.log_status(f"Connection error: {str(e)}")
    
    def disconnect(self):
        if self.client_socket:
            self.client_socket.close()
        
        self.connected = False
        self.connect_button.config(text="Connect")
        self.log_status("Disconnected")
    
    def receive_messages(self):
        while self.connected:
            try:
                data = self.client_socket.recv(4096)
                if not data:
                    break
                    
                message = data.decode('utf-8')
                self.log_status(f"Received: {message}")
            except:
                break
        
        # If we get here, the connection is broken
        if self.connected:
            self.root.after(0, self.handle_disconnection)
    
    def handle_disconnection(self):
        self.disconnect()
        self.log_status("Connection lost")
    
    def send_command(self, command, params=None):
        if not self.connected:
            messagebox.showwarning("Not Connected", "Please connect to Unity first.")
            return
            
        if params is None:
            params = {}
            
        message = {
            "type": "command",
            "content": {
                "command": command,
                "parameters": params
            }
        }
        
        self.send_message(message)
        self.log_status(f"Sent command: {command}")
    
    def load_config(self):
        filename = filedialog.askopenfilename(
            title="Load Configuration",
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")]
        )
        
        if not filename:
            return
            
        try:
            with open(filename, 'r') as f:
                config = json.load(f)
                self.log_status(f"Loaded config from {filename}")
                # Here you'd normally update UI elements with the loaded config
        except Exception as e:
            messagebox.showerror("Error", f"Failed to load config: {str(e)}")
    
    def save_config(self):
        # Here you'd normally gather config from UI elements
        config = {
            "rosIPAddress": "127.0.0.1",
            "rosProtocol": 1,
            "connectOnStartup": True,
            "robotModel": 0,
            "environmentName": "SimpleWarehouse"
        }
        
        filename = filedialog.asksaveasfilename(
            title="Save Configuration",
            defaultextension=".json",
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")]
        )
        
        if not filename:
            return
            
        try:
            with open(filename, 'w') as f:
                json.dump(config, f, indent=2)
                self.log_status(f"Saved config to {filename}")
        except Exception as e:
            messagebox.showerror("Error", f"Failed to save config: {str(e)}")
    
    def send_config(self):
        # Here you'd normally gather config from UI elements
        config = {
            "rosIPAddress": "127.0.0.1",
            "rosProtocol": 1,
            "connectOnStartup": True,
            "robotModel": 0,
            "environmentName": "SimpleWarehouse"
        }
        
        message = {
            "type": "config",
            "content": config
        }
        
        self.send_message(message)
        self.log_status("Sent configuration to Unity")
    
    def send_message(self, message):
        if not self.connected:
            return
            
        try:
            json_str = json.dumps(message)
            self.client_socket.sendall(json_str.encode('utf-8'))
        except Exception as e:
            self.log_status(f"Send error: {str(e)}")
    
    def log_status(self, message):
        self.status_text.insert(tk.END, f"{message}\n")
        self.status_text.see(tk.END)
    
    def on_closing(self):
        if self.connected:
            if messagebox.askyesno("Quit", "Disconnect and quit?"):
                self.disconnect()
                self.root.destroy()
        else:
            self.root.destroy()

if __name__ == "__main__":
    root = tk.Tk()
    app = UnityControlGUI(root)
    root.mainloop()
```

### Step 2: Complete Unity Integration

Add this script to process messages from Python:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public class CommandMessage
{
    public string command;
    public object parameters;
}

[Serializable]
public class Message
{
    public string type;
    public string content;
}

public class PythonCommandProcessor : MonoBehaviour
{
    public SocketManager socketManager;
    
    void Start()
    {
        if (socketManager == null)
            socketManager = GetComponent<SocketManager>();
    }
    
    public void ProcessMessage(string messageJson)
    {
        try
        {
            Message message = JsonUtility.FromJson<Message>(messageJson);
            
            switch (message.type)
            {
                case "command":
                    ProcessCommand(message.content);
                    break;
                case "config":
                    ProcessConfig(message.content);
                    break;
                default:
                    Debug.LogWarning($"Unknown message type: {message.type}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing message: {e.Message}");
        }
    }
    
    private void ProcessCommand(string commandJson)
    {
        CommandMessage cmd = JsonUtility.FromJson<CommandMessage>(commandJson);
        
        switch (cmd.command)
        {
            case "start":
                // Start the simulation
                break;
                
            case "stop":
                // Stop the simulation
                SceneManager.LoadScene("MainMenuScene");
                break;
                
            case "pause":
                Time.timeScale = 0;
                break;
                
            case "resume":
                Time.timeScale = 1;
                break;
                
            default:
                Debug.LogWarning($"Unknown command: {cmd.command}");
                break;
        }
    }
    
    private void ProcessConfig(string configJson)
    {
        SimulationConfig config = SimulationConfig.FromJson(configJson);
        // Apply configuration settings
        // This will depend on how your simulation is structured
    }
}
```

### Step 3: Finalize Main Menu with Python Connection Status

Add these UI elements to your main menu:

1. Python Connection Status indicator 
2. Manual connect/disconnect button
3. Auto-connect on startup option

## Quick Implementation Instructions

1. Create the main menu scene and UI
2. Implement the MenuManager script
3. Create Python socket communication
4. Implement the Python GUI
5. Test the full workflow

Don't forget to add both scenes to the Build Settings and set the Main Menu scene as the first scene to load.

## Building and Exporting

For a standalone build:
1. File > Build Settings
2. Add both scenes
3. Set platform (Windows/Mac/Linux)
4. Player Settings > Other Settings > check "Scripting Backend" is set to Mono
5. Build 