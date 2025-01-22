from datetime import date
from typing import Optional
from pydantic import BaseModel

class AppUser(BaseModel):
    name: str
    date_of_birth: date
    biography: Optional[str] = None 