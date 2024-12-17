# Click Counter App

A Windows desktop application that tracks mouse clicks and keyboard presses, with real-time synchronization to a MongoDB database through a Node.js backend.

## Status
Initial build is DONE and running smoothly on the local! But the MongoDB connections and sync? Totally mashed up 😅🚧 I'll fix it when I get some time. But hey, y'all feel free to drop a PR! 🙌

## Features

- Real-time tracking of mouse clicks and keyboard presses
- User authentication (login/register)
- Background synchronization of statistics
- Minimizes to system tray
- Server-side data storage with MongoDB
- REST API backend with Node.js
- Secure authentication with JWT

## Prerequisites

### Client Application
- Windows OS
- .NET Framework 4.7.2 or later
- Visual Studio 2019 or later (for development)

### Server Application
- Node.js 14.x or later
- MongoDB 4.x or later
- npm or yarn package manager

## Installation

### Server Setup
1. Navigate to the `api-server` directory
2. Install dependencies:
   ```bash
   npm install
   ```
3. Create a `.env` file with the following variables:
   ```env
   MONGODB_URI=your_mongodb_connection_string
   JWT_SECRET=your_jwt_secret_key
   PORT=3000
   ```
4. Start the server:
   ```bash
   npm start
   ```

### Client Setup
1. Open the solution in Visual Studio
2. Restore NuGet packages
3. Build the solution
4. Configure the `App.config` with your server URL:
   ```xml
   <appSettings>
     <add key="ApiEndpoint" value="http://localhost:3000/api" />
   </appSettings>
   ```

## Usage

1. Launch the application
2. Register a new account or login with existing credentials
3. The application will start tracking mouse clicks and keyboard presses
4. Statistics are automatically synced to the server every minute
5. Minimize to system tray to keep tracking in background
6. Access settings to configure server URL or logout

## Development

### Project Structure

```
ClickCounterApp/
├── Forms/                 # Windows Forms UI
├── Models/               # Data models
├── Services/             # Business logic and services
├── Helpers/             # Utility classes
└── api-server/          # Node.js backend
    ├── src/
    │   ├── models/     # MongoDB schemas
    │   ├── routes/     # API endpoints
    │   └── utils/      # Utility functions
    └── tests/          # API tests
```

### API Endpoints

- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/clicks` - Save click data
- `GET /api/view/stats` - View user statistics

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This repository is licensed under a [﻿Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License](http://creativecommons.org/licenses/by-nc-sa/4.0/). For more information, see the [﻿LICENSE](LICENSE) file.

## Acknowledgments

- MongoDB for database
- Node.js for backend
- .NET Framework for Windows Forms
- JWT for authentication 
