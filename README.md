# Fraud Rule Engine Service

Problem overview
----------------
This service ingests transaction events, evaluates them against a set of fraud rules, persists findings, and exposes results via an API. 
It's intended for banks and financial-services teams that need a deterministic, configurable rule engine with a clear separation of concerns for auditability and testability.

Architecture diagram
--------------------
Client / Test Harness
  ↓
ASP.NET Web API (thin controllers)
  ↓
Mediator (Use Cases / Commands / Queries)
  ↓
Application Services (Rule Orchestration)
  ↓
Domain (Entities, Rules, Risk Model)
  ↓
Infrastructure (MongoDB, Logging, Caching)

Why this architecture
---------------------
- Clean Architecture separates boundaries and makes rules auditable/testable without forcing infrastructure concerns into the core logic.
- Mediator (MediatR) centralizes use-cases and keeps controllers thin; makes it easy to add cross-cutting behaviors (logging, metrics, retries) via pipeline behaviors.
- An orchestration service contains the rule orchestration (pure logic) so you can swap persistence and delivery without touching domain rules.
- Infrastructure is pluggable: InMemory repo for fast local dev/tests, MongoDB for persistence in production.

Rule engine design
------------------
- Rules live in the Application/Services layer (FraudOrchestrator). Each rule is a unit of logic that returns whether it matched.
- Results are scored simply by matched rules count. Each matched rule is recorded to support explainability and investigations.
- Rules are intentionally simple and configurable; move thresholds and lists to IOptions (appsettings / secret store) for production configuration and dynamic tuning.
- For more advanced needs, the orchestrator can call a rules-evaluation microengine (Drools-style or a dynamic rule store) or incorporate ML scoring in a risk-model step.

## Quick start — download & run

Follow these minimal steps to get the project running in Docker or locally.

### Prerequisites
- .NET 8 SDK (for local development and tests)
- Git (to clone the repository)
- Docker & Docker Compose (optional — required if you want to run in containers)
- MongoDB (optional) — only required if you want to run the service with a real database. The app supports an in-memory repository if you leave the connection string empty.

### Download the repository
# Clone the repo (replace with your fork or upstream URL)
git clone https://github.com/<your-org>/fraud-rule-engine-service.git
cd fraud-rule-engine-service

### Environment variables
Minimal required configuration values:
- `JwtSettings__SigningKey` — signing key for JWT validation (required)
- `MongoDbSettings__ConnectionString` — optional; leave empty to use the in-memory repository for development/tests

This signing key is for testing purposes only. In a production environment, User Secrets, environment variables, or a secret manager should be used to store sensitive information.
"JwtSettings__SigningKey=w5sbqVHXhDJ6gCSqdSIxUrfXciZXIo4WMCNp1RFX6Kc="

If you prefer to generate your own signing key for testing, you can use the following PowerShell command:
$rng=[System.Security.Cryptography.RandomNumberGenerator]::Create(); $b=New-Object byte[] 32; $rng.GetBytes($b); [Convert]::ToBase64String($b)


### Run in Docker (single container)

### Build the Docker image
docker build -t fraud-service . 

Run without MongoDB (in-memory repository):
docker run -e EnableSwagger=true -e JwtSettings__SigningKey=w5sbqVHXhDJ6gCSqdSIxUrfXciZXIo4WMCNp1RFX6Kc= -e MongoDbSettings__ConnectionString= -p 8080:80 fraud-service

Run with a local MongoDB instance (replace the connection string accordingly):
docker run -e "EnableSwagger=true" -e "JwtSettings__SigningKey=w5sbqVHXhDJ6gCSqdSIxUrfXciZXIo4WMCNp1RFX6Kc=" -e "MongoDbSettings__ConnectionString=mongodb://host.docker.internal:27017" -p 8080:80 fraud-service
# API available at http://localhost:8080


### Run locally (dotnet)
# Restore, build and run the API
export JwtSettings__SigningKey="your-test-signing-key"
export MongoDbSettings__ConnectionString=""
dotnet restore
dotnet build -c Release
dotnet run --project "Fraud Rule Engine Service" --urls "https://localhost:5001;http://localhost:5000"

# Run tests
dotnet test


### Notes
- MongoDB is optional for development; if `MongoDbSettings__ConnectionString` is empty the app will use the in-memory repository.
- Always provide `JwtSettings__SigningKey` so the app can start and tests can create valid tokens.
- If you run into HTTPS redirects in Docker, the Dockerfile sets `ASPNETCORE_URLS=http://+:80` so the container listens on HTTP. For local development, the app still uses HTTPS by default.

