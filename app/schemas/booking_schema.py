from datetime import date
from pydantic import BaseModel

class BookingCreate(BaseModel):
    user_id: int
    room_id: int
    check_in: date
    check_out: date

class BookingResponse(BaseModel):
    id: int
    user_id: int
    room_id: int
    check_in: date
    check_out: date
    status: str

    class Config:
        from_attributes = True