# 🎓 Student Management System (SMS)

A modern, comprehensive student management system built with **ASP.NET Core Razor Pages**, **Entity Framework Core**, and **PostgreSQL**. The application features a robust N-Tier architecture and integrates advanced AI capabilities and real-time communication to provide a seamless educational management experience.

---

## ✨ Features

- **🔐 Role-Based Access Control**: Secure portals and specialized permissions for Students, Teachers, and Administrators.
- **👥 Student Profile Management**: Full CRUD operations for student accounts, including administrative actions like password resets and profile updates.
- **📚 Course & Registration System**: 
  - Students can easily browse course syllabi and view their personalized timetables.
  - Smart course registration system allowing enrollment for up to 5 courses per term.
  - Automated scheduling conflict prevention and duplicate registration checks.
- **🤖 AI Student Assistant**: An intelligent chatbot powered by **Microsoft Foundry SDK** (Azure AI). It provides students with real-time advisory on timetables, course details, study assistance, event notifications, and psychological support.
- **💬 Real-time Communication**: Interactive chat features built on **SignalR**. Includes real-time typing indicators and inline media previews (PDFs, Videos) for enhanced collaboration.
- **🏗️ Clean N-Tier Architecture**: Strict separation of concerns across the Web Application (Razor Pages), Business Logic Layer (BLL), and Data Access Layer (DAL).

---

## 🛠️ Technology Stack

- **Framework**: .NET 10 / ASP.NET Core Razor Pages
- **Database**: PostgreSQL (v12+)
- **ORM**: Entity Framework (EF) Core
- **Real-Time Communication**: SignalR
- **AI Integration**: Microsoft Foundry SDK
- **Frontend**: Bootstrap, Vanilla CSS & JS

---

## 📁 Project Structure

The project follows a clean N-Tier architecture for maintainability and scalability:

```text
spring26-student-management-system/
├── src/
│   ├── SmsRazor.DAL/          # Data Access Layer (Entities, DbContext, Migrations)
│   ├── SmsRazor.BLL/          # Business Logic Layer (Services, Validation logic)
│   └── SmsRazor.WebApp/       # Web Application (Razor Pages, SignalR Hubs, UI)
├── infrastructure/            # Infrastructure & Deployment configurations (e.g., Bicep/Terraform)
└── testproj/                  # Unit and Integration Tests
```

---

## 🚀 Getting Started

### 1. Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (version 12 or higher)
- Visual Studio 2022, VS Code, or JetBrains Rider

### 2. Clone the Repository

```bash
git clone <repository-url>
cd spring26-student-management-system
```

### 3. Configure Database Connection

You have two options for configuring the database connection:

#### Option A: Using `.env` file (Recommended for development)

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

#### Option B: Using `appsettings.json`

Edit `src/SmsRazor.WebApp/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sms-db;Username=postgres;Password=your_password_here"
  }
}
```

> **Note**: The application prioritizes `.env` if it exists, otherwise it falls back to `appsettings.json`. For running EF migrations from the CLI, ensure the connection string is correctly set in `appsettings.json`.

### 4. Install Dependencies

From the `src` directory, restore all NuGet packages:

```bash
cd src
dotnet restore
```

### 5. Create the Database & Apply Migrations

Ensure your PostgreSQL server is running. Then apply the migrations to create the database schema:

```bash
cd src/SmsRazor.WebApp
dotnet ef database update --project ..\SmsRazor.DAL\SmsRazor.DAL.csproj
```

This will create the `sms-db` database and all necessary tables (Roles, Accounts, StudentInfo, Syllabi, Enrollments, etc.).

### 6. Run the Application

```bash
dotnet run
```

The application will launch and be available at the URL specified in your `launchSettings.json` (e.g., `http://localhost:5205`).

---

## 💾 Database Schema Overview

The core entities modeled in the Data Access Layer include:
- **Role & Account**: Handles authentication and authorization.
- **StudentInfo, AdminInfo, TeacherInfo**: Stores core user profiles and personal information.
- **Department & Intake**: Manages academic organization and batch tracking.
- **Syllabus & Course**: Defines academic offerings.
- **StudentStatus & Enrollment**: Tracks course registration and student progress.

All tables automatically track creation and modification timestamps for auditing.

---

## 🧑‍💻 Development Guide

### Adding New Migrations

After modifying entities in `SmsRazor.DAL`, create a new migration:

```bash
cd src/SmsRazor.WebApp
dotnet ef migrations add <MigrationName> --project ..\SmsRazor.DAL\SmsRazor.DAL.csproj
```

### Applying Migrations

```bash
dotnet ef database update --project ..\SmsRazor.DAL\SmsRazor.DAL.csproj
```

### Reverting Migrations

To remove the last unapplied migration:

```bash
dotnet ef migrations remove --project ..\SmsRazor.DAL\SmsRazor.DAL.csproj
```

### Building the Solution

```bash
cd src
dotnet build
```

---

## 🐳 Docker Support

You can easily containerize the application for consistent deployment:

1. **Build the image**:
   ```bash
   docker build -t sms-app .
   ```

2. **Run the container**:
   ```bash
   docker run -d -p 8080:80 --name sms-container sms-app
   ```
   Access the app at `http://localhost:8080`.

> **Note**: To connect the Docker container to your local PostgreSQL instance, you may need to adjust the DB host in your connection string (e.g., change `localhost` to `host.docker.internal`).

---

## ❓ Troubleshooting

- **Database Connection Issues**: Verify credentials in `.env` or `appsettings.json`. Ensure PostgreSQL is running and accessible on the specified port.
- **Port Already in Use**: Change the application port in `Properties/launchSettings.json` or terminate the conflicting process using `netstat` and `taskkill` (Windows) or `lsof` and `kill` (Mac/Linux).
- **Migration Errors**: Ensure `appsettings.json` has the correct connection string (EF CLI tools primarily read from `appsettings.json` rather than `.env`). Check verify that no other process is locking the database.
