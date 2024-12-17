# Click Tracker API Server

API server for the Click Tracker application that stores and manages click and keyboard press statistics.

## Prerequisites

- Node.js (v14 or higher)
- MongoDB (v4.4 or higher)
- npm or yarn

## Setup

1. Install dependencies:
```bash
npm install
```

2. Create a `.env` file in the root directory with the following variables:
```env
PORT=3000
MONGODB_URI=mongodb://localhost:27017/click-tracker
NODE_ENV=development
```

3. Create the logs directory:
```bash
mkdir logs
```

## Running the Server

Development mode with auto-reload:
```bash
npm run dev
```

Production mode:
```bash
npm start
```

## API Endpoints

### POST /api/clicks
Save click data.

Request headers:
- `x-client-id`: Unique identifier for the client (optional)

Request body:
```json
{
    "mouseClicks": 10,
    "keyboardPresses": 20,
    "timestamp": "2023-12-17T12:00:00Z"
}
```

### GET /api/clicks/stats
Get statistics for the last 24 hours.

Request headers:
- `x-client-id`: Unique identifier for the client (optional)

Response:
```json
{
    "success": true,
    "data": {
        "totalMouseClicks": 100,
        "totalKeyboardPresses": 200,
        "averageMouseClicks": 10,
        "averageKeyboardPresses": 20,
        "period": {
            "start": "2023-12-16T12:00:00Z",
            "end": "2023-12-17T12:00:00Z"
        }
    }
}
```

### GET /api/health
Health check endpoint.

## Error Handling

All endpoints return a consistent error format:
```json
{
    "success": false,
    "message": "Error description"
}
```

## Logging

Logs are stored in:
- `logs/error.log`: Error-level logs
- `logs/combined.log`: All logs

In development mode, logs are also printed to the console.

## Security

The server includes:
- CORS protection
- Helmet security headers
- Input validation
- Error handling middleware

## Production Deployment

1. Set environment variables for production
2. Ensure MongoDB is properly secured
3. Use a process manager like PM2
4. Set up proper monitoring and logging
5. Configure a reverse proxy (e.g., Nginx) 