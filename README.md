# 🛡️ SentinelAI — Defence Cyber Incident & Safety Portal

[![Build Status](https://img.shields.io/badge/Build-Succeeded-success?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Framework](https://img.shields.io/badge/ASP.NET%20Core-10.0-blue?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/SQL%20Server-2022-red?style=flat-square&logo=microsoft-sql-server)](https://www.microsoft.com/en-us/sql-server)
[![AI Engine](https://img.shields.io/badge/AI-Gemini%202.5%20Flash-gold?style=flat-square&logo=google-gemini)](https://ai.google.dev/)

**SentinelAI** is an advanced, AI-powered cybersecurity incident reporting, assessment, and awareness platform designed specifically for defence personnel, veterans, and unit commanding teams. It streamlines cyber threat reporting, automates threat classification, recommends immediate mitigation steps, and provides command dashboards to monitor local and regional cyber security postures.

---

## 🌟 Key Features

### 1. 🤖 AI-Powered Threat Analysis
- **Automatic Incident Classification**: Leveraging the **Google Gemini 2.5 Flash API**, SentinelAI processes free-text descriptions of incidents in real-time to identify the precise threat category (e.g., *Phishing, Malware, Espionage, Fraud, Social Engineering*).
- **Risk Assessment**: Instantly computes risk levels (*Low, Medium, High, Critical*) to flag severe incidents for immediate escalation.
- **Dynamic Mitigation Engine**: Generates context-aware, actionable immediate steps for defence personnel (e.g., network isolation protocols, credential resets).
- **Keyword Fallback Safety Net**: Implements local rule-based regex parsing to ensure the application remains functional even in offline mode or during API rate limits.
- <img width="1191" height="877" alt="image" src="https://github.com/user-attachments/assets/53fcedbc-8f57-48e8-bd14-2ce12b2c3ebb" />


### 2. 🗺️ Threat Heat Map (Admin Restricted)
- **Regional Threat Insights**: A visually premium dashboard displaying cybersecurity threat distribution across India.
- **Top Affected Leaderboard**: Tracks and displays the top 5 states by incident count with visual progress bars color-coded by peak risk level.
- **Interactive Visualizations**: Powered by **Chart.js**, featuring:
  - Stacked horizontal bar chart for state-wise threat density.
  - Risk level distribution doughnut chart with dynamic center-text totals.
  - Attack type distribution chart.
- **State-wise Incident Breakdown**: Full tabular data with customized threat level badges and quick filtering shortcuts.

### 3. 💬 SentinelAI Cyber Assistant
- **Defence Cyber Advisor**: An in-portal chatbot fine-tuned to answer security queries, outline safe digital practices, and advise on securing messaging and banking apps.
- **Strict Scope Guardrails**: Rejects off-topic queries to prevent abuse, remaining dedicated purely to cybersecurity and defence safety.
- **Escalation Guidance**: Automatically attaches standardized unit CERT and national helpline contacts (`1930`) to sensitive inquiries.

### 4. 📊 CERT Command Dashboard (Admin Panel)
- **Role-Based Access**: Restricted to administrative and security personnel.
- **System-Wide Monitoring**: Summarizes metrics (Total, Critical, High, Pending, Investigating, Resolved) with animated status cards.
- **Inline Operations**: Allows security officers to review incident files, update ticket statuses inline, and track resolution lifecycles.

### 5. 📂 Incident Reporting & Media Evidence
- Secure portal enabling users to report incidents, specify location (State), and upload visual evidence or attachments for CERT investigation.

---

## 🛠️ Technology Stack

| Layer | Technology | Description |
| :--- | :--- | :--- |
| **Core Framework** | ASP.NET Core 10.0 MVC | High-performance model-view-controller framework |
| **ORM** | Entity Framework Core | Database queries, context mapping, and migrations |
| **Database** | MS SQL Server / Express | Relational database engine for secure data storage |
| **Identity & Access** | ASP.NET Core Identity | Secure passwords, cookie-based sessions, role checks |
| **AI Processing** | Google Gemini 2.5 Flash API | Direct HTTP integration for incident triage and chatbot |
| **Styling (CSS)** | Custom CSS Variables + Bootstrap 5 | Premium dark navy layout with gold highlights |
| **Charts & Visuals** | Chart.js & FontAwesome v6 | For heat map analytics and iconography |

---

## 📂 Project Architecture

```
SentinelAI/
│
├── Controllers/
│   ├── AccountController.cs         # Registration, Login, and Logout
│   ├── AdminController.cs           # Command dashboard, inline updates [Admin]
│   ├── CyberAssistantController.cs  # Interactive chatbot logic
│   ├── DashboardController.cs        # User dashboard statistics
│   ├── HomeController.cs           # Landing/Hero pages
│   └── ThreatMapController.cs       # Heat Map analytics endpoints [Admin]
│
├── Data/
│   └── ApplicationDbContext.cs      # EF Core database mapping
│
├── Models/
│   ├── Complaint.cs                 # Incident model schemas
│   └── ViewModels/                  # Dedicated payload views
│
├── Services/
│   ├── GeminiService.cs             # AI analysis & chatbot API interactions
│   └── FileUploadService.cs         # Evidence file processing
│
├── Views/
│   ├── Admin/                       # Admin command panel views
│   ├── Shared/_Layout.cshtml        # Main shell navigation layout
│   └── ThreatMap/Index.cshtml       # Chart.js Heat Map template
│
└── wwwroot/
    ├── css/site.css                 # Dark navy theme and dynamic design tokens
    └── js/site.js                   # Client side form validation and chat UX
```

---

## 🚀 Installation & Local Setup

### Prerequisites
- **.NET SDK**: 10.0 or higher
- **Database**: SQL Server LocalDB or SQL Server Express
- **API Key**: A valid [Google Gemini API Key](https://aistudio.google.com/)

### 1. Clone the Repository
```bash
git clone https://github.com/Arunachalam9626/SentinelAI.git
cd SentinelAI
```

### 2. Configure Database and API Keys
Open `appsettings.json` and customize the connection string and API credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=SentinelAIDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "GeminiApiKey": "PASTE_YOUR_GEMINI_API_KEY_HERE",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
> [!TIP]
> If using SQL Server LocalDB, you can adjust the connection string server value to `(localdb)\\MSSQLLocalDB`.

### 3. Run Migrations & Apply Database Schema
Execute the database update to create identity tables, complaints, and seed-related tables:
```bash
dotnet ef database update
```
*(If the CLI tools aren't globalized, build the project and it will automatically apply migrations and database seeding upon startup)*

### 4. Build and Run the Application
```bash
dotnet build
dotnet run
```
By default, the application will launch and be accessible at:
- **HTTPS**: `https://localhost:49972`
- **HTTP**: `http://localhost:49973`

---

## 🔒 Test Credentials & Roles

Upon startup, the database is automatically seeded with default roles, a secure administrator account, and dummy threat reports.

* **Admin Role Account**:
  * **Email / Username**: `admin@sentinel.mil`
  * **Password**: `Admin@123`
* **Regular Role Account**:
  * Register dynamically using the **Register** button in the top-right header menu.

---

## 🛡️ Security & Role-Based Access Control

SentinelAI enforces strict security controls:
- **General Access**: Authenticated users can report threats, use the Cyber Assistant, and review their personal dashboard.
- **Admin Access**: Only users with the `Admin` role can view the **Threat Heat Map** or access the **CERT Command Dashboard**. Non-admin requests to these endpoints will trigger a `403 Access Denied` response.
