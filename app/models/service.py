from sqlalchemy import Column, Integer, String, Float
from app.db.database import Base

class HotelService(Base):
    __tablename__ = "services"

    id = Column(Integer, primary_key=True, index=True)
    service_name = Column(String(100), nullable=False)
    price = Column(Float, nullable=False)