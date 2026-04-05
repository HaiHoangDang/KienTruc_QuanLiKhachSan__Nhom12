from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from app.db.database import get_db
from app.repositories.room_repository import get_all_rooms

router = APIRouter(prefix="/rooms", tags=["Rooms"])

@router.get("/")
def list_rooms(db: Session = Depends(get_db)):
    return get_all_rooms(db)