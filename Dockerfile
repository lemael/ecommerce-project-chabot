# Étape 1 : Build du frontend
FROM node:18 as frontend
WORKDIR /app
COPY ecommerce-frontend/ .
RUN npm install && npm run build

# Étape 2 : Build du backend .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 as backend
WORKDIR /src
COPY EcommerceChatbot/ .
RUN dotnet publish -c Release -o /app/out

# Étape 3 : Image finale
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Installer NGINX et dépendances
RUN apt-get update && apt-get install -y nginx

# Copier les artefacts
COPY --from=frontend /app/build /var/www/html
COPY --from=backend /app/out .
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Ports et commande
EXPOSE 10000
CMD service nginx start && dotnet EcommerceChatbot.dll