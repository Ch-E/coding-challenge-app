# Script to start both the API and the Web application

# Start the API in a new tab
Start-Process powershell -ArgumentList "-Command", "cd $pwd; dotnet run --project CodingChallengeApp.Api --urls=http://localhost:5104; Read-Host 'Press Enter to exit...'"

# Wait a moment for the API to start
Start-Sleep -Seconds 3

# Start the Web app in the current console
Write-Host "Starting Web app..."
dotnet run --project CodingChallengeApp.Web --urls=http://localhost:5204 