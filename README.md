cat << 'EOF' > README.md
# 🤖 AI Personal Finance Tracker

[![Framework](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Frontend](https://img.shields.io/badge/Blazor-WASM-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![ML Engine](https://img.shields.io/badge/ML.NET-SDCA%20Classifier-orange?logo=dotnet)](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet)
[![Containerization](https://img.shields.io/badge/Docker-Containerized-2496ED?logo=docker)](https://www.docker.com/)
[![Testing](https://img.shields.io/badge/xUnit-100%25%20Passing-brightgreen?logo=dotnet)](https://xunit.net/)
[![Database](https://img.shields.io/badge/EF%20Core-SQLite-003B57?logo=sqlite)](https://sqlite.org/)

An enterprise-grade, local-first personal finance management system engineered with **.NET 10**, **Blazor WebAssembly**, **ASP.NET Core Web API**, and **ML.NET**. Features automated multiclass expense categorization, real-time Human-in-the-Loop ML model retraining, thread-safe model artifact persistence, and containerized deployment.

---

## 📌 Table of Contents
- [Executive Summary & Purpose](#-executive-summary--purpose)
- [The Problem & Defined Solution](#-the-problem--defined-solution)
- [Existing Systems vs. Our System](#-existing-systems-vs-our-system)
- [Tech Stack Architecture](#-tech-stack-architecture)
- [System Design & Architecture](#-system-design--architecture)
- [Implementation & How the API Connection Works](#-implementation--how-the-api-connection-works)
- [Data Pipeline & Human-in-the-Loop ML](#-data-pipeline--human-in-the-loop-ml)
- [Alignment with the 6-Phase Action Plan](#-alignment-with-the-6-phase-action-plan)
- [Enterprise Quality & Testing Suite](#-enterprise-quality--testing-suite)
- [Quick Start & Docker Deployment](#-quick-start--docker-deployment)

---

## 🎯 Executive Summary & Purpose

### What is this project for?
The **AI Personal Finance Tracker** provides automated, privacy-first personal wealth and budget tracking. It bridges the gap between static spreadsheet logging and cloud-dependent budgeting tools by executing machine learning predictions **on-device**.

**Key Capabilities:**
1. **Bulk Data Ingestion**: Import transaction CSV files via non-blocking asynchronous stream buffers (`MemoryStream`).
2. **Automated Expense Categorization**: Instantly predict expense categories using a trained **ML.NET** multiclass text classifier (`SdcaMaximumEntropy`).
3. **Continuous AI Retraining**: Implements a **Human-in-the-Loop** feedback mechanism where manual user category overrides retrain and persist the model in real time.
4. **Budget Health Monitoring**: Dynamically tracks spending limits with 80% and 100% visual alert indicators.

---

## 💡 The Problem & Defined Solution

### What main problem is it solving?
Traditional financial tracking tools suffer from two major trade-offs:
1. **Manual Entry Friction**: Spreadsheet-based tracking requires tedious line-by-line manual entry and classification, leading to user fatigue and inaccurate data.
2. **Privacy & Security Exposure**: SaaS budgeting tools (e.g., YNAB, Monarch) require paid recurring subscriptions and route sensitive, personal banking transactions through third-party aggregators to external cloud servers.

### How are we defining the solution?
A **local-first, decoupled, full-stack .NET system** hosted on private infrastructure or Docker containers. Personal financial data remains strictly within the application boundary, while an embedded machine learning engine continuously adapts to individual spending habits.

- **Privacy Model**: Local-First / Zero Data Exfiltration
- **Machine Learning**: Embedded On-Device ML.NET Classifier
- **Adaptability**: Real-Time Model Retraining on Manual Category Edit
- **Client Runtime**: Blazor WebAssembly SPA (Browser C# Execution)
- **Orchestration**: Multi-Stage Docker Container on Port 5000

---

## 📊 Existing Systems vs. Our System

### Is this product or system existing?
While budgeting apps and spreadsheets exist, traditional market offerings fail to combine **privacy**, **automation**, and **on-device ML training** into a unified open-source architecture.

| Feature | Spreadsheets (Excel / Google Sheets) | Commercial SaaS (YNAB / Mint) | Our System (AI Finance Tracker) |
| :--- | :--- | :--- | :--- |
| **Privacy & Security** | High (Local file) | Low (Third-party cloud data exposure) | **Maximum** (On-device execution, zero cloud exfiltration) |
| **Categorization** | 100% Manual | Rule-based Cloud Matching | **Automated ML.NET Multiclass Classifier** |
| **Learning Ability** | None | Static | **Human-in-the-Loop Real-Time Model Retraining** |
| **Cost** | Free / Low | $10–$15/month subscription | **$0 (Open-Source / Self-Hosted)** |
| **Deployment** | Desktop Application | Proprietary Web Portal | **Dockerized Container (.NET 10 Host)** |

---

## 🛠️ Tech Stack Architecture

| Layer | Technology | Engineering Responsibility |
| :--- | :--- | :--- |
| **Frontend UI** | **Blazor WASM (.NET 10)** | Reactive Single-Page Application (SPA) executed directly inside the browser using C#. |
| **Backend API** | **ASP.NET Core API (.NET 10)** | REST controller handling transaction CRUD, stream buffering, and health telemetry. |
| **Machine Learning** | **ML.NET Framework** | Text featurization (`FeaturizeText`) + `SdcaMaximumEntropy` classification trainer. |
| **Persistence** | **EF Core 10 + SQLite** | Relational data mapping with `WAL` journal mode and nullable foreign key relations (`int? CategoryId`). |
| **Testing** | **xUnit + In-Memory EF** | Automated unit and integration test suite asserting ML predictions and data logic. |
| **DevOps** | **Docker & Docker Compose** | Multi-stage Docker container build hosting full-stack solution on port 5000. |

---

## 🏗️ System Design & Architecture

The application implements a **Decoupled Tiered Client-Server Architecture** running inside a containerized host environment.

```text
+-------------------------------------------------------+
|             Blazor WASM Client (Browser)              |
|   - Single-Page Application (SPA)                     |
|   - Direct Mono/WASM C# Execution                     |
+---------------------------+---------------------------+
                            |
                 Async HTTP Requests (HttpClient)
                            |
                            v
+---------------------------+---------------------------+
|              ASP.NET Core Web API                     |
|   - RESTful Controllers                               |
|   - Health Monitoring Endpoint (/health)             |
+-------------+---------------------------+-------------+
              |                           |
    EF Core SQL Queries          Invoke ML Predict/Retrain
              |                           |
              v                           v
+-------------+-------------+  +----------+-------------+
|    SQLite Database        |  |  ML.NET Categorization |
|  (Local Data Storage)     |  |     Service Engine     |
+---------------------------+  +------------------------+
