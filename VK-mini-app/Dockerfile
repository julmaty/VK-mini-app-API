#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app/publish
RUN dotnet dev-certs https --clean
RUN dotnet dev-certs https -ep VK-mini-app.pfx  -p crypticpassword
WORKDIR /src
COPY ["VK-mini-app/VK-mini-app.csproj", "VK-mini-app/"]
RUN dotnet restore "./VK-mini-app/VK-mini-app.csproj"
COPY . .
WORKDIR "/src/VK-mini-app"
RUN dotnet build "./VK-mini-app.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VK-mini-app.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER root
RUN chmod og+r VK-mini-app.pfx
USER app
WORKDIR /app
ENTRYPOINT ["dotnet", "VK-mini-app.dll"]