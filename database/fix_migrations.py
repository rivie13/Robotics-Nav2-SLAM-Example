import os
import sys
from flask_migrate import init, migrate, upgrade, stamp
from app import app, db
from models import SimulationEvent

def fix_migrations():
    with app.app_context():
        print("Fixing database migrations...")
        
        # Check if migrations/versions directory exists and is empty
        versions_dir = os.path.join(os.path.dirname(__file__), 'migrations', 'versions')
        if not os.path.exists(versions_dir):
            os.makedirs(versions_dir)
            print(f"Created versions directory: {versions_dir}")
        
        # Ensure tables exist
        print("Ensuring tables exist...")
        db.create_all()
        
        # Check if we need to initialize migrations
        try:
            # Create a new migration if needed
            print("Creating migration...")
            migrate()
            
            # Apply the migration and stamp the database
            print("Applying migration and stamping database...")
            upgrade()
            
            # Extra stamp to ensure alembic_version is correct
            stamp()
            print("Migration system fixed successfully!")
            
            # Verify the simulation_events table exists and has the correct schema
            print("Verifying simulation_events table...")
            count = SimulationEvent.query.count()
            print(f"Table has {count} records")
            
            if count == 0:
                print("Adding sample data...")
                samples = [
                    SimulationEvent('TurtleBot3', 'Warehouse'),
                    SimulationEvent('TurtleBot3', 'Office', 'Fire'),
                    SimulationEvent('CustomRobot', 'Apartment', 'Flood')
                ]
                
                # Complete one of the events
                samples[1].complete(120.5)
                
                for sample in samples:
                    db.session.add(sample)
                
                db.session.commit()
                print(f"Successfully added {len(samples)} sample records")
            
        except Exception as e:
            print(f"Error during migration repair: {e}")
            import traceback
            traceback.print_exc()

if __name__ == '__main__':
    print("=== Running migration system repair ===")
    fix_migrations()
    print("=== Migration repair complete ===")
    sys.stdout.flush() 