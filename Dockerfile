# ——— Dev Stage: SDK + Hot-Reload ———
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /app

# dotnet ef tool ni o‘rnatamiz
RUN dotnet tool install --global dotnet-ef \
    && echo 'export PATH="$PATH:/root/.dotnet/tools"' >> ~/.bashrc
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .
EXPOSE 8080
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

COPY --from=build /app/publish .

COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

CMD ["/app/entrypoint.sh"]

EXPOSE 8080
