import os

BROKER_HOST = os.getenv("BROKER_HOST", "192.168.1.6")
BROKER_PORT = int(os.getenv("BROKER_PORT", "50051"))
BROKER_TARGET = os.getenv("BROKER_TARGET", f"{BROKER_HOST}:{BROKER_PORT}")
USE_TLS = os.getenv("BROKER_USE_TLS", "false").lower() == "true"
