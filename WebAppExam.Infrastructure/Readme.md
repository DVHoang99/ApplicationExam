# WebAppExam.Infrastructure

## ⚙️ Libraries and Tools Used
- dotnet add package Microsoft.EntityFrameworkCore
- dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
- dotnet add package Microsoft.EntityFrameworkCore.Design
- dotnet add package Microsoft.EntityFrameworkCore.Tools
- dotnet add package StackExchange.Redis

WebAppExam.Infrastructure
│
├── Persistence
│    ├── AppDbContext.cs
│    ├── Migrations/
│    ├── Configurations/
│
├── Repositories
│    ├── OrderRepository.cs
│    ├── ProductRepository.cs
│
├── UnitOfWork
│    ├── UnitOfWork.cs