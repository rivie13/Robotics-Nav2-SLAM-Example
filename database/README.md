# Simulation Database

This component provides a PostgreSQL database with SQLAlchemy ORM for storing simulation events in the Nav2 SLAM Example project.

## Setup Instructions

### Prerequisites

1. PostgreSQL Server (v12 or later) - Windows installation
2. pgAdmin 4 (for database management)
3. Python 3.7 or higher with pip (I am using 3.12.9 so I would recommend you do the same)

### PostgreSQL Setup

1. **Launch pgAdmin4** from the Start menu

2. **Create a new database**
   - Right-click on "Databases" in the browser tree
   - Select "Create" > "Database..."
   - Enter "simulation_db" as the database name
   - Leave the owner as "postgres" (or select your preferred user)
   - Click "Save"

3. **Create a user (optional)**
   - Right-click on "Login/Group Roles" in the browser tree
   - Select "Create" > "Login/Group Role..."
   - On the "General" tab, enter "simulation_user" as the name
   - On the "Definition" tab, enter a password
   - On the "Privileges" tab, enable "Can login?" and any other needed permissions
   - Click "Save"

4. **Grant privileges (if you created a custom user)**
   - Right-click on your "simulation_db" database
   - Select "Properties"
   - Go to the "Security" tab
   - Click "+" to add a new entry
   - Select your "simulation_user" from the dropdown
   - Check "ALL" under privileges
   - Click "Save"

### Finding Your PostgreSQL Password

If you've forgotten your PostgreSQL password, you have two options:

1. **Reset the postgres user password**:
   - In pgAdmin4, right-click on the "postgres" login role
   - Select "Properties"
   - Go to the "Definition" tab
   - Enter a new password
   - Click "Save"

2. **Use a new user with a known password**:
   - Follow the steps above to create a new "simulation_user" with a password you'll remember
   - Grant the appropriate privileges to the user
   - Then use those credentials as shown below

### Python Setup (Windows)

1. **Set up a Python virtual environment**
   ```cmd
   # Create a virtual environment in the database directory
   cd C:\Users\rivie\Robotics-Nav2-SLAM-Example\database
   python -m venv venv
   
   # Activate the virtual environment
   venv\Scripts\activate
   ```

2. **Install Python dependencies in the virtual environment**
   ```cmd
   # With virtual environment activated
   pip install -r requirements.txt
   ```

3. **Configure database connection credentials**
   - Create a .env file by copying the example:
   ```cmd
   copy .env.example .env
   ```
   
   - Edit the .env file with your configuration using your favorite text editor:
   ```
   POSTGRES_HOST=localhost
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=your_actual_password
   POSTGRES_DB=simulation_db
   ```
   
   The application will automatically load these settings.

4. **Initialize the database**
   ```cmd
   # With virtual environment activated
   python init_db.py
   ```

5. **Start the Flask API server**
   ```cmd
   # With virtual environment activated
   python app.py
   ```

6. **When finished, deactivate the virtual environment**
   ```cmd
   deactivate
   ```

### Quick Start (Using Batch File)

For convenience, a Windows batch file is provided to automate the setup and startup process:

1. **Run the batch file**
   - Double-click `start_windows.bat` in the database directory
   - Or run it from Command Prompt: `C:\Users\rivie\Robotics-Nav2-SLAM-Example\database\start_windows.bat`

2. **Access API**
   - The API will be available at http://localhost:5000

## Unity Integration

1. Add the `DatabaseConnector.cs` script to a GameObject in your Unity scene
2. Call the following methods from your robot spawn or game controller scripts:

   ```csharp
   // Get reference to the DatabaseConnector component
   DatabaseConnector dbConnector = FindObjectOfType<DatabaseConnector>();
   
   // Start a new simulation event when spawning a robot
   dbConnector.StartSimulationEvent("TurtleBot3", "Warehouse", "Fire");
   
   // Complete the event when the robot resolves the disaster
   dbConnector.CompleteSimulationEvent(120.5f);  // Time in seconds
   
   // Get all events (for statistics, etc.)
   dbConnector.GetAllEvents(events => {
       foreach (var evt in events)
       {
           Debug.Log($"Event {evt.id}: Robot {evt.robot_type} in {evt.world_type}");
       }
   });
   ```

## Database Schema

The database contains a `simulation_events` table with the following columns:

- `id` - Primary key
- `robot_type` - Type of robot that was spawned
- `world_type` - Environment the robot was spawned in
- `disaster_type` - Type of disaster that was spawned (null if none)
- `resolution_time_seconds` - Time taken to resolve the disaster (null if not completed)
- `completed` - Whether the simulation event was completed
- `started_at` - Timestamp when the simulation started
- `completed_at` - Timestamp when the simulation completed (null if not completed)

## API Endpoints

- `POST /api/events` - Create a new simulation event
- `GET /api/events` - Get all simulation events
- `GET /api/events/<id>` - Get a specific simulation event
- `POST /api/events/<id>/complete` - Mark a simulation event as completed

## Troubleshooting

### Connection Issues

- **"connection refused" error**: 
  - Ensure PostgreSQL is running on Windows
  - Verify that PostgreSQL service is started (check Windows Services)
  - Make sure you're using the correct host in .env (localhost or 127.0.0.1)

- **"password authentication failed" error**:
  - Ensure you're using the correct PostgreSQL password in your .env file
  - Verify credentials in pgAdmin4 by testing a connection
  - Try creating a new database user with a known password if needed

## Future Extensions

- Add more detailed data collection about robot performance
- Track robot path and movements
- Store sensor readings from simulations
- Implement analytics and visualization for simulation results 