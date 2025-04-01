from app import app, db
from models import SimulationEvent
from flask_migrate import init, migrate, upgrade
import traceback
import sys
from sqlalchemy import inspect, text

def init_database():
    try:
        print("Starting database initialization...")
        with app.app_context():
            inspector = inspect(db.engine)
            
            # Check what schemas and tables actually exist
            schemas = inspector.get_schema_names()
            print(f"Available schemas in database: {schemas}")
            
            for schema in schemas:
                tables = inspector.get_table_names(schema=schema)
                print(f"Tables in schema '{schema}': {tables}")
            
            # Check initial state
            try:
                count = SimulationEvent.query.count()
                print(f"Initial table count: {count}")
                if count > 0:
                    print(f"Table exists and has {count} records")
            except Exception as e:
                print(f"Table might not exist yet: {e}")
                
            # Create database tables
            print("Creating all tables...")
            db.create_all()
            
            # Initialize migrations
            print("Setting up migrations...")
            try:
                # Skip init if migrations directory exists
                try:
                    init()
                except Exception as e:
                    print(f"Migration init skipped (non-critical): {e}")
                migrate()
                upgrade()
            except Exception as e:
                print(f"Migration error (non-critical): {e}")
            
            # Add sample data (without checking if table is empty)
            print("Adding sample data to the database...")
            try:
                # First clear any existing data
                print("Clearing existing data...")
                db.session.execute(text("DELETE FROM simulation_events"))
                db.session.commit()
                print("Table cleared successfully")
                
                # Now add new samples
                samples = [
                    SimulationEvent('TurtleBot3', 'Warehouse'),
                    SimulationEvent('TurtleBot3', 'Office', 'Fire'),
                    SimulationEvent('CustomRobot', 'Apartment', 'Flood')
                ]
                
                # Complete one of the events
                samples[1].complete(120.5)  # 120.5 seconds to resolve
                
                for i, sample in enumerate(samples):
                    print(f"Adding sample {i+1}: {sample.robot_type} in {sample.world_type}")
                    db.session.add(sample)
                
                try:
                    db.session.commit()
                    print("Successfully committed sample data!")
                except Exception as e:
                    print(f"Error committing data: {e}")
                    db.session.rollback()
                    raise
                
                # Verify data was inserted using raw SQL to bypass any ORM issues
                result = db.session.execute(text("SELECT COUNT(*) FROM simulation_events"))
                sql_count = result.scalar()
                print(f"SQL query shows {sql_count} records in table")
                
                # Also check with ORM
                orm_count = SimulationEvent.query.count()
                print(f"ORM query shows {orm_count} records in table")
                
                if sql_count > 0:
                    print("Sample data added successfully!")
                else:
                    print("WARNING: Sample data may not have been added correctly!")
                
            except Exception as e:
                print(f"ERROR during sample data insertion: {e}")
                db.session.rollback()
                traceback.print_exc()
            
            print("Database initialization complete.")
    except Exception as e:
        print(f"ERROR during database initialization: {e}")
        traceback.print_exc()

if __name__ == '__main__':
    print("=== Running database initialization ===")
    init_database()
    print("=== Database initialization finished ===")
    sys.stdout.flush()  # Ensure all output is printed 