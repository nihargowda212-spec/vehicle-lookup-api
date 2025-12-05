# Vehicle Number Lookup - ASP.NET Core Minimal API

A simple web application for looking up vehicle information by registration number.

## Features

- Clean, modern UI with card-based layout
- ASP.NET Core 8 Minimal API backend
- Real-time vehicle information lookup
- Error handling for various scenarios
- Responsive design

## Prerequisites

- .NET 8 SDK installed
- Visual Studio 2022, VS Code, or any .NET-compatible IDE

## Running the Application

1. Open a terminal in the project directory
2. Run the following command:
   ```
   dotnet run
   ```
3. Open your browser and navigate to: `http://localhost:5000`

## API Endpoint

- **GET** `/api/vehicleinfo?regno={registration_number}`
  - Returns vehicle details in JSON format
  - Example: `/api/vehicleinfo?regno=KA01AB1234`

## Configuration

To use a real vehicle API, update the following in `Program.cs`:

```csharp
string apiUrl = "https://your-actual-api-url.com/lookup";
string apiKey = "YOUR_ACTUAL_API_KEY";
```

Currently, the application uses mock data for demonstration purposes.

## Project Structure

```
.
├── Program.cs              # Minimal API setup and endpoint
├── VehicleLookup.csproj    # Project file
├── appsettings.json        # Configuration
├── Properties/
│   └── launchSettings.json # Launch configuration
└── wwwroot/
    └── index.html          # Frontend UI
```

## Notes

- The application includes CORS support for cross-origin requests
- Static files are served from the `wwwroot` directory
- The API currently returns mock data when using placeholder URLs














