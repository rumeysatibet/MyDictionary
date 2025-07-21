# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MyDictionary is a social dictionary platform built with .NET 9, featuring a microservices architecture using .NET Aspire for orchestration. The application consists of:
- **MyDictionary.ApiService**: Backend API with Entity Framework Core and SQL Server
- **MyDictionary.Web**: Blazor Server frontend
- **MyDictionary.AppHost**: Aspire orchestration host
- **MyDictionary.ServiceDefaults**: Shared service configurations

## Common Development Commands

### Build and Run
```bash
# Development startup (recommended)
dev-start.bat

# Manual startup
dotnet clean
dotnet build
dotnet run --project MyDictionary.AppHost
```

### Database Management
```bash
# Apply migrations
dotnet ef database update --project MyDictionary.ApiService

# Create new migration
dotnet ef migrations add MigrationName --project MyDictionary.ApiService

# Drop database (development only)
dotnet ef database drop --project MyDictionary.ApiService
```

### Individual Service Startup
```bash
# API Service only
dotnet run --project MyDictionary.ApiService

# Web frontend only  
dotnet run --project MyDictionary.Web
```

## Architecture

### Backend (MyDictionary.ApiService)
- **Framework**: ASP.NET Core 9.0 Web API
- **Database**: Entity Framework Core with SQL Server (LocalDB)
- **Authentication**: JWT Bearer tokens
- **Key Services**:
  - `IAuthService`: User authentication and JWT token management
  - `INotificationService`: Notification creation and management
  - `DataSeedingService`: Test data seeding
- **Controllers**: Organized by feature (Auth, User, Profile, Friends, Messages, Notifications, Categories, Topics, Entries, Favorites)

### Frontend (MyDictionary.Web)
- **Framework**: Blazor Server (.NET 9)
- **State Management**: Service-based approach with `AuthenticationStateService`
- **Communication**: HttpClient factory with Aspire service discovery
- **Key Components**:
  - Layout components in `Components/Layout/`
  - Page components in `Components/Pages/`
  - Shared components in `Components/Shared/`

### Database Schema
The application uses a comprehensive social platform schema with the following key entities:
- **Users**: Authentication and profile management
- **Categories/Topics/Entries**: Content hierarchy (like Reddit's subreddits/posts/comments)
- **Friendships/FriendRequests**: Social connections
- **Messages/Conversations**: Direct messaging
- **Notifications**: System notifications
- **EntryFavorites/EntryLinks**: Content interaction features

Key relationships are configured with appropriate cascade behaviors and performance indexes in `DictionaryDbContext.cs:26-229`.

### Authentication Flow
- JWT tokens with configurable issuer/audience/key
- Bearer token authentication on API
- Frontend stores JWT in browser storage
- `AuthenticationStateService` manages auth state across Blazor components

### File Upload System
- Profile photos stored in `wwwroot/uploads/profiles/`
- Static file serving configured for uploads directory
- Web app proxies upload requests to API service

## Development Environment

### Prerequisites
- Visual Studio 2022 (17.8+)
- .NET 9 SDK
- SQL Server LocalDB

### Configuration
- **API Service**: Runs on port 5000 (configured in launchSettings.json)
- **Web Service**: Runs on port 5001 with Aspire service discovery
- **Database**: Uses LocalDB with connection string "mydictionarydb"

### Aspire Integration
The project uses .NET Aspire for:
- Service orchestration and discovery
- Automatic database migration on startup
- Centralized configuration management
- Development dashboard and monitoring

### CORS Configuration
API service is configured with "AllowAll" CORS policy for development. This should be restricted for production deployment.

## Key Patterns

### Entity Framework Patterns
- DbContext with comprehensive relationship configuration
- Automatic migrations applied on startup
- Repository-like pattern through DbContext direct usage
- Performance indexes on frequently queried columns

### Service Layer Pattern
- Services registered with dependency injection
- Interface-based service contracts
- Scoped lifetime for request-scoped services

### Frontend Communication
- HttpClient factory with named clients
- Aspire service discovery for inter-service communication
- Fallback URL configuration for development

## Testing and Debug

### Debug Features
- Swagger UI available at root path in development
- Debug endpoints: `/api/auth/debug/users`, `/debug` page
- McpProbe integration for debugging and monitoring
- Structured logging to console and debug output

### Database Utilities
- `database-manager.bat`: Database management utilities
- `test-database.bat`: Database testing scripts
- `cleanup.bat`: Process cleanup for development restarts

## Security Considerations

- JWT tokens with configurable secrets (default key should be changed for production)
- Password hashing using BCrypt
- SQL injection protection through EF Core
- CORS configuration (currently permissive for development)
- File upload validation and secure storage paths