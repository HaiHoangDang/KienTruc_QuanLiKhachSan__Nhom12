# ĐẶC TẢ API

## 1. Auth Service
- `POST /api/auth/login`
- `POST /api/auth/register`
- `GET /api/auth/me`

## 2. Room Service
- `GET /api/rooms`
- `GET /api/rooms/{id}`
- `GET /api/rooms/available`

## 3. Booking Service
- `POST /api/bookings`
- `GET /api/bookings/{id}`
- `PUT /api/bookings/{id}/cancel`

## 4. Payment Service
- `POST /api/payments/create`
- `POST /api/payments/callback`

## 5. Chatbot Service
- `POST /chat`
- `GET /health`