import os
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

class Config:
    DEBUG = False
    TESTING = False
    CSRF_ENABLED = True
    SECRET_KEY = 'simulation-database-secret-key'
    
    # Get PostgreSQL credentials from environment variables or use defaults
    PG_USER = os.environ.get('POSTGRES_USER', 'postgres')
    PG_PASSWORD = os.environ.get('POSTGRES_PASSWORD', 'postgres')
    PG_DB = os.environ.get('POSTGRES_DB', 'simulation_db')
    PG_HOST = os.environ.get('POSTGRES_HOST', 'localhost')
    
    # Use postgresql+psycopg2 instead of postgresql+psycopg
    SQLALCHEMY_DATABASE_URI = os.environ.get(
        'DATABASE_URL', 
        f'postgresql+psycopg2://{PG_USER}:{PG_PASSWORD}@{PG_HOST}:5432/{PG_DB}'
    )
    SQLALCHEMY_TRACK_MODIFICATIONS = False
    
    print(f"Database connection: postgresql+psycopg2://{PG_USER}:****@{PG_HOST}:5432/{PG_DB}")

class ProductionConfig(Config):
    DEBUG = False

class DevelopmentConfig(Config):
    DEVELOPMENT = True
    DEBUG = True

class TestingConfig(Config):
    TESTING = True

def get_config():
    env = os.environ.get('FLASK_ENV', 'development')
    if env == 'production':
        return ProductionConfig
    elif env == 'testing':
        return TestingConfig
    else:
        return DevelopmentConfig 