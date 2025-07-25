﻿# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["SlackAzureIntegration.csproj", "."]
RUN dotnet restore "SlackAzureIntegration.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "SlackAzureIntegration.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SlackAzureIntegration.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "SlackAzureIntegration.dll"]