from pydantic import BaseModel

class RoomResponse(BaseModel):
    id: int
    room_number: str
    room_type: str
    price: float
    capacity: int
    status: str

    class Config:
        from_attributes = True