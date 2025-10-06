# 🎲 DnD Tavern — Your Gateway to Tabletop RPG Adventures

A modern, full-stack marketplace application for discovering, booking, and managing tabletop RPG campaigns. Whether you're a player looking for your next adventure or a Game Master ready to lead epic quests, DnD Tavern brings the community together in one seamless platform.

Built with **microservices architecture**, featuring an **ASP.NET Core** backend, **Angular** frontend, **RabbitMQ** message broker, and **Hangfire** for background jobs. This project showcases enterprise-grade patterns including event-driven architecture, async messaging, and distributed systems.

![DnD Tavern Preview](dnd-tavern/Screenshot_1.jpg)

---

## ✨ Key Features

### For Players
- **🔍 Campaign Discovery** — Browse through a catalog of exciting D&D campaigns with rich details, schedules, and Game Master profiles
- **📅 Smart Booking System** — Reserve your spot in upcoming sessions with real-time slot availability
- **📧 Email Notifications** — Instant booking confirmations and automated reminders (30 days, 3 days, same day)
- **⭐ Review & Rate** — Share your experience and help others find the best Game Masters
- **👤 User Dashboard** — Manage your bookings, view upcoming sessions, and track your adventure history

### For Game Masters
- **🎭 Campaign Management** — Create and manage your campaigns with detailed descriptions, schedules, and pricing
- **📊 GM Dashboard** — Monitor bookings, manage slots, and interact with your player base
- **💰 Flexible Pricing** — Set your rates and manage campaign availability

### Technical Highlights
- **🏗️ Microservices Architecture** — Independent services for API and email processing
- **📨 Event-Driven Design** — Asynchronous messaging with RabbitMQ for scalability
- **⏰ Background Jobs** — Hangfire for scheduled reminders and long-running tasks
- **🔐 JWT Authentication** — Secure authentication with Google OAuth integration
- **📱 Responsive Design** — Beautiful UI built with Angular 20 and Bootstrap 5
- **🐳 Docker Ready** — Fully containerized for easy deployment
- **🚀 CI/CD Pipeline** — Automated builds and deployments via GitHub Actions to AWS Elastic Beanstalk
- **📖 API Documentation** — Complete Swagger/OpenAPI documentation for all endpoints
- **🧩 Clean Architecture** — Separation of concerns with Domain, Application, Infrastructure, and API layers
- **💾 Redis Caching** — Circuit breaker pattern for fault tolerance

---

## 🏗️ System Architecture

```
┌─────────────────────┐
│   Angular Frontend  │  ← User Interface (Port 80/4200)
└──────────┬──────────┘
           │ HTTP/REST
           ↓
┌─────────────────────┐
│   ASP.NET Core API  │  ← Main Application (Port 5000)
│   (Publisher)       │  ← JWT Auth, Business Logic
└──────────┬──────────┘
           │
           ├─→ PostgreSQL (Port 5432)     ← Primary Database
           ├─→ Redis (Port 6379)          ← Cache Layer
           │
           │ async messaging
           ↓
      ┌─────────┐
      │RabbitMQ │  ← Message Broker (Port 5672)
      │ Queue   │  ← Durable, Persistent Messages
      └────┬────┘
           │ consume
           ↓
┌─────────────────────┐
│  EmailWorker        │  ← Background Service
│  (Consumer)         │  ← Processes email notifications
│                     │
│  ┌────────────────┐ │
│  │   Hangfire     │ │  ← Job Scheduler (Port 5001)
│  │  (PostgreSQL)  │ │  ← Dashboard & Task Management
│  └────────────────┘ │
│                     │
│  ┌────────────────┐ │
│  │  EmailService  │ │  ← MailKit (SMTP)
│  │    (Gmail)     │ │  ← Sends emails via Gmail
│  └────────────────┘ │
└─────────────────────┘
```

### Microservices Communication Flow

**1. User creates a booking:**
```
User → Frontend → API → PostgreSQL (save booking)
                      ↓
                  RabbitMQ (publish message)
```

**2. EmailWorker processes message:**
```
RabbitMQ → EmailWorker → Sends confirmation email
                      ↓
                  Hangfire (schedules 3 reminder tasks)
```

**3. Hangfire executes scheduled reminders:**
```
Hangfire → EmailWorker → Sends reminder email
                      ↓
                  (30 days before, 3 days before, same day)
```

### Key Architectural Patterns

- **Publisher-Subscriber Pattern** — Decoupled communication via RabbitMQ
- **Circuit Breaker** — Resilience with Polly for Redis connections
- **Repository Pattern** — Clean data access abstraction
- **Unit of Work** — Transaction management across repositories
- **Dependency Injection** — Loose coupling and testability
- **CQRS Principles** — Separation of read/write operations
- **Background Jobs** — Hangfire for delayed and recurring tasks

---

## 🔌 API Documentation

The backend exposes a comprehensive RESTful API documented with **Swagger/OpenAPI**.

### Accessing Swagger UI

When running the backend locally in development mode:

**URL:** `http://localhost:5000/swagger`

The Swagger UI provides:
- Interactive API exploration
- Request/response schemas
- Authentication testing with JWT tokens
- Try-it-out functionality for all endpoints

### Available API Endpoints

| Resource | Endpoints | Description |
|----------|-----------|-------------|
| **Campaigns** | `/api/campaigns` | Browse, search, and manage D&D campaigns |
| **Masters** | `/api/masters` | Game Master profiles and information |
| **Bookings** | `/api/bookings` | Create and manage session bookings (triggers async email) |
| **Users** | `/api/users` | User authentication and profile management |

All endpoints return standardized responses wrapped in a custom middleware for consistent error handling and data formatting.

---

## 🚀 Quick Start

### Prerequisites

#### Backend Requirements
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [PostgreSQL](https://www.postgresql.org/) database
- [Redis](https://redis.io/) for caching
- [RabbitMQ](https://www.rabbitmq.com/) for message broker
- Docker & Docker Compose (recommended)

#### Frontend Requirements
- [Node.js 20.x](https://nodejs.org/) or later
- npm (comes with Node.js)

---

## 🐳 Docker Deployment (Recommended)

The easiest way to run the entire stack is using Docker Compose:

### 1. Clone the Repository
```bash
git clone https://github.com/Barik766/DnDAgency.git
cd DnDAgency
```

### 2. Configure Environment Variables

Create a `.env` file in the root directory with the following variables:

```env
# Database Configuration
RDS_HOSTNAME=postgres
RDS_PORT=5432
RDS_DB_NAME=DnDAgencyDb
RDS_USERNAME=postgres
RDS_PASSWORD=your-secure-password

# Redis Cache
REDIS_HOST=redis:6379

# RabbitMQ Message Broker
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest

# JWT Settings
JwtSettings__Key=your-super-secret-jwt-key-minimum-32-characters-long
JwtSettings__Issuer=DnDAgency.Api
JwtSettings__Audience=DnDAgency.Client

# Google OAuth (optional)
GoogleOAuth__ClientId=your-google-client-id
GoogleOAuth__ClientSecret=your-google-client-secret

# Email Configuration (for EmailWorker)
EMAIL_SMTP_HOST=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_SENDER=your-email@gmail.com
EMAIL_PASSWORD=your-gmail-app-password
```

### 3. Start the Application

**For local development:**
```bash
docker-compose -f docker-compose.local.yml up --build
```

**For production:**
```bash
docker-compose up --build
```

The application will be available at:
- **Frontend:** http://localhost:4200 (dev) or http://localhost:80 (prod)
- **Backend API:** http://localhost:5000
- **Swagger UI:** http://localhost:5000/swagger
- **RabbitMQ Management:** http://localhost:15672 (guest/guest)
- **Hangfire Dashboard:** http://localhost:5001/hangfire
- **Redis:** localhost:6379
- **PostgreSQL:** localhost:5432

---

## 💻 Local Development Setup

### Backend Setup

1. **Navigate to the backend directory:**
   ```bash
   cd DnDAgency
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Update connection string in `appsettings.Development.json`:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=DnDAgencyDb;Username=postgres;Password=yourpassword",
       "Redis": "localhost:6379"
     },
     "RabbitMQ": {
       "Host": "localhost",
       "Port": "5672",
       "Username": "guest",
       "Password": "guest"
     }
   }
   ```

4. **Run database migrations:**
   ```bash
   dotnet ef database update --project DnDAgency.Infrastructure
   ```
   
   If `dotnet ef` is not installed:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

5. **Run the backend:**
   ```bash
   dotnet run --project DnDAgency/DnDAgency.Api.csproj
   ```
   
   The API will be available at `http://localhost:5000`

### EmailWorker Setup

1. **Navigate to EmailWorker directory:**
   ```bash
   cd DnDAgency.EmailWorker
   ```

2. **Update `appsettings.json`:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=DnDAgencyDb;Username=postgres;Password=yourpassword"
     },
     "RabbitMQ": {
       "Host": "localhost",
       "Port": "5672",
       "Username": "guest",
       "Password": "guest",
       "QueueName": "booking-confirmations"
     },
     "Email": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your-email@gmail.com",
       "SenderName": "DnD Agency",
       "Password": "your-app-password"
     }
   }
   ```

3. **Run the EmailWorker:**
   ```bash
   dotnet run
   ```
   
   Hangfire dashboard will be available at `http://localhost:5001/hangfire`

### Frontend Setup

1. **Navigate to the frontend directory:**
   ```bash
   cd dnd-tavern
   ```

2. **Install dependencies:**
   ```bash
   npm ci
   ```

3. **Run the development server:**
   ```bash
   npm start
   ```
   
   The Angular app will be available at `http://localhost:4200`

4. **Build for production:**
   ```bash
   npm run build
   ```

---

## 📚 Example API Usage

Here are some example API requests to get you started:

### 1. Get All Campaigns
```bash
curl -X GET "http://localhost:5000/api/campaigns" -H "accept: application/json"
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Lost Mines of Phandelver",
      "description": "A classic D&D adventure for new players",
      "system": "D&D 5e",
      "level": "Beginner",
      "pricePerSession": 15.00,
      "masterId": "1234-5678-90ab-cdef",
      "masterName": "John the DM"
    }
  ]
}
```

### 2. Register a New User
```bash
curl -X POST "http://localhost:5000/api/users/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "adventurer123",
    "email": "adventurer@example.com",
    "password": "SecurePass123!"
  }'
```

### 3. Create a Booking (Triggers Email Notification)
```bash
curl -X POST "http://localhost:5000/api/bookings" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "campaignId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "startTime": "2025-12-01T18:00:00Z",
    "playersCount": 4
  }'
```

**What happens:**
1. ✅ API saves booking to PostgreSQL
2. ✅ Publishes message to RabbitMQ
3. ✅ EmailWorker consumes message
4. ✅ Sends confirmation email immediately
5. ✅ Schedules 3 reminder emails in Hangfire:
   - 30 days before session
   - 3 days before session
   - Morning of session day

---

## 🛠️ For Developers

### Project Structure

```
DnDAgency/
├── DnDAgency/                    # API Layer (ASP.NET Core Web API)
│   ├── Controllers/              # API Controllers
│   ├── Filters/                  # Custom action filters
│   ├── Middleware/               # Custom middleware (auth, logging, error handling)
│   └── Program.cs                # Application entry point
│
├── DnDAgency.Domain/             # Domain Layer (Entities & Interfaces)
│   ├── Entities/                 # Domain models (Campaign, Master, User, Booking, etc.)
│   └── Interfaces/               # Repository interfaces
│
├── DnDAgency.Application/        # Application Layer (Business Logic)
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Interfaces/               # Service interfaces
│   ├── Services/                 # Business logic implementation
│   └── Messages/                 # Message contracts for RabbitMQ
│
├── DnDAgency.Infrastructure/     # Infrastructure Layer (Data Access)
│   ├── Data/                     # DbContext and configuration
│   ├── Repositories/             # Repository implementations
│   ├── Migrations/               # EF Core migrations
│   ├── Messaging/                # RabbitMQ publisher
│   └── UnitOfWork/               # Unit of Work pattern
│
├── DnDAgency.EmailWorker/        # EmailWorker Microservice
│   ├── Services/                 # Email & Reminder services
│   ├── Infrastructure/           # Authorization filters
│   ├── Program.cs                # Worker entry point
│   └── Worker.cs                 # RabbitMQ consumer
│
├── dnd-tavern/                   # Frontend (Angular 20)
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/             # Core features (auth, campaigns, bookings, etc.)
│   │   │   └── shared/           # Shared components and services
│   │   └── stylesFolder/         # SCSS styles, design tokens
│   ├── Dockerfile                # Frontend Docker configuration
│   └── nginx.conf                # Nginx reverse proxy config
│
├── Dockerfile                    # Backend Docker configuration
├── docker-compose.yml            # Production orchestration
├── docker-compose.local.yml      # Development orchestration
└── .github/workflows/            # CI/CD pipeline configuration
```

### Architecture Pattern

The backend follows **Clean Architecture** with **Microservices** principles:

- **Domain Layer:** Pure business logic and entities (no dependencies)
- **Application Layer:** Use cases and business rules (depends on Domain)
- **Infrastructure Layer:** Data access, external services (depends on Domain & Application)
- **API Layer:** Controllers, filters, middleware (depends on all layers)
- **EmailWorker Service:** Independent microservice for async operations

### Key Technologies

**Backend:**
- ASP.NET Core 8.0
- Entity Framework Core
- PostgreSQL
- Redis (with Polly Circuit Breaker)
- RabbitMQ (Message Broker)
- JWT Authentication
- Swagger/OpenAPI

**EmailWorker:**
- .NET 8.0 Worker Service
- Hangfire (Background Jobs)
- RabbitMQ.Client (Consumer)
- MailKit (SMTP Email)
- PostgreSQL (Hangfire storage)

**Frontend:**
- Angular 20
- RxJS
- Bootstrap 5
- SCSS

**DevOps:**
- Docker & Docker Compose
- GitHub Actions
- AWS Elastic Beanstalk
- Nginx

### Running Tests

```bash
# Backend tests
cd DnDAgency
dotnet test

# Frontend tests
cd dnd-tavern
npm test
```

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName --project DnDAgency.Infrastructure --startup-project DnDAgency
```

Apply migrations:
```bash
dotnet ef database update --project DnDAgency.Infrastructure --startup-project DnDAgency
```

### Monitoring Services

**RabbitMQ Management UI:**
- URL: http://localhost:15672
- Credentials: guest/guest
- Monitor queues, exchanges, message rates

**Hangfire Dashboard:**
- URL: http://localhost:5001/hangfire
- View scheduled jobs, recurring tasks, job history
- Retry failed jobs manually

---

## 🗺️ Roadmap & Future Enhancements

We're continuously improving DnD Tavern! Here's what's on the horizon:

### Upcoming Features
- [ ] **Real-time Chat** — Live messaging between players and Game Masters using SignalR
- [ ] **Advanced Search & Filters** — Filter campaigns by system, level, price range, location, and more
- [ ] **Calendar Integration** — Sync bookings with Google Calendar and other calendar apps
- [ ] **Payment Integration** — Stripe/PayPal integration for seamless payment processing
- [ ] **Session Notes & Documents** — Upload and share campaign materials, character sheets, and session notes
- [ ] **Virtual Tabletop Integration** — Direct links to Roll20, Foundry VTT, and other platforms
- [ ] **SMS Notifications** — Twilio integration for SMS reminders

### Architecture Improvements
- [x] **Microservices Architecture** — Implemented with API and EmailWorker services
- [x] **Message Queue** — RabbitMQ for async communication
- [x] **Background Jobs** — Hangfire for scheduled tasks
- [x] **Caching Layer** — Redis integration with circuit breaker
- [ ] **GraphQL API** — Alternative query language for more flexible data fetching
- [ ] **Event Sourcing** — Track all changes to bookings and campaigns with CQRS pattern
- [ ] **API Gateway** — Ocelot or YARP for unified entry point
- [ ] **Service Mesh** — Istio for advanced traffic management

### Developer Experience
- [ ] **Unit & Integration Tests** — Comprehensive test coverage
- [ ] **API Versioning** — Support multiple API versions for backward compatibility
- [ ] **Kubernetes Deployment** — Container orchestration with K8s
- [ ] **Monitoring & Logging** — Application Insights, ELK stack integration
- [ ] **Health Checks** — Endpoint monitoring for all services
- [ ] **Distributed Tracing** — OpenTelemetry integration

---

## 🤝 Contributing

We welcome contributions! Whether it's bug fixes, new features, or documentation improvements, your help makes DnD Tavern better for everyone.

### How to Contribute

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is available for educational and portfolio purposes. Feel free to use it as inspiration for your own projects!

---

## 🌟 Acknowledgments

Built with passion for the tabletop RPG community and modern software architecture principles. Special thanks to all dungeon masters and players who make every session memorable!

**Architecture inspired by:**
- Clean Architecture (Robert C. Martin)
- Domain-Driven Design (Eric Evans)
- Microservices Patterns (Chris Richardson)
- Enterprise Integration Patterns (Gregor Hohpe)

---

## 📧 Contact

**Project Maintainer:** [Barik766](https://github.com/Barik766)

**Live Demo:** [Coming Soon]

**Issues & Support:** [GitHub Issues](https://github.com/Barik766/DnDAgency/issues)

---

<div align="center">
  <strong>Roll for initiative and start your adventure today! 🎲⚔️</strong>
  
  <br><br>
  
  **Built with Clean Architecture | Microservices | Event-Driven Design**
</div>