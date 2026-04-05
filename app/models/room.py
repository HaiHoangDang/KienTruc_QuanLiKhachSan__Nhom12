from sqlalchemy import Column, Integer, String, Float
from app.db.database import Base

class Room(Base):
    __tablename__ = "rooms"

    id = Column(Integer, primary_key=True, index=True)
    room_number = Column(String(50), nullable=False)
    room_type = Column(String(50), nullable=False)
    price = Column(Float, nullable=False)
    capacity = Column(Integer, nullable=False)
    status = Column(String(20), default="available")