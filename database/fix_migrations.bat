@echo off
echo Fixing database migrations system...

REM Activate the virtual environment if it exists, otherwise create it
if exist venv (
    call venv\Scripts\activate.bat
) else (
    echo Creating virtual environment...
    python -m venv venv
    call venv\Scripts\activate.bat
    echo Installing dependencies...
    pip install -r requirements.txt
)

REM Run the migration fix script
echo Running migration repair...
python fix_migrations.py

REM Deactivate the virtual environment
call venv\Scripts\deactivate.bat
echo Migration repair completed.
pause 