from sqlalchemy.orm import Session
from app.repositories.booking_repository import create_booking

def handle_create_booking(db: Session, booking_data):
    if booking_data.check_out <= booking_data.check_in:
        raise ValueError("Ngày check-out phải lớn hơn ngày check-in")

    return create_booking(
        db,
        {
            "user_id": booking_data.user_id,
            "room_id": booking_data.room_id,
            "check_in": booking_data.check_in,
            "check_out": booking_data.check_out,
        },
    )