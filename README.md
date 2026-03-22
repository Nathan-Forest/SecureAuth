# 🔐 SecureAuth

**Production-grade authentication API with modern security practices**

A full-stack authentication system built with C# ASP.NET Core backend and TypeScript React frontend, implementing OWASP Top 10 security protections, JWT authentication, and comprehensive audit logging.

![SecureAuth Dashboard](screenshots/dashboard.png)

## 🎯 Project Overview

SecureAuth is a secure authentication system designed to be the central SSO (Single Sign-On) hub for all portfolio applications. Built with enterprise-grade security practices and modern web technologies.

### Live Demo
- **Frontend:** `http://localhost:5173`
- **Backend API:** `http://localhost:5000`
- **Swagger Docs:** `http://localhost:5000/swagger`

## ✨ Features

### Backend Features
- ✅ JWT token authentication (15-minute expiration)
- ✅ Refresh tokens (7-day expiration, HTTP-only cookies)
- ✅ BCrypt password hashing (work factor 12)
- ✅ Account lockout protection (5 failed attempts, 15-min lockout)
- ✅ Comprehensive audit logging
- ✅ PostgreSQL database with Entity Framework Core
- ✅ FluentValidation for input validation
- ✅ Docker containerization
- ✅ OWASP Top 10 security protections
- ✅ Security headers (CSP, HSTS, X-Frame-Options)

### Frontend Features
- ✅ Beautiful Tailwind CSS UI
- ✅ Real-time password strength meter
- ✅ Protected routes
- ✅ TypeScript for type safety
- ✅ Context API state management
- ✅ Responsive design
- ✅ Loading states and error handling

## 🛠️ Tech Stack

### Backend
- **Framework:** ASP.NET Core 8
- **Database:** PostgreSQL 15
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer Tokens
- **Password Hashing:** BCrypt.Net
- **Validation:** FluentValidation
- **Containerization:** Docker & Docker Compose
- **API Documentation:** Swagger/OpenAPI

### Frontend
- **Framework:** React 18
- **Language:** TypeScript
- **Styling:** Tailwind CSS
- **Build Tool:** Vite
- **Routing:** React Router
- **HTTP Client:** Axios
- **State Management:** Context API

## 🚀 Getting Started

### Prerequisites
- Docker Desktop
- Node.js 18+
- .NET 8 SDK (for local development)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/Nathan-Forest/SecureAuth.git
cd SecureAuth
```

2. **Start the backend with Docker**
```bash
docker-compose up
```

3. **Install frontend dependencies**
```bash
cd frontend
npm install
```

4. **Start the frontend**
```bash
npm run dev
```

5. **Visit the application**
- Frontend: http://localhost:5173
- Backend API: http://localhost:5000
- Swagger Docs: http://localhost:5000/swagger

## 📁 Project Structure
```
SecureAuth/
├── backend/
│   └── SecureAuthAPI/
│       ├── Controllers/      # API endpoints
│       ├── Models/          # Database entities
│       ├── DTOs/            # Data transfer objects
│       ├── Services/        # Business logic
│       ├── Data/            # Database context
│       └── Migrations/      # EF Core migrations
├── frontend/
│   ├── src/
│   │   ├── components/      # React components
│   │   ├── pages/           # Page components
│   │   ├── contexts/        # React contexts
│   │   ├── services/        # API services
│   │   └── types/           # TypeScript types
│   └── public/
├── docker-compose.yml
└── README.md
```

## 🔒 Security Features

### OWASP Top 10 Protections
1. **Broken Access Control:** JWT authentication, protected routes
2. **Cryptographic Failures:** BCrypt hashing, secure token storage
3. **Injection:** Parameterized queries, input validation
4. **Insecure Design:** Security-first architecture
5. **Security Misconfiguration:** Security headers, HTTPS ready
6. **Vulnerable Components:** Updated dependencies
7. **Authentication Failures:** Account lockout, strong passwords
8. **Data Integrity Failures:** JWT signature verification
9. **Logging Failures:** Comprehensive audit logging
10. **SSRF:** Input validation, allowlisting

### Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### Account Security
- Account lockout after 5 failed login attempts
- 15-minute lockout duration
- IP address tracking
- Audit logging for all authentication events

## 📡 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login with credentials
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout and revoke tokens
- `GET /api/auth/verify` - Verify JWT token validity

### Health Check
- `GET /health` - API health status

## 🧪 Testing

### Test User Registration
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "SecurePass123!",
    "confirmPassword": "SecurePass123!"
  }'
```

### Test Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'
```

## 🎨 Screenshots

### Login Page
![Login](screenshots/login.png)

### Register Page
![Register](screenshots/register.png)

### Dashboard
![Dashboard](screenshots/dashboard.png)

## 🔮 Future Enhancements

- [ ] Two-factor authentication (TOTP/Google Authenticator)
- [ ] Email verification
- [ ] Password reset flow
- [ ] Rate limiting middleware
- [ ] OAuth/SSO integration (Google, GitHub)
- [ ] Admin dashboard
- [ ] User management endpoints
- [ ] Session management
- [ ] Device tracking
- [ ] Geolocation-based security

## 👨‍💻 Author

**Nathan Forest**
- GitHub: [@Nathan-Forest](https://github.com/Nathan-Forest)
- LinkedIn: [Nathan Forest](https://linkedin.com/in/nathan-forest-australia)

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- Built as part of IT Support → Developer transition portfolio
- Security best practices from OWASP guidelines
- Architecture inspired by enterprise authentication systems
```

---

## 🎯 **WHAT'S NEXT?**

**You have options:**

1. **🌐 Deploy to Cloud** - Azure, AWS, or DigitalOcean
2. **📱 Add 2FA** - Google Authenticator integration
3. **✉️ Email Verification** - SendGrid integration
4. **🔌 Retrofit StockTracker** - Connect your Python app to SecureAuth!
5. **📝 LinkedIn Post** - Show the world!
6. **🎨 More Features** - Password reset, OAuth, etc.

---

## 🏆 **PORTFOLIO COUNT:**
```
✅ 1. Invoice Validator (Node.js + Jest)
✅ 2. Expense Tracker (JavaScript + localStorage)
✅ 3. Dice of Fortune (C# Console)
✅ 4. FinanceHub (C# + SQLite + EF Core)
✅ 5. StockTracker (Python Flask + SQLAlchemy)
✅ 6. SecureAuth (C# + PostgreSQL + TypeScript React) 🔥