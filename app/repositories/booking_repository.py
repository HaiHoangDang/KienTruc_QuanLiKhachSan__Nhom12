from sqlalchemy.orm import Session
from app.models.booking import Booking

def create_booking(db: Session, booking_data: dict):
    booking = Booking(**booking_data)
    db.add(booking)
    db.commit()
    db.refresh(booking)
    return booking