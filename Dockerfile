# ——— Dev Stage: SDK + Hot-Reload ———
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /app

# Kodlarni konteynerga nusxalaymiz
COPY . .

# Live-reload uchun port
EXPOSE 8080

# Dev environmentda dotnet watch bilan ishga tushadi
CMD ["dotnet", "watch", "run", "--project", "VazirlikWeb.csproj", "--urls=http://0.0.0.0:8080"]


# ——— Build Stage: SDK + Publish ———
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish VazirlikWeb.csproj -c Release -o /app/publish


# ——— Final Stage: Runtime ———
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Faqat build qilingan fayllarni olib kelamiz
COPY --from=build /app/publish .

# Runtime port
EXPOSE 8080

# Productionda shu buyruq bilan ishlaydi
ENTRYPOINT ["dotnet", "VazirlikWeb.dll"]
