# AI Personal Finance Tracker

AI Personal Finance Tracker is a privacy-focused personal finance platform for recording transactions, managing budgets and savings goals, understanding recurring costs, and turning financial history into useful planning insights.

The project is a full-stack .NET application with a Blazor WebAssembly client, ASP.NET Core API, SQLite persistence, and ML.NET services. It is designed to run locally or as a containerized web application.

## Features

### Account and profile management

- Registration with name, email, country, currency, password, and password confirmation.
- Secure salted PBKDF2 password hashing.
- Login and session-based client authentication.
- Password recovery with expiring reset tokens.
- Profile preferences for country, time zone, base currency, monthly budget, and goal allocation.
- International currency selection with live conversion when the profile currency changes.

> The development password-reset flow displays the reset token in the response. A production deployment should connect this flow to a secure email delivery provider and should not expose tokens in the UI.

### Transactions

- Manual transaction entry for income and expenses.
- Natural-language Smart Add, for example:
  `Spent 450 INR on groceries at Reliance Fresh yesterday`.
- ML-assisted category prediction.
- Recurring monthly transactions.
- Search, category filtering, type filtering, deletion, and CSV export.
- One unified upload control:
  - Receipt image, PDF, text, JSON, or supported document files are sent through receipt extraction.
  - CSV/XLSX files are imported as transaction history.
- Anomaly detection during transaction processing.

### Financial intelligence

- Dashboard balance, income, expense, category, and budget views.
- ML.NET SSA cash-flow forecasting.
- Forecasted month-end balance and spending burn rate.
- Multi-factor anomaly risk scores from 0 to 100.
- Context-aware advisor responses based on transactions, categories, and savings goals.
- Subscription and recurring-payment analysis.
- Monthly and annual fixed-cost burn-rate estimates.
- Upcoming renewal estimates.
- Interactive Chart.js spending visualizations.
- Downloadable QuestPDF monthly financial reports.

### Goals and collaboration

- Savings goals with target and current amounts.
- Automatic allocation of eligible income toward active goals.
- Shared family or team budget workspaces.
- Viewer and Contributor workspace roles.
- Workspace member invitations.

### User experience

- Responsive Blazor WebAssembly interface.
- Light and dark themes.
- International currency formatting.
- Dedicated About, Reports, Subscriptions, Advisor, Goals, Profile, and Shared Budgets pages.
- Accessible labels, validation messages, loading states, and explicit error feedback.

## Technology stack

| Area | Technology |
| --- | --- |
| Frontend | Blazor WebAssembly, Razor components, Bootstrap, custom CSS |
| Backend | ASP.NET Core Web API on .NET 10 |
| Shared contracts | .NET shared models and DTOs |
| Persistence | Entity Framework Core 10 with SQLite |
| Machine learning | ML.NET categorization, anomaly detection, and SSA forecasting |
| Charts | Chart.js |
| PDF reports | QuestPDF |
| Testing | xUnit and in-memory/integration test coverage |
| Deployment | Docker and Docker Compose |
| External integration | Frankfurter exchange-rate API |

## Repository structure

```text
AIPersonalFinanceTracker.Api/       ASP.NET Core API, controllers, database, services
AIPersonalFinanceTracker.Client/    Blazor WebAssembly UI, pages, layout, browser assets
AIPersonalFinanceTracker.ML/        ML.NET prediction and forecasting services
AIPersonalFinanceTracker.Shared/    Shared entities, DTOs, and request models
AIPersonalFinanceTracker.Tests/     Automated tests
Dockerfile                           Multi-stage container build
docker-compose.yml                   Local/container orchestration
AIPersonalFinanceTracker.slnx        Solution file
```

## Architecture

```text
Browser
  │
  │ Blazor WebAssembly and HttpClient
  ▼
ASP.NET Core API
  ├── Authentication and profile controllers
  ├── Transaction, goal, budget, report, and analytics controllers
  ├── Exchange-rate and receipt services
  └── ML.NET services
          │
          ├── Category prediction
          ├── Anomaly scoring
          └── Cash-flow forecasting
  │
  ▼
Entity Framework Core + SQLite
```

The client uses relative `api/...` URLs and normally runs with the ASP.NET Core host serving both the API and the compiled WebAssembly assets.

## Main application routes

| Route | Purpose |
| --- | --- |
| `/` | Main financial dashboard |
| `/login` | Sign in |
| `/register` | Create an account |
| `/forgot-password` | Request and complete password recovery |
| `/transactions` | Add, import, search, filter, and export transactions |
| `/profile` | Manage profile and currency preferences |
| `/goals` | Manage savings goals |
| `/advisor` | Ask questions about financial history |
| `/subscriptions` | Review recurring payments and fixed-cost burn rate |
| `/reports` | Download monthly PDF reports |
| `/budgets` | Create shared workspaces and invite members |
| `/about` | Platform overview and privacy information |

## Requirements

- .NET SDK 10
- Git
- Optional: Docker and Docker Compose

Check the installed SDK:

```bash
dotnet --version
```

## Run locally

Restore dependencies:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build AIPersonalFinanceTracker.slnx
```

Run the API and hosted Blazor application:

```bash
dotnet run --project AIPersonalFinanceTracker.Api/AIPersonalFinanceTracker.Api.csproj
```

The default development profile uses:

```text
http://localhost:5000
```

To use another port:

```bash
dotnet run \
  --project AIPersonalFinanceTracker.Api/AIPersonalFinanceTracker.Api.csproj \
  --urls http://localhost:5001
```

The application exposes a health endpoint at `/health`.

## Run tests

```bash
dotnet test AIPersonalFinanceTracker.Tests/AIPersonalFinanceTracker.Tests.csproj
```

The test suite covers API behavior, Smart Add parsing, ML-related behavior, and financial services.

## Configuration and data

The API uses SQLite by default:

```text
Data Source=finance.db
```

The connection string can be overridden with configuration:

```bash
ConnectionStrings__DefaultConnection="Data Source=/path/to/finance.db"
```

For containers, persistent storage should be mounted for the SQLite database and generated ML/report files. Do not rely on ephemeral container storage for real financial data.

The exchange-rate integration uses the Frankfurter API and refreshes rates in the background. If the external service is unavailable, the application reports conversion failures rather than silently presenting incorrect values.

## Docker

Build and run with Docker Compose:

```bash
docker compose up --build
```

The compose configuration publishes the application on port `5000`. For production, configure:

- Persistent database volume.
- HTTPS and a trusted reverse proxy.
- Secure application settings and secrets.
- Production email delivery for password reset.
- Appropriate CORS and authentication policies.
- Backups and monitoring.

## Deployment guidance

This is not a static-only website. GitHub Pages cannot host the complete application because it cannot run:

- ASP.NET Core API endpoints.
- SQLite persistence.
- Authentication and password reset logic.
- ML.NET services.
- Receipt processing.
- PDF generation.

Deploy the complete application to a backend-capable platform such as Azure App Service, Render, Railway, Fly.io, AWS, or a Docker-capable VPS. A split deployment with the Blazor client on GitHub Pages and the API elsewhere is possible, but requires an API base URL, CORS configuration, persistent database hosting, and Blazor SPA fallback configuration.

## Security and privacy notes

- Passwords are stored as salted PBKDF2 hashes, never as plaintext.
- Financial data is persisted by the application database.
- The advisor is an application insight tool, not an investment, tax, legal, or regulated financial advisor.
- Use HTTPS in any shared or public environment.
- Replace development password-reset token delivery before production.
- Protect the SQLite file and generated ML/report artifacts.
- Review the dependency security warnings from `dotnet build` before production deployment.

## Development conventions

- Keep business logic in API or ML services rather than duplicating it in Razor markup.
- Reuse shared DTOs and models for client/API contracts.
- Use explicit validation and user-visible error messages.
- Preserve the selected profile currency across dashboard, transactions, goals, reports, and analytics.
- Run the build and targeted tests before opening a pull request.

## Project status

The application currently provides:

- Full transaction and budget workflow.
- International currency selection and conversion.
- ML-backed categorization, anomaly scoring, and forecasting.
- Receipt and history import.
- Advisor, subscriptions, reports, goals, and shared budgets.
- Responsive light/dark user interface.
- Password recovery and platform documentation.

## License and ownership

Review the repository license and organization policies before distributing or deploying the application publicly.
