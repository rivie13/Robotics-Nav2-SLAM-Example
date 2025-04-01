from flask_sqlalchemy import SQLAlchemy
from datetime import datetime
from sqlalchemy import Column, Integer, String, Float, DateTime, Boolean, ForeignKey
from sqlalchemy.orm import relationship

db = SQLAlchemy()

class SimulationEvent(db.Model):
    __tablename__ = 'simulation_events'
    
    id = Column(Integer, primary_key=True)
    robot_type = Column(String(100), nullable=False)
    world_type = Column(String(100), nullable=False)
    disaster_type = Column(String(100), nullable=True)
    resolution_time_seconds = Column(Float, nullable=True)
    completed = Column(Boolean, default=False)
    started_at = Column(DateTime, default=datetime.utcnow)
    completed_at = Column(DateTime, nullable=True)
    
    def __init__(self, robot_type, world_type, disaster_type=None):
        self.robot_type = robot_type
        self.world_type = world_type
        self.disaster_type = disaster_type
        self.completed = False
    
    def complete(self, resolution_time_seconds):
        self.completed = True
        self.resolution_time_seconds = resolution_time_seconds
        self.completed_at = datetime.utcnow()
    
    def to_dict(self):
        return {
            'id': self.id,
            'robot_type': self.robot_type,
            'world_type': self.world_type,
            'disaster_type': self.disaster_type,
            'resolution_time_seconds': self.resolution_time_seconds,
            'completed': self.completed,
            'started_at': self.started_at.isoformat() if self.started_at else None,
            'completed_at': self.completed_at.isoformat() if self.completed_at else None
        } 