@echo off
echo Starting simulation database API...

REM Check if Python is installed
where python >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo Python not found. Please install Python and try again.
    pause
    exit /b 1
)

REM Remove existing virtual environment if it exists
if exist venv (
    echo Removing existing virtual environment...
    rmdir /s /q venv
)

REM Create a fresh virtual environment
echo Creating virtual environment...
python -m venv venv

REM Activate the virtual environment
call venv\Scripts\activate.bat

REM Install dependencies
echo Installing dependencies...
pip install -r requirements.txt

REM Initialize the database
echo Initializing database...
python init_db.py

REM Start the Flask application
echo Starting Flask API server...
python app.py

REM This line will only execute if the app crashes or is closed
call venv\Scripts\deactivate.bat
echo Server stopped.
pause 