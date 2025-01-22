import os
from fastapi import FastAPI, HTTPException
import uvicorn
import aiomysql
from prometheus_fastapi_instrumentator import Instrumentator
from models import AppUser

app = FastAPI()

# Add Prometheus metrics
Instrumentator().instrument(app).expose(app)

# MySQL connection configuration
DB_CONFIG = {
    "host": os.getenv("MYSQL_HOST", "localhost"),
    "port": int(os.getenv("MYSQL_PORT", 3306)),
    "user": os.getenv("MYSQL_USER", "root"),
    "password": os.getenv("MYSQL_PASSWORD", "example"),
    "db": os.getenv("MYSQL_DATABASE", "testdb"),
}

async def get_db_pool():
    return await aiomysql.create_pool(**DB_CONFIG)

@app.on_event("startup")
async def startup():
    app.state.pool = await get_db_pool()

@app.on_event("shutdown")
async def shutdown():
    app.state.pool.close()
    await app.state.pool.wait_closed()

@app.post("/users", response_model=dict)
async def create_user(user: AppUser):
    async with app.state.pool.acquire() as conn:
        # Perform 100 independent transactions
        user_ids = []
        for _ in range(100):
            async with conn.cursor() as cur:
                try:
                    query = """
                    INSERT INTO app_users (name, date_of_birth, biography)
                    VALUES (%s, %s, %s)
                    """
                    await cur.execute(query, (user.name, user.date_of_birth, user.biography))
                    await conn.commit()
                    user_ids.append(cur.lastrowid)
                except Exception as e:
                    raise HTTPException(status_code=500, detail=str(e))
        return {
            "user_ids": user_ids,
            "message": "100 users created successfully"
        }
        async with conn.cursor() as cur:
            try:
                query = """
                INSERT INTO app_users (name, date_of_birth, biography)
                VALUES (%s, %s, %s)
                """
                await cur.execute(query, (user.name, user.date_of_birth, user.biography))
                await conn.commit()
                
                user_id = cur.lastrowid
                return {
                    "user_id": user_id,
                    "message": "User created successfully"
                }
            except Exception as e:
                raise HTTPException(status_code=500, detail=str(e))

@app.get("/health")
async def health_check():
    try:
        async with app.state.pool.acquire() as conn:
            async with conn.cursor() as cur:
                await cur.execute("SELECT 1")
                return {"status": "healthy"}
    except Exception as e:
        raise HTTPException(status_code=503, detail=str(e))

if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
