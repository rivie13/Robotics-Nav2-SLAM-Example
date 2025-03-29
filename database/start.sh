#!/bin/bash
set -e

# Determine the PostgreSQL host
if [ -z "$POSTGRES_HOST" ]; then
    # Try to get the host IP from config.py
    echo "POSTGRES_HOST not set. Detecting from config.py..."
    DETECTED_HOST=$(python -c "from config import get_windows_host; print(get_windows_host())")
    export POSTGRES_HOST=$DETECTED_HOST
fi

echo "Using PostgreSQL host: $POSTGRES_HOST"

# Wait for PostgreSQL to be available
echo "Waiting for PostgreSQL to be ready..."
for i in {1..30}; do
    if nc -z $POSTGRES_HOST 5432; then
        echo "PostgreSQL is ready!"
        break
    fi
    
    if [ $i -eq 30 ]; then
        echo "Error: PostgreSQL not available after 30 seconds"
        echo "Please make sure PostgreSQL is running and configured to accept connections"
        echo "See the README.md for configuration instructions"
        exit 1
    fi
    
    echo "PostgreSQL not ready yet, waiting..."
    sleep 2
done

# Initialize the database
echo "Initializing database..."
python init_db.py || {
    echo "Database initialization failed."
    echo "Please check your PostgreSQL configuration."
    echo "See the README.md for configuration instructions."
    exit 1
}

# Start the Flask application
echo "Starting Flask API..."
python app.py 