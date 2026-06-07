```markdown
# Enterprise Task Management System

A full-stack, enterprise-grade Task Management application built with a React (Vite) frontend and a secure C# .NET Core REST API.

## Prerequisites

To run this application locally, you will need the following installed on your machine:
* Node.js (v18 or higher)
* .NET 10.0 SDK (or newer)
* SQL Server (Express, Developer)
* A code editor like VS Code or Visual Studio
* Git

---

## Getting Started

First, clone the repository to your local machine and navigate into the root project directory:

```bash
git clone [https://github.com/TalhaYounas6/Task-Management-Tool-10Pearls-.git](https://github.com/TalhaYounas6/Task-Management-Tool-10Pearls-.git)
cd Task-Management-Tool-10Pearls-

```


---

## Backend Setup (.NET API)

The backend is configured to automatically restore all required NuGet packages (like Entity Framework, Serilog, and DotNetEnv) the first time you run or build the project.

### 1. Navigate to the API Directory

Ensure your terminal is in the root `Task-Management-Tool-10Pearls-` folder, then navigate into the backend directory:

```bash
cd TaskManagement.API

```

### 2. Database Configuration

1. Open `appsettings.json` (located inside the `TaskManagement.API` folder).
2. Locate the `ConnectionStrings` section.
3. Update the `DefaultConnection` string to point to your local SQL Server instance. 

### 3. Environment Variables (Security)

This API uses `.env` files to securely manage the JWT signing keys outside of source control.

1. Ensure your terminal is still inside the `TaskManagement.API` folder.
2. Create a new file named exactly `.env`.
3. Add the following line, replacing the placeholder with a secure string of your choice (at least 256 bits/32 characters long):
```env
JWT_KEY=your-super-secure-secret-key-goes-here!

```

### 4. Database Migrations

Ensure your local SQL Server is running. From inside the `TaskManagement.API` folder, run the following command to automatically generate the database schema and tables:

```bash
dotnet ef database update

```

### 5. Run the API

Start the server using the .NET CLI:

```bash
dotnet run

```

*The API will start (https://localhost:7169). You can append `/swagger` to the URL to view and test the API endpoints directly in your browser.*

---

## Frontend Setup (React / Vite)

The frontend is a modern React application built with Vite.

### 1. Navigate to the Client Directory

Open a *new* terminal window, ensure you are in the root `Task-Management-Tool-10Pearls-` folder, and navigate to the React directory:

```bash
cd client

```

### 2. Install Dependencies

Install the required Node packages:

```bash
npm install

```

### 3. Run the Development Server

Start the Vite development server:

```bash
npm run dev

```

*The React app will open in your default browser (typically at http://localhost:5173).*

---

## Admin Access

To test the role-based access control (RBAC) and administrative features, you can log in using the following pre-configured admin credentials:

* **Admin Email:** user@example.com
* **Admin Password:** Abc123!

---

## Code Quality & Architecture

* **Security:** JWT Authentication with Role-Based Access Control (RBAC). Secrets are explicitly decoupled from source control using environment variables.
* **Static Analysis:** Fully scanned and verified using SonarQube/SonarCloud.
* **Logging:** Integrated Serilog for rolling file and console logs.
* **API Documentation:** Fully interactive Swagger UI implementation with Bearer Token support.

```

```