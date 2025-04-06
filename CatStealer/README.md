# Cat Stealer API

This ASP.NET Core Web API project fetches cat images from the Cat as a Service API and stores them in a SQL Server database.

## Features

- Fetch cat images from the Cat API and store them in a database
- Store cat metadata including width, height, and temperament tags
- Process long-running tasks in the background using Hangfire
- RESTful API with paging and filtering support
- Swagger documentation

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Cat API Key from [thecatapi.com](https://thecatapi.com/)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/sevdalone/Cat-Stealer-API.git
cd cat-stealer
```

### 2. Configure the application

Update the `appsettings.json` file with your Cat API key and database connection strings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CatStealerDb;Trusted_Connection=True;MultipleActiveResultSets=true",
    "HangfireConnection": "Server=(localdb)\\mssqllocaldb;Database=CatStealerHangfire;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "CatApi": {
    "ApiKey": "YOUR_CAT_API_KEY_HERE"
  }
}
```

### 3. Apply database migrations

```bash
dotnet ef database update
```

### 4. Run the application

```bash
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

Swagger documentation is available at `/swagger`.

## API Endpoints

- **POST /api/cats/fetch**: Start a background job to fetch 25 cat images from the Cat API and save them to the database.
- **GET /api/cats/{id}**: Retrieve a cat by its ID.
- **GET /api/cats?page=1&pageSize=10**: Retrieve cats with paging support.
- **GET /api/cats?tag=playful&page=1&pageSize=10**: Retrieve cats with a specific tag with paging support.
- **GET /api/jobs/{id}**: Check the status of a background job.

## Docker Support

Build and run the application using Docker:

```bash
# Build the Docker image
docker build -t cat-stealer .

# Run the container
docker run -p 8080:80 -e "ConnectionStrings__DefaultConnection=YOUR_DB_CONNECTION_STRING" -e "CatApi__ApiKey=YOUR_API_KEY" cat-stealer
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.