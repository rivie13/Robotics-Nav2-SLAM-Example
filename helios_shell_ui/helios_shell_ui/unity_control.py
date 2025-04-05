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