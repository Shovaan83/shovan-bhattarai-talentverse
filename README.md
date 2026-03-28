<div align="center">

# 🌟 TalentVerse

### *The Skill-Swapping Platform That Connects Talent*

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-14-000000?style=for-the-badge&logo=nextdotjs&logoColor=white)](https://nextjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)

[Features](#-features) • [Tech Stack](#-tech-stack) • [Architecture](#-architecture) • [Documentation](#-documentation) • [License](#-license)

---

</div>

## 🎯 About TalentVerse

**TalentVerse** is a revolutionary skill-swapping platform where individuals exchange their expertise without money changing hands. Whether you're a designer seeking coding lessons or a developer wanting to learn photography, TalentVerse connects you with the perfect skill partner.

### 💡 The Concept

In a world where knowledge is abundant but often gatekept by price tags, TalentVerse creates a **barter economy of skills**. Users offer what they know and request what they want to learn, forming mutually beneficial partnerships that foster growth, collaboration, and community.

---

## ✨ Features

### 🔐 **Secure Authentication**
- JWT-based authentication with refresh tokens
- Two-Factor Authentication (2FA) via email
- OAuth integration (Google, GitHub)
- Password reset and account recovery

### 🎓 **Skill Management**
- Dual skill tracking: **Offer** skills you have, **Want** skills you seek
- Proficiency level system (1-5 stars)
- Category organization for easy discovery
- Rich skill profiles with descriptions

### 🤝 **Smart Proposal System**
- Send swap proposals to potential partners
- State machine workflow: Pending → Accepted → Completed
- Dual confirmation required for completion (builds trust)
- Cancel or decline with grace

### 🔍 **Intelligent Marketplace**
- Advanced search with filters (skill, category, proficiency)
- Featured users spotlight
- User profiles with skill showcases
- Match compatibility system

### 💬 **Real-Time Messaging**
- SignalR-powered instant messaging
- Proposal-specific conversation threads
- Read/unread status tracking
- Persistent message history

### ⭐ **Reputation System**
- Post-swap reviews and ratings
- Cumulative reputation scores
- Review history and feedback
- Trust-building transparency

### 💰 **Virtual Economy**
- **Swap Credits**: Earn by completing swaps
- Credit packs available for purchase (Stripe integration)
- Leaderboard showcasing top contributors
- Transaction history tracking

### 🏆 **Gamification & Badges**
- Automated badge awards for milestones
- "First Steps" (first signup)
- "Skill Sharer" (5 skills added)
- "Swap Master" (10 completed swaps)
- "Reviewer" (5 reviews submitted)
- "Verified" (identity verification badge)
- Credit rewards for achievements

### 🆔 **Identity Verification**
- Document upload for verification
- Admin review workflow
- Verified badge on profiles
- Enhanced trust and safety

### 📅 **Appointment Scheduling**
- Google Calendar integration
- Schedule swap sessions
- Event creation and management
- Calendar sync across platforms

### 👨‍💼 **Admin Panel**
- Identity verification review system
- User management capabilities
- Platform oversight tools
- Analytics dashboard (roadmap)

### 🔮 **Coming Soon**
- AI-powered skill matching
- Smart recommendation engine
- Video call integration
- Mobile apps (iOS/Android)

---

## 🛠️ Tech Stack

### **Backend**
- **ASP.NET Core 9.0** - Web API framework
- **PostgreSQL 17** - Primary database
- **Entity Framework Core** - ORM for Identity
- **Dapper** - High-performance data access for business logic
- **SignalR** - Real-time messaging
- **Cloudinary** - Image upload and management
- **Stripe** - Payment processing
- **JWT** - Token-based authentication

### **Frontend**
- **Next.js 14** - React framework with App Router
- **TypeScript** - Type-safe development
- **TanStack Query** - Data fetching and caching
- **React Hook Form** - Form management
- **Zod** - Schema validation
- **Tailwind CSS** - Utility-first styling
- **Shadcn/ui** - Component library

### **DevOps & Tools**
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **GitHub Actions** - CI/CD (roadmap)
- **Azure** - Production hosting (planned)

---

## 🏗️ Architecture

### **Backend Architecture**
```
┌─────────────────────────────────────────────────────┐
│                   Controllers                        │
│          (Thin - HTTP concerns only)                │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│                 Service Layer                        │
│     (Business Logic, Validation, Orchestration)     │
└─────────────────────────────────────────────────────┘
                        ↓
┌──────────────────────┬──────────────────────────────┐
│   EF Core (Identity) │    Dapper (Business Data)    │
│  UserManager, Roles  │  Skills, Proposals, Reviews  │
└──────────────────────┴──────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│              PostgreSQL Database                     │
└─────────────────────────────────────────────────────┘
```

### **Key Design Principles**
- **Clean Architecture**: Clear separation of concerns
- **Repository Pattern**: Abstraction over data access
- **Service Layer**: Encapsulated business logic
- **DTO Pattern**: API boundary safety
- **Hybrid ORM Strategy**: EF Core for Identity, Dapper for performance

---

## 📊 Database Schema Highlights

- **AspNetUsers** - Identity management (EF Core)
- **UserSkills** - Skill offerings and wants
- **Proposals** - Swap proposals with state machine
- **Messages** - Real-time and persistent messaging
- **Reviews** - Rating and reputation system
- **CreditTransactions** - Virtual economy ledger
- **Badges** - Gamification achievements
- **Appointments** - Scheduling and calendar sync
- **VerificationRequests** - Identity verification workflow

---

## 🎨 User Experience

### **Design Philosophy**
- **Bento Grid Layouts** - Modern, card-based interfaces
- **Semantic Color System** - Visual hierarchy by entity type
  - 🟢 Emerald: Skills you offer
  - 🟠 Orange: Skills you want
  - 🔵 Blue: Proposals and actions
- **Responsive Design** - Mobile-first approach
- **Real-time Updates** - Instant feedback and notifications
- **Intuitive Navigation** - Clear user flows

---

## 📚 Documentation

Comprehensive guides for deployment and development:

- **[Production Deployment Summary](PRODUCTION_DEPLOYMENT_SUMMARY.md)** - Complete deployment guide
- **[Security Checklist](SECURITY_CHECKLIST.md)** - Pre-deployment security verification
- **[Production Secrets Template](production-secrets-template.txt)** - Environment configuration reference

### **Additional Resources**
- API contract documentation (`.context/API_CONTRACT.md`)
- Architecture details (`.context/ARCHITECTURE.md`)
- Code conventions (`.context/CONVENTIONS.md`)
- Database schema (`.context/DB_SCHEMA.md`)
- Feature specifications (`.context/FEATURES.md`)

---

## 🌟 What Makes TalentVerse Special?

### **1. No Money, Just Skills**
Traditional freelance platforms charge fees and require payment. TalentVerse eliminates financial barriers entirely, making skill exchange accessible to everyone.

### **2. Mutual Growth**
Both parties benefit equally. There's no client-contractor dynamic—just two people helping each other grow.

### **3. Community-Driven**
The reputation system and gamification foster a supportive community where quality interactions are rewarded.

### **4. Trust & Safety**
Identity verification, dual-confirmation swap completion, and transparent reviews ensure a safe environment.

### **5. Modern Tech Stack**
Built with cutting-edge technologies for performance, scalability, and developer experience.

---

## 🎯 Use Cases

### **For Students**
- Exchange tutoring in different subjects
- Learn practical skills outside the classroom
- Build a portfolio of diverse experiences

### **For Professionals**
- Cross-train in complementary skills
- Expand your professional network
- Save money on professional development

### **For Hobbyists**
- Find teaching partners for your passion
- Learn new hobbies without cost
- Connect with like-minded enthusiasts

### **For Career Switchers**
- Gain skills for your new career path
- Leverage your current expertise to learn
- Build confidence through practical exchange

---

## 🔒 Security & Privacy

- **Encrypted authentication** with JWT and refresh tokens
- **Two-factor authentication** for enhanced security
- **OAuth integration** with trusted providers
- **Identity verification** for high-trust users
- **Rate limiting** to prevent abuse
- **HTTPS enforced** in production
- **Environment-based configuration** (no hardcoded secrets)
- **Regular security audits** (roadmap)

---

## 🚀 Performance

- **Dapper ORM** for high-performance database queries
- **PostgreSQL connection pooling** for scalability
- **TanStack Query** for intelligent data caching
- **Next.js 14** with App Router for optimized rendering
- **SignalR** for efficient real-time communication
- **Docker containerization** for consistent deployments

---

## 🤝 Contributing

TalentVerse is a learning project and open to contributions! Whether you're fixing bugs, adding features, or improving documentation, your help is welcome.

### **Ways to Contribute**
- 🐛 Report bugs and issues
- 💡 Suggest new features
- 📝 Improve documentation
- 🔧 Submit pull requests
- ⭐ Star the project

---

## 📝 License

This project is licensed under the **MIT License** - see the LICENSE file for details.

---

## 👨‍💻 Author

**Shovan Bhandari**  
*Third Year CS Student | Full-Stack Developer*

- Email: shovanbthr@gmail.com
- GitHub: [@shovanbthr](https://github.com/shovanbthr)

---

## 🙏 Acknowledgments

- Built as a **Final Year Project (FYP)** for Computer Science
- Inspired by the power of knowledge sharing and community building
- Special thanks to the open-source community for amazing tools and libraries

---

<div align="center">

### ⭐ Star this repo if you find it interesting!

**TalentVerse** - *Where Skills Meet, and Everyone Grows*

Made with ❤️ and lots of ☕

</div>