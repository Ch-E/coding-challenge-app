# Coding Challenge App

This is a .NET Core application that provides a platform for coding challenges.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or Visual Studio Code
- SQL Server (local or Azure)
- SendGrid account (for email functionality)
- Azure Communication Services (for SMS functionality)

## Setup

1. Clone the repository
2. Copy `appsettings.template.json` to `appsettings.json` in the `CodingChallengeApp.Api` project
3. Update the following settings in `appsettings.json`:
   - `ConnectionStrings.DefaultConnection`: Your database connection string
   - `Jwt.Key`: A secure secret key for JWT token generation
   - `SendGrid.ApiKey`: Your SendGrid API key
   - `AzureCommunicationServices.ConnectionString`: Your Azure Communication Services connection string
   - `AzureCommunicationServices.SenderAddress`: Your verified sender email address

## Development

1. Open the solution in Visual Studio or Visual Studio Code
2. Restore NuGet packages
3. Run the application

## API Documentation

The API documentation is available at `/swagger` when running the application.

## Testing

The project includes unit tests and integration tests. Run them using:

```bash
dotnet test
```

## Contributing

1. Create a new branch for your feature
2. Make your changes
3. Submit a pull request

## License

This project is licensed under the MIT License. 