# 🤖 SignLanguage.G27

**SignLanguage.G27** is an AI-powered system that translates sign language into readable text.  
It is built using **ASP.NET Core** for the backend and **Angular** for the frontend, following a clean architecture design.

---

## 📁 Project Structure

SignLanguage.G27.Solution/
├── SignLanguage.APIs/ # ASP.NET Core Web API
├── SignLanguage.Core/ # Entities and Interfaces
├── SignLanguage.Infrastructure/ # Data access, Redis, Email, JWT, etc.
├── SignLanguage.Application/ # Application layer logic and services
├── SignLanguage.sln # Visual Studio Solution file



---

## 🚀 Getting Started (Backend)

> ✅ Before running the backend, make sure you have installed:
> - [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
> - Redis (local or cloud)
> - SQL Server (local or remote)
> - SMTP service for email (e.g., Gmail)
> - Google OAuth credentials (Client ID & Secret)
> - Telegram Bot Token

---

### 🛠️ 1. Clone the Repository

bash
git clone https://github.com/YourUser/SignLanguage.G27.Clean.git
cd SignLanguage.G27.Solution
🛠️ 2. Setup Your Own Configuration
The file appsettings.json is not included in the repository for security reasons.
🧩 In your appsettings.json, you must add your own configuration values:

ConnectionStrings
JWT: SecretKey, ValidIssuer, ValidAudience
Redis: Host, Port, Password
EmailSettings: SMTP and Gmail credentials
TelegramBot: BotToken
Google OAuth: ClientId, ClientSecret

⚠️ You are responsible for setting your own secure values.
Never push your real secrets to GitHub.

▶️ 3. Run the API
Navigate to the API project folder and run:


cd SignLanguage.APIs
dotnet run
By default, the API will be available at https://localhost:5001 or as defined in launchSettings.json.

✨ Features
🧠 Sign Language to Text Conversion

🔐 External Login (Google, Telegram)

🔁 Password Reset via Email or Telegram

📬 OTP via Gmail SMTP

📦 Redis caching for OTP and performance

🧱 Clean layered architecture (Core, Application, Infrastructure, API)

📥 Support for SMS-based login (optional)

🔐 Secrets & Configuration
The file appsettings.json is intentionally excluded.

You must manually create your own version of this file using the provided template.

Store all API keys, credentials, and tokens securely on your machine.

Do not share or commit sensitive data.

👨‍💻 Contributors
Mohab M. Belkan – Backend Developer (.NET Core)

Frontend Team – Developed with Angular

📄 License
This project is intended for educational and demonstration purposes only.
If you plan to use it in production, make sure to apply proper security practices and thorough testing.


