import tkinter as tk
from tkinter import ttk, messagebox, simpledialog

class DashboardWindow(tk.Frame):
    def __init__(self, parent, switch_callback):
        super().__init__(parent)
        self.switch_callback = switch_callback

        # Table Headers
        columns = ["robot_type", "world_type", "disaster_type", "resolution_time_seconds",
                   "completed", "started_at", "completed_at"]

        # Title Label
        ttk.Label(self, text="Simulation Dashboard", font=("Arial", 16)).pack(pady=10)

        # Treeview (table)
        self.tree = ttk.Treeview(self, columns=columns, show="headings", selectmode="browse")
        self.tree.pack(padx=10, pady=10)

        # Add checkboxes using a separate column (displayed in Treeview's style workaround)
        self.check_vars = []
        for col in columns:
            self.tree.heading(col, text=col)
            self.tree.column(col, width=140, anchor='center')

        # Sample data (10 rows)
        self.sample_data = [
            ["TurtleBot3", "Warehouse", "Fire", 120.5, True, "2023-01-01 10:00:00", "2023-01-01 10:02:00"],
            ["TurtleBot3", "Office", "Smoke", 95.0, True, "2023-01-02 09:00:00", "2023-01-02 09:01:35"],
            ["TurtleBot3", "Factory", None, None, False, "2023-01-03 14:00:00", None],
            ["TurtleBot3", "Lab", "Chemical", 150.2, True, "2023-01-04 12:00:00", "2023-01-04 12:02:30"],
            ["TurtleBot3", "Basement", None, None, False, "2023-01-05 15:00:00", None],
            ["TurtleBot3", "Warehouse", "Fire", 102.1, True, "2023-01-06 13:00:00", "2023-01-06 13:01:42"],
            ["TurtleBot3", "Garage", None, None, False, "2023-01-07 11:00:00", None],
            ["TurtleBot3", "Kitchen", "Gas Leak", 77.8, True, "2023-01-08 08:30:00", "2023-01-08 08:31:48"],
            ["TurtleBot3", "Attic", None, None, False, "2023-01-09 16:00:00", None],
            ["TurtleBot3", "Lobby", "Overheat", 88.3, True, "2023-01-10 17:00:00", "2023-01-10 17:01:28"]
        ]

        self.tree_data_ids = []
        for i, row in enumerate(self.sample_data):
            row_id = self.tree.insert('', 'end', values=row)
            self.tree_data_ids.append(row_id)

        # Buttons
        btn_frame = ttk.Frame(self)
        btn_frame.pack(pady=10)

        ttk.Button(btn_frame, text="Update", command=self.update_selected_row).grid(row=0, column=0, padx=10)
        ttk.Button(btn_frame, text="Add", command=self.add_new_row).grid(row=0, column=1, padx=10)
        ttk.Button(self, text="Back to Main", command=lambda: self.switch_callback("main")).pack(pady=10)

    def update_selected_row(self):
        selected_item = self.tree.selection()
        if not selected_item:
            messagebox.showwarning("Warning", "Please select a row to update.")
            return

        item_id = selected_item[0]
        current_values = self.tree.item(item_id)['values']

        updated_values = []
        headers = ["robot_type", "world_type", "disaster_type", "resolution_time_seconds",
                   "completed", "started_at", "completed_at"]

        for i, header in enumerate(headers):
            val = simpledialog.askstring("Update Row", f"Enter new value for {header}:", initialvalue=current_values[i])
            if val is None:
                return  # Cancelled
            updated_values.append(val)

        self.tree.item(item_id, values=updated_values)

    def add_new_row(self):
        new_values = []
        headers = ["robot_type", "world_type", "disaster_type", "resolution_time_seconds",
                   "completed", "started_at", "completed_at"]

        for header in headers:
            val = simpledialog.askstring("Add Row", f"Enter value for {header}:")
            if val is None:
                return  # Cancelled
            new_values.append(val)

        self.tree.insert('', 'end', values=new_values)