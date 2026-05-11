FROM python:3.12-slim

WORKDIR /app

# Cài dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy source code service
COPY . .

# Mount toàn bộ project vào /app/project (bind mount qua docker-compose)
# Biến môi trường
ENV PROJECT_ROOT=/app/project
ENV DS2API_URL=http://ds2api:5001
ENV DS2API_KEY=hotel-ai-key

EXPOSE 8000

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
