from flask import Flask, request, jsonify
from flask_migrate import Migrate
from models import db, SimulationEvent
from config import get_config
import os
import json
from datetime import datetime

app = Flask(__name__)
app.config.from_object(get_config())
db.init_app(app)
migrate = Migrate(app, db)

@app.route('/api/events', methods=['POST'])
def create_event():
    data = request.json
    
    if not data:
        return jsonify({'error': 'No data provided'}), 400
    
    required_fields = ['robot_type', 'world_type']
    for field in required_fields:
        if field not in data:
            return jsonify({'error': f'Missing required field: {field}'}), 400
    
    event = SimulationEvent(
        robot_type=data['robot_type'],
        world_type=data['world_type'],
        disaster_type=data.get('disaster_type')
    )
    
    db.session.add(event)
    db.session.commit()
    
    return jsonify(event.to_dict()), 201

@app.route('/api/events/<int:event_id>/complete', methods=['POST'])
def complete_event(event_id):
    data = request.json
    
    if not data or 'resolution_time_seconds' not in data:
        return jsonify({'error': 'Resolution time required'}), 400
    
    event = SimulationEvent.query.get(event_id)
    if not event:
        return jsonify({'error': 'Event not found'}), 404
    
    event.complete(data['resolution_time_seconds'])
    db.session.commit()
    
    return jsonify(event.to_dict())

@app.route('/api/events', methods=['GET'])
def get_events():
    events = SimulationEvent.query.all()
    return jsonify([event.to_dict() for event in events])

@app.route('/api/events/<int:event_id>', methods=['GET'])
def get_event(event_id):
    event = SimulationEvent.query.get(event_id)
    if not event:
        return jsonify({'error': 'Event not found'}), 404
    
    return jsonify(event.to_dict())

if __name__ == '__main__':
    with app.app_context():
        db.create_all()
    app.run(host='0.0.0.0', port=5000) 