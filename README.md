# 🔐 SecureAuth - Production-Grade Authentication API

A comprehensive authentication and authorization service built with security-first principles, implementing OWASP Top 10 protections.

## 🎯 Features

### Core Authentication
- ✅ **User Registration** with email validation
- ✅ **Secure Login** with bcrypt password hashing (work factor 12)
- ✅ **JWT Tokens** with 15-minute expiration
- ✅ **Refresh Tokens** with 7-day expiration
- ✅ **Token Verification** endpoint for consuming apps
- ✅ **Account Lockout** after 5 failed login attempts

### Security Features
- ✅ **Password Requirements** - Uppercase, lowercase, number, special character
- ✅ **HTTP-Only Cookies** for refresh tokens (XSS protection)
- ✅ **Secure Cookie Settings** - HTTPS only, SameSite=Strict
- ✅ **Security Headers** - CSP, X-Frame-Options, HSTS, etc.
- ✅ **Comprehensive Audit Logging** - All authentication events tracked
- ✅ **IP Address Tracking** - Monitor login locations
- ✅ **Rate Limiting** - Account lockout prevents brute force attacks

### OWASP Top 10 Protection

| OWASP Risk | Protection Implemented |
|------------|------------------------|
| Broken Access Control | JWT token validation, role-based authorization ready |
| Cryptographic Failures | BCrypt password hashing, HTTPS enforcement, secure cookies |
| Injection | EF Core parameterized queries (SQL injection prevention) |
| Insecure Design | Security-first architecture, principle of least privilege |
| Security Misconfiguration | Security headers, environment-based configuration |
| Vulnerable Components | Regular NuGet updates, dependency scanning |
| Authentication Failures | Strong password policy, account lockout, 2FA ready |
| Software Integrity | Docker image verification, code signing ready |
| Logging & Monitoring | Comprehensive audit logs, security event tracking |
| SSRF | Input validation, URL whitelist ready |

## 🛠️ Tech Stack

- **Backend:** C# 12, ASP.NET Core 8
- **Database:** PostgreSQL 15
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer, BCrypt.Net
- **Containerization:** Docker, Docker Compose
- **API Documentation:** Swagger/OpenAPI

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

### Quick Start with Docker

1. **Clone the repository**
```bash
   git clone https://github.com/Nathan-Forest/SecureAuth.git
   cd SecureAuth
```

2. **Start the services**
```bash
   docker-compose up --build
```

3. **API will be available at:**
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - Health Check: http://localhost:5000/health

### Local Development (Without Docker)

1. **Install PostgreSQL**
   - Download from https://www.postgresql.org/download/

2. **Update connection string**
   - Edit `backend/SecureAuthAPI/appsettings.Development.json`
   - Set your local PostgreSQL connection

3. **Run migrations**
```bash
   cd backend/SecureAuthAPI
   dotnet ef database update
```

4. **Run the API**
```bash
   dotnet run
```

## 📚 API Documentation

### Base URL
```
http://localhost:5000/api
```

### Endpoints

#### Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/auth/register` | Register new user | No |
| POST | `/auth/login` | Login with credentials | No |
| POST | `/auth/refresh` | Refresh JWT token | No (uses cookie) |
| POST | `/auth/logout` | Logout and revoke token | Yes |
| GET | `/auth/verify` | Verify JWT token validity | No |

### Sample Requests

**Register**
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Nathan Forest",
  "email": "nathan@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "CfDJ8N...",
  "expiresAt": "2024-03-20T15:30:00Z",
  "user": {
    "id": 1,
    "name": "Nathan Forest",
    "email": "nathan@example.com",
    "emailVerified": false,
    "twoFactorEnabled": false,
    "createdAt": "2024-03-20T15:15:00Z"
  }
}
```

**Login**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "nathan@example.com",
  "password": "SecurePass123!"
}
```

**Verify Token** (for consuming apps)
```http
GET /api/auth/verify
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
{
  "valid": true,
  "user": {
    "id": "1",
    "email": "nathan@example.com",
    "name": "Nathan Forest"
  }
}
```

## 🗄️ Database Schema

### Users Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | varchar(100) | User's full name |
| Email | varchar(255) | Unique email address |
| PasswordHash | text | BCrypt hashed password |
| EmailVerified | boolean | Email verification status |
| TwoFactorEnabled | boolean | 2FA enabled flag |
| TwoFactorSecret | text | TOTP secret (nullable) |
| CreatedAt | timestamp | Account creation time |
| LastLoginAt | timestamp | Last successful login |
| FailedLoginAttempts | int | Failed login counter |
| LockoutEnd | timestamp | Account lockout expiration |
| IsActive | boolean | Account active status |

### RefreshTokens Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Token | text | Unique refresh token |
| UserId | int | Foreign key to Users |
| ExpiresAt | timestamp | Token expiration |
| IsRevoked | boolean | Revocation status |
| CreatedByIp | varchar(45) | IP address of creation |

### AuditLogs Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| UserId | int | Foreign key to Users (nullable) |
| Action | varchar(50) | Action performed |
| Details | varchar(500) | Additional details |
| IpAddress | varchar(45) | IP address |
| Success | boolean | Action success status |
| CreatedAt | timestamp | Log timestamp |

## 🔐 Security Best Practices Implemented

1. **Password Security**
   - BCrypt hashing with work factor 12
   - Minimum 8 characters, complexity requirements
   - Passwords never logged or stored in plain text

2. **Token Security**
   - Short-lived JWT (15 minutes)
   - Long-lived refresh tokens (7 days)
   - Refresh tokens in HTTP-only cookies
   - Token rotation on refresh

3. **Attack Prevention**
   - Account lockout after 5 failed attempts
   - 15-minute lockout duration
   - IP address logging
   - Comprehensive audit trail

4. **Transport Security**
   - HTTPS enforcement (production)
   - Secure cookie flags
   - Security headers (HSTS, CSP, etc.)

5. **Input Validation**
   - Data annotations on DTOs
   - Email format validation
   - Password complexity validation
   - SQL injection prevention (EF Core)

## 🎯 Use Cases

### 1. Central Authentication for Multiple Apps
Use SecureAuth as the single source of truth for user authentication across multiple applications (SSO).

### 2. API Authentication Service
Secure your microservices by validating tokens with the `/verify` endpoint.

### 3. Learning Platform
Study production-grade authentication implementation and security best practices.

## 🚧 Future Enhancements (Roadmap)

- [ ] Email verification with SendGrid/SMTP
- [ ] Two-Factor Authentication (TOTP)
- [ ] OAuth 2.0 providers (Google, Microsoft, GitHub)
- [ ] Password reset flow
- [ ] Rate limiting middleware
- [ ] User profile management
- [ ] Admin dashboard
- [ ] Session management UI
- [ ] TypeScript frontend (React)
- [ ] API rate limiting per user
- [ ] IP whitelist/blacklist
- [ ] Device fingerprinting
- [ ] Kubernetes deployment configs

## 👨‍💻 Author

**Nathan Forest**
- GitHub: [@Nathan-Forest](https://github.com/Nathan-Forest)
- LinkedIn: [Nathan Forest](https://linkedin.com/in/nathan-forest-australia)

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 🙏 Acknowledgments

- Built as part of a security-focused portfolio demonstrating OWASP Top 10 protections
- Implements industry best practices for authentication and authorization
- Designed to serve as the authentication backbone for future projects

---

**Part of Nathan's Development Portfolio:**
1. [Invoice Validator](https://github.com/Nathan-Forest/invoice-validator) - Node.js with Jest
2. [Expense Tracker](https://github.com/Nathan-Forest/expense-tracker) - JavaScript (live)
3. [Dice Game](https://github.com/Nathan-Forest/dice-game) - C# Console
4. [FinanceHub](https://github.com/Nathan-Forest/FinanceHub) - C# ASP.NET Core
5. [StockTracker](https://github.com/Nathan-Forest/StockTracker) - Python Flask
6. **SecureAuth** - C# Authentication API _(this project)_