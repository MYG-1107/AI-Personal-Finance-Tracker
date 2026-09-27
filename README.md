# AI Personal Finance Tracker

[![Framework](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Frontend](https://img.shields.io/badge/Blazor-WASM-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![ML Engine](https://img.shields.io/badge/ML.NET-SDCA-orange?logo=dotnet)](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet)
[![Testing](https://img.shields.io/badge/xUnit-100%25%20Passing-brightgreen?logo=dotnet)](https://xunit.net/)
[![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED?logo=docker)](https://www.docker.com/)

An enterprise-grade, privacy-first personal finance application built with **.NET 10**, **Blazor WebAssembly**, **ASP.NET Core Web API**, and **ML.NET**. It performs local, on-device expense categorization and continuous machine learning retraining without exposing sensitive financial data to cloud servers.

---

## ⚡ Key Engineering Highlights

- **Local-First On-Device Machine Learning**: Uses an embedded **ML.NET** `SdcaMaximumEntropy` text classifier to categorize expense descriptions locally with zero external API calls or subscription costs.
- **Human-in-the-Loop Continuous Learning**: When a user manually overrides a category, the system retrains the model in real time and serializes the state to disk (`model.zip`) using thread-safe static locking (`lock (_fileLock)`).
- **Asynchronous Non-Blocking I/O**: Processes bulk CSV transaction imports via an in-memory stream buffer (`MemoryStream`), preventing WebAssembly UI thread freeze.
- **Enterprise Testing & Observability**: Covered by an **xUnit** integration test suite (100% pass rate) with a production health endpoint (`/health`).

---

## 🛠️ Tech Stack

| Component | Technology | Role |
| :--- | :--- | :--- |
| **Frontend** | **Blazor WebAssembly (.NET 10)** | Single-Page Application executing C# directly in the browser. |
| **Backend API** | **ASP.NET Core Web API (.NET 10)** | REST API controllers for transaction CRUD, CSV ingestion, and health telemetry. |
| **Machine Learning** | **ML.NET Framework** | On-device text featurization and multiclass classification engine. |
| **Database** | **EF Core 10 + SQLite** | Relational data persistence with `WAL` journal mode and nullable foreign keys (`int? CategoryId`). |
| **Testing** | **xUnit + In-Memory EF** | Unit and integration test suite for API endpoints and ML prediction accuracy. |
| **DevOps** | **Docker & Docker Compose** | Multi-stage container deployment hosted on port 5000. |

---

## 🏗️ Architecture & Data Flow

```text
[ User Interface (Blazor WASM) ]
               │
      Async REST Requests (HttpClient)
               │
               ▼
[ ASP.NET Core Web API Controller ]
       │                      │
       │ (Database Query)     │ (Predict / Retrain)
       ▼                      ▼
[ EF Core + SQLite ]   [ ML.NET Engine ] ──► [ Thread-Safe model.zip ]
```

---

Developed by:

Puppireddy Vishwateja - System Architect <br>
Padakanti Sairam - Backend Developer <br>
Mallarapu Yaswanth - ML and Principle Engineer
Apple 
