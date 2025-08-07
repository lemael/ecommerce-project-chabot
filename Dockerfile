# Étape 1 - Build du frontend React
FROM node:18-alpine as frontend-builder
WORKDIR /app
COPY ecommerce-frontend/package.json .
RUN npm install --silent
COPY ecommerce-frontend .
RUN npm run build

# Étape 2 - Build du backend .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 as backend-builder
WORKDIR /app
COPY EcommerceChatbot/EcommerceChatbot.csproj .
RUN dotnet restore
COPY EcommerceChatbot .
COPY --from=frontend-builder /app/build ./www
RUN dotnet publish -c Release -o out /p:EnvironmentName=Production

# Étape 3 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=backend-builder /app/out .
ENV ASPNETCORE_URLS=http://*:$PORT
EXPOSE $PORT
ENTRYPOINT ["dotnet", "EcommerceChatbot.dll"]
