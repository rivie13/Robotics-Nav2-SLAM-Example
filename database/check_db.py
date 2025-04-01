from app import app, db
from models import SimulationEvent

def check_database():
    with app.app_context():
        count = SimulationEvent.query.count()
        print(f"Total simulation events: {count}")
        
        events = SimulationEvent.query.all()
        for event in events:
            print(f"Event {event.id}: {event.robot_type} in {event.world_type}, disaster: {event.disaster_type}, completed: {event.completed}")

if __name__ == "__main__":
    check_database() 