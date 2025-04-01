import os
import sys
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from dotenv import load_dotenv
from datetime import datetime

# Load environment variables
load_dotenv()

# Connection parameters from environment
PG_USER = os.environ.get('POSTGRES_USER', 'postgres')
PG_PASSWORD = os.environ.get('POSTGRES_PASSWORD', 'postgres')
PG_DB = os.environ.get('POSTGRES_DB', 'simulation_db')
PG_HOST = os.environ.get('POSTGRES_HOST', 'localhost')

# Database connection string
DB_URI = f'postgresql+psycopg2://{PG_USER}:{PG_PASSWORD}@{PG_HOST}:5432/{PG_DB}'
print(f"Connecting to: postgresql+psycopg2://{PG_USER}:****@{PG_HOST}:5432/{PG_DB}")

try:
    # Create engine and session
    engine = create_engine(DB_URI)
    Session = sessionmaker(bind=engine)
    session = Session()
    
    # Create simulation_events table directly with SQL
    print("Creating simulation_events table...")
    session.execute("""
    CREATE TABLE IF NOT EXISTS simulation_events (
        id SERIAL PRIMARY KEY,
        robot_type VARCHAR(100) NOT NULL,
        world_type VARCHAR(100) NOT NULL,
        disaster_type VARCHAR(100),
        resolution_time_seconds FLOAT,
        completed BOOLEAN DEFAULT FALSE,
        started_at TIMESTAMP DEFAULT NOW(),
        completed_at TIMESTAMP
    )
    """)
    
    # Insert sample data
    print("Inserting sample data...")
    current_time = datetime.utcnow()
    
    # First sample: TurtleBot3 in Warehouse
    session.execute("""
    INSERT INTO simulation_events 
    (robot_type, world_type, disaster_type, completed, started_at)
    VALUES ('TurtleBot3', 'Warehouse', NULL, FALSE, :timestamp)
    """, {"timestamp": current_time})
    
    # Second sample: TurtleBot3 in Office with Fire (completed)
    completed_time = datetime.utcnow()
    session.execute("""
    INSERT INTO simulation_events 
    (robot_type, world_type, disaster_type, resolution_time_seconds, completed, started_at, completed_at)
    VALUES ('TurtleBot3', 'Office', 'Fire', 120.5, TRUE, :timestamp, :completed_timestamp)
    """, {"timestamp": current_time, "completed_timestamp": completed_time})
    
    # Third sample: CustomRobot in Apartment with Flood
    session.execute("""
    INSERT INTO simulation_events 
    (robot_type, world_type, disaster_type, completed, started_at)
    VALUES ('CustomRobot', 'Apartment', 'Flood', FALSE, :timestamp)
    """, {"timestamp": current_time})
    
    # Commit changes
    session.commit()
    print("Sample data inserted successfully!")
    
    # Verify data was inserted
    result = session.execute("SELECT COUNT(*) FROM simulation_events")
    count = result.scalar()
    print(f"Total records in simulation_events: {count}")
    
    print("Database reset complete!")
    
except Exception as e:
    print(f"ERROR: {e}")
    import traceback
    traceback.print_exc()
finally:
    # Close session
    if 'session' in locals():
        session.close()
    print("Done.") 