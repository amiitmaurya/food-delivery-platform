# 🍔 Food Delivery Platform

A full-stack food delivery platform inspired by modern applications like Swiggy and Zomato. This project is built using **ASP.NET Core, .NET 10, Angular, SQL Server, and Entity Framework Core**, following production-oriented software architecture and development practices.

> 🚧 **Project Status:** In Development

---

## 📌 Overview

**Food Delivery Platform** is a full-stack web application designed to provide an online food-ordering experience.

The project includes user authentication, restaurant management, food/menu management, cart functionality, and a scalable backend architecture.

The application is divided into two major parts:

* **Backend:** ASP.NET Core Web API
* **Frontend:** Angular

The backend follows **Clean Architecture, Repository Pattern, Unit of Work, SOLID, and DRY principles**.

---

## ✨ Features

### 👤 Authentication & Authorization

* User Registration
* User Login
* JWT Authentication
* Role-Based Authorization
* Password Hashing
* Protected API Endpoints
* Server-Side Validation

### 🍽️ Restaurant Management

* Restaurant Management
* Restaurant Categories
* Food Item Management
* Restaurant Listing
* Menu Management

### 🛒 Cart Management

* Add Food Item to Cart
* Update Cart Item Quantity
* Remove Cart Item
* View Current Cart
* Cart & Cart Item Relationship Management

### 🖥️ Frontend

* Angular SPA
* Login & Registration
* Dashboard
* Restaurant Listing
* Food/Menu UI
* Cart UI
* Reusable Components
* Authentication State Management
* Responsive UI

---

# 🏗️ Architecture

The backend follows **Clean Architecture** principles.

```text
Food Delivery Platform
│
├── MiniSwiggy.API
│   ├── Controllers
│   ├── Middleware
│   └── API Configuration
│
├── MiniSwiggy.Application
│   ├── DTOs
│   ├── Interfaces
│   └── Application Services
│
├── MiniSwiggy.Domain
│   ├── Entities
│   └── Domain Models
│
├── MiniSwiggy.Infrastructure
│   ├── DbContext
│   ├── Repositories
│   ├── Unit of Work
│   ├── EF Core
│   └── Infrastructure Services
│
└── MiniSwiggy.Shared
    └── Common Components
```

### Architecture Flow

```text
Angular Frontend
       │
       ▼
ASP.NET Core Web API
       │
       ▼
Application Layer
       │
       ▼
Domain Layer
       │
       ▼
Infrastructure Layer
       │
       ├── Repository
       ├── Unit of Work
       └── Entity Framework Core
       │
       ▼
SQL Server
```

---

# 🛠️ Technology Stack

## Backend

| Technology            | Purpose              |
| --------------------- | -------------------- |
| C#                    | Programming Language |
| .NET 10               | Backend Framework    |
| ASP.NET Core          | Web API              |
| Entity Framework Core | ORM                  |
| SQL Server            | Database             |
| LINQ                  | Data Querying        |
| REST API              | API Architecture     |
| JWT                   | Authentication       |
| Swagger / OpenAPI     | API Documentation    |

## Frontend

| Technology | Purpose              |
| ---------- | -------------------- |
| Angular 22 | Frontend Framework   |
| TypeScript | Programming Language |
| HTML5      | UI Structure         |
| CSS3       | Styling              |
| Bootstrap  | UI Design            |
| RxJS       | Reactive Programming |

## Development Tools

* Visual Studio
* Visual Studio Code
* Git
* GitHub
* Postman
* Swagger

---

# 📂 Project Structure

```text
food-delivery-platform/
│
├── MiniSwiggy/
│   ├── MiniSwiggy.API/
│   ├── MiniSwiggy.Application/
│   ├── MiniSwiggy.Domain/
│   ├── MiniSwiggy.Infrastructure/
│   └── MiniSwiggy.Shared/
│
├── MiniSwiggyUI/
│   └── Angular Frontend
│
└── README.md
```

---

# 🗄️ Database

The application uses **Microsoft SQL Server** with **Entity Framework Core**.

### Main Entities

* User
* Role
* Restaurant
* Category
* FoodItem
* Cart
* CartItem

### Entity Relationships

```text
Role
 └── User

Restaurant
 ├── Category
 └── FoodItem

Category
 └── FoodItem

User
 └── Cart
      └── CartItem
           └── FoodItem
```

---

# 🔐 Authentication

The application uses **JWT Bearer Authentication**.

### Authentication Flow

```text
Register
   ↓
Login
   ↓
JWT Token Generated
   ↓
Token Stored by Client
   ↓
Authorization Header
   ↓
Protected API
```

Example:

```http
Authorization: Bearer <your-jwt-token>
```

---

# 🚀 Getting Started

## Prerequisites

Make sure you have the following installed:

* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Node.js](https://nodejs.org/)
* Angular CLI
* SQL Server
* SQL Server Management Studio
* Git

---

## 1️⃣ Clone Repository

```bash
git clone https://github.com/amiitmaurya/food-delivery-platform.git
```

```bash
cd food-delivery-platform
```

---

# 🔹 Backend Setup

Navigate to the backend:

```bash
cd MiniSwiggy
```

Restore NuGet packages:

```bash
dotnet restore
```

Build the project:

```bash
dotnet build
```

---

# 🔹 Database Configuration

Update your SQL Server connection string in the backend configuration.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=FoodDeliveryDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> ⚠️ Never commit real database passwords, JWT secrets, API keys, or other sensitive information to GitHub.

---

# 🔹 Entity Framework Migration

Install EF Core CLI if required:

```bash
dotnet tool install --global dotnet-ef
```

Apply database migrations:

```bash
dotnet ef database update
```

---

# 🔹 Run Backend

```bash
dotnet run
```

Swagger is available at:

```text
https://localhost:<port>/swagger
```

---

# 🔹 Frontend Setup

Open another terminal and navigate to the Angular application:

```bash
cd MiniSwiggyUI
```

Install dependencies:

```bash
npm install
```

Run Angular:

```bash
ng serve
```

Open:

```text
http://localhost:4200
```

---

# 🔌 API Modules

The backend is organized around different application modules:

```text
Authentication
Restaurants
Categories
Food Items
Cart
Users / Profiles
Orders
Payments
Reviews
Delivery Partners
```

Some modules are currently under development.

---

# 🧪 API Testing

The API can be tested using:

### Swagger

Swagger/OpenAPI provides an interactive interface for testing API endpoints.

### Postman

Postman can be used for:

* Registration
* Login
* JWT Authentication
* Restaurant APIs
* Category APIs
* Food Item APIs
* Cart APIs
* Order APIs

For protected endpoints, provide the JWT token as a Bearer token.

---

# 🧩 Design Principles

This project follows modern software engineering principles.

### Clean Architecture

Separates business logic from API, database, and infrastructure concerns.

### Repository Pattern

Provides abstraction between application logic and data-access logic.

### Unit of Work

Coordinates multiple repository operations and database transactions.

### SOLID Principles

The project follows:

* **S** — Single Responsibility Principle
* **O** — Open/Closed Principle
* **L** — Liskov Substitution Principle
* **I** — Interface Segregation Principle
* **D** — Dependency Inversion Principle

### DRY

Common functionality is implemented using reusable components and services to avoid unnecessary duplication.

---

# 🔮 Planned Features

* [ ] Complete Order Management
* [ ] Order Status Tracking
* [ ] Delivery Partner Management
* [ ] Admin Dashboard
* [ ] Restaurant Dashboard
* [ ] Payment Integration
* [ ] Order History
* [ ] Reviews & Ratings
* [ ] Wishlist
* [ ] Coupons & Offers
* [ ] Search & Filtering
* [ ] Real-Time Order Tracking
* [ ] Automated Unit Testing
* [ ] Integration Testing
* [ ] CI/CD Pipeline
* [ ] Production Deployment

---

# 📸 Screenshots

Screenshots will be added as the project UI continues to evolve.

```text
docs/
└── screenshots/
    ├── login.png
    ├── register.png
    ├── dashboard.png
    ├── restaurants.png
    └── cart.png
```

---

# 🤝 Contributing

Contributions and suggestions are welcome.

### 1. Fork the repository

### 2. Create a feature branch

```bash
git checkout -b feature/your-feature
```

### 3. Commit your changes

```bash
git add .
git commit -m "Add your feature"
```

### 4. Push your branch

```bash
git push origin feature/your-feature
```

### 5. Create a Pull Request

---

# 📄 License

This project is currently intended for **learning, portfolio, and development purposes**.

A formal open-source license can be added when the project is ready for public contribution.

---

# 👨‍💻 Author

## Amit Maurya

**.NET Backend / Full Stack Developer**

GitHub:
https://github.com/amiitmaurya

---

⭐ **If you find this project useful, consider giving the repository a star!**

### 🚀 Food Delivery Platform

**Built with ASP.NET Core + .NET 10 + Angular + SQL Server + Entity Framework Core**
