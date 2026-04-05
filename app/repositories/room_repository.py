from sqlalchemy.orm import Session
from app.models.room import Room

def get_all_rooms(db: Session):
    return db.query(Room).all()

def get_available_double_rooms(db: Session):
    return (
        db.query(Room)
        .filter(Room.room_type == "double", Room.status == "available")
        .all()
    )