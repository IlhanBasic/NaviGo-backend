# NaviGo API

NaviGo API is a **logistics management platform** built with **ASP.NET Core**, implementing **Clean Architecture** principles.  
It supports **multi-database integration** (PostgreSQL, MongoDB, Neo4j) and provides advanced features for handling logistics operations, penalties, and user/session management.

---

## ✨ Features

- **Clean Architecture** with Domain, Application, Infrastructure, and API layers.
- **Authentication & Authorization**
  - JWT authentication
  - Google OAuth support
  - Role-based access control
- **CQRS with MediatR** for structured request handling.
- **Repository & Unit of Work** patterns for clean data access.
- **Entity Framework Core** integration with PostgreSQL.
- **Multi-database support**
  - PostgreSQL – primary relational storage
  - MongoDB – for flexible, document-based storage
  - Neo4j – for graph-based relationships
- **Automapper** for DTO <-> Entity mapping.
- **Swagger** for interactive API documentation.
- **Server-side filtering, sorting & paging** on all endpoints.
- **Custom Middlewares** (Geo-Location Validation, Session Lock).

---

## 📘 Business Domain

The **NaviGo platform** is designed for logistics and transportation companies, freight forwarders, and clients.  
It provides a unified system for managing transport offers, shipments, and penalties for delivery delays.

### 🛡️ User Roles

- **Transport Companies**
  - Manage vehicle fleets and routes.
  - Accept or reject shipping offers.
  - Handle on-time delivery and penalties.
- **Freight Forwarders**
  - Act as intermediaries between clients and transport companies.
  - Submit and negotiate offers.
  - Track delivery status and ensure compliance.
- **Clients**
  - Create shipment requests.
  - Track real-time shipment status.
  - Receive invoices and penalty refunds in case of delays.

### ⚙️ Core Functionalities

- **Offer Management** – Transporters and forwarders can create, accept, or reject offers.
- **Shipment Tracking** – Real-time delivery updates.
- **Delay Penalty System** – Automatic calculation of penalties in case of late deliveries, including refund logic.
- **Company & User Management** – Manage users, roles, and companies across multiple databases.
- **Secure Authentication** – Session lock middleware ensures user safety, while JWT & Google Auth provide flexible login options.

---

## 🌍Custom Middleware

The project includes **custom-built middlewares** to extend the security and reliability of the API.

### Geo-Location Validation Middleware
- Ensures that certain requests are only accepted from specific regions.
- Can be used to prevent fraud or enforce regional compliance rules.

### Session Lock Middleware
- Automatically locks inactive user sessions after a configurable interval (default: 5 minutes).
- Prevents unauthorized usage of abandoned sessions.
- Can trigger alerts or actions to notify users of forced logouts.

---

## 💻 Technology Stack

- **Backend:** ASP.NET Core 8, C#
- **Databases:**
  - PostgreSQL (Relational)
  - MongoDB (Document-based)
  - Neo4j (Graph)
- **ORM & Data Access:** Entity Framework Core, Sequelize (for existing schemas)
- **Messaging & Patterns:** CQRS, MediatR, Repository, Unit of Work
- **Authentication:** JWT, Google OAuth
- **Mapping:** AutoMapper
- **Documentation:** Swagger

---
## 📂Project Structure
```
NaviGo-backend/
│── NaviGoApi/              # API Layer (Controllers, Middleware, Routing)
│── NaviGoApi.Application/  # Application Layer (CQRS, DTOs, Services)
│── NaviGoApi.Domain/       # Domain Layer (Entities, Interfaces, Enums)
│── NaviGoApi.Infrastructure/ # Infrastructure Layer (DB Repositories, Configurations)
│── NaviGoApi.Tests/        # Unit and Integration Tests
│── README.md               # Project Documentation
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL, MongoDB, and Neo4j running locally or in Docker
- Visual Studio / VS Code

### Installation
```bash
git clone https://github.com/IlhanBasic/NaviGo-backend.git
cd NaviGo-backend
dotnet restore
dotnet build
dotnet run
```
---

## 📝 API Documentation

Interactive Swagger documentation is available at:

```
https://localhost:7028/swagger
```

## ⚖️ License

This project is licensed under the MIT License.  
See the LICENSE file for details.

---

© 2025 NaviGo API – Logistics Management Platform
