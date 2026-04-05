from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from app.db.database import get_db
from app.schemas.booking_schema import BookingCreate
from app.services.booking_service import handle_create_booking

router = APIRouter(prefix="/bookings", tags=["Bookings"])

@router.post("/")
def create_new_booking(data: BookingCreate, db: Session = Depends(get_db)):
    try:
        return handle_create_booking(db, data)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))