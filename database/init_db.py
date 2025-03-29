from app import app, db
from models import SimulationEvent
from flask_migrate import init, migrate, upgrade

def init_database():
    with app.app_context():
        # Create database tables
        db.create_all()
        
        # Initialize migrations
        init()
        migrate()
        upgrade()
        
        # Add some sample data
        if SimulationEvent.query.count() == 0:
            samples = [
                SimulationEvent('TurtleBot3', 'Warehouse'),
                SimulationEvent('TurtleBot3', 'Office', 'Fire'),
                SimulationEvent('CustomRobot', 'Apartment', 'Flood')
            ]
            
            # Complete one of the events
            samples[1].complete(120.5)  # 120.5 seconds to resolve
            
            for sample in samples:
                db.session.add(sample)
            
            db.session.commit()
            print("Sample data added to the database.")
        
        print("Database initialization complete.")

if __name__ == '__main__':
    init_database() 