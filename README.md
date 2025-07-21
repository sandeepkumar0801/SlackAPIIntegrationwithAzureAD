SlackAPIIntegration is a real-world .NET 8 Web API project that demonstrates how to integrate Slack Events API with Azure Active Directory authentication. It showcases a clean architecture, secure webhook handling, and modern development practices suitable for enterprise-grade applications.

Key Features
Slack Events API Integration (message events, mentions, etc.)

Azure AD authentication using Microsoft.Identity.Web

ASP.NET Core 8.0 Web API with Swagger support

Entity Framework Core setup

Exception middleware for error handling

Secure token-based endpoints

Modular and testable folder structure

Getting Started

Prerequisites

.NET 8 SDK

Slack App with Events API and Bot Token

Azure Active Directory App Registration

SQL Server (optional, for DB setup)

Setup Instructions

Clone the repository

git clone https://github.com/yourusername/SlackEventAPI_DotNet.git

cd SlackEventAPI_DotNet

Update appsettings.json

AzureAd section: Replace TenantId, ClientId, and Domain

Slack section: Replace with your bot token

ConnectionStrings: Use your SQL Server connection string (optional)

Restore and run the application.


dotnet restore

dotnet run

Access Swagger UI

http://localhost:5000/swagger

Endpoints

POST /api/slack/events — Handles Slack Events webhook

GET /api/auth/userinfo — Returns authenticated Azure AD user info

Technologies Used

ASP.NET Core 8.0

Microsoft.Identity.Web (Azure AD)

Slack Web API

Entity Framework Core

Swagger / OpenAPI
