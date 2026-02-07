# Student Management System (SMS)

A comprehensive student management system built with ASP.NET Core Razor Pages and PostgreSQL.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (version 12 or higher)
- A code editor (Visual Studio, VS Code, or Rider)

## Project Structure

```
student_management_system/
├── src/
│   ├── SmsRazor.DAL/          # Data Access Layer (Entities, DbContext)
│   ├── SmsRazor.BLL/          # Business Logic Layer
│   └── SmsRazor.WebApp/       # Web Application (Razor Pages)
└── infrastructure/            # Infrastructure configuration
```

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd student_management_system
```

### 2. Configure Database Connection

You have two options for database configuration:

#### Option A: Using .env file (Recommended for development)

Navigate to `src/SmsRazor.WebApp` and create a `.env` file:

```bash
cd src/SmsRazor.WebApp
cp .env.sample .env
```

Edit the `.env` file with your PostgreSQL credentials:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=sms-db
DB_USER=postgres
DB_PASS=your_password_here
```

#### Option B: Using appsettings.json

Edit `src/SmsRazor.WebApp/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sms-db;Username=postgres;Password=your_password_here"
  }
}
```

> **Note**: The application prioritizes `.env` if it exists, otherwise falls back to `appsettings.json`. For EF migrations, update the connection string in `appsettings.json`.

### 3. Install Dependencies

From the `src` directory, restore all NuGet packages:

```bash
cd src
dotnet restore
```

### 4. Ensure PostgreSQL is Running

Make sure your PostgreSQL server is running and accessible:

**Windows (if installed as service):**
```bash
# Check if PostgreSQL is running
sc query postgresql-x64-<version>

# Start if not running
net start postgresql-x64-<version>
```

**Linux:**
```bash
sudo systemctl status postgresql
sudo systemctl start postgresql
```

**macOS:**
```bash
brew services list
brew services start postgresql
```

### 5. Create the Database

**Important**: Update the connection string in `appsettings.json` before running migrations, as EF tools read from configuration files, not `.env`.

Apply the migrations from the `SmsRazor.WebApp` directory:

```bash
cd src/SmsRazor.WebApp
dotnet ef database update --project ..\SmsRazor.DAL\SmsRazor.DAL.csproj
```

This will create the `sms-db` database with all required tables.

### 6. Run the Application

```bash
dotnet run
```

The application will start and be available at `http://localhost:5205` (or the port specified in `launchSettings.json`).

## Database Schema

The system includes the following entities:

- **Role**: User roles and permissions
- **Account**: User accounts with authentication
- **StudentInfo**: Student records and academic information
- **AdminInfo**: Administrative staff information
- **TeacherInfo**: Teacher/instructor information
- **Department**: Academic departments
- **Intake**: Student intake/cohort information
- **Syllabus**: Course syllabi
- **StudentStatus**: Student enrollment status

All entities automatically track creation and modification timestamps.

## Development

### Adding New Migrations

After modifying entities, create a new migration:

```bash
cd src/SmsRazor.WebApp
dotnet ef migrations add <MigrationName> --project ..\SmsRazor.DAL\SmsRazor.DAL.csproj
```

Apply the migration:

```bash
dotnet ef database update --project ..\SmsRazor.DAL\SmsRazor.DAL.csproj
```

### Reverting Migrations

To remove the last migration:

```bash
dotnet ef migrations remove --project ..\SmsRazor.DAL\SmsRazor.DAL.csproj
```

### Building the Solution

```bash
cd src
dotnet build
```

## Troubleshooting

### PostgreSQL Not Running

**Error**: `Failed to connect to 127.0.0.1:5432` or `No connection could be made because the target machine actively refused it`

**Solution**: Ensure PostgreSQL is running (see step 4 above).

### Port Already in Use

If you encounter "address already in use" errors:

**Windows:**
```bash
netstat -ano | findstr :<port>
taskkill /F /PID <process_id>
```

**Linux/Mac:**
```bash
lsof -i :<port>
kill -9 <process_id>
```

### Database Connection Issues

1. Verify PostgreSQL is running
2. Check credentials in `.env` or `appsettings.json`
3. Ensure the database user has proper permissions
4. Test connection: `psql -h localhost -U postgres -d sms-db`

### Migration Errors

If migrations fail:
1. Check database connectivity
2. Ensure `appsettings.json` has the correct connection string (EF tools don't read `.env`)
3. Verify no other process is using the database
4. Check entity configurations in `SmsDbContext`

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `DB_HOST` | PostgreSQL host | `localhost` |
| `DB_PORT` | PostgreSQL port | `5432` |
| `DB_NAME` | Database name | `sms-db` |
| `DB_USER` | Database user | `postgres` |
| `DB_PASS` | Database password | - |


## Docker Support

### Build and Run with Docker

1. **Build the image**:
   ```bash
   docker build -t sms-app .
   ```

2. **Run the container**:
   ```bash
   docker run -d -p 8080:80 --name sms-container sms-app
   ```
   The application will be accessible at `http://localhost:8080`.

> **Note**: For the Docker container to connect to your local PostgreSQL, you may need to adjust the connection string or use Docker Compose.


## Contributing

[Your Contributing Guidelines Here]
