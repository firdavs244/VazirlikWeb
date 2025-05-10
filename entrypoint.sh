#!/bin/bash

echo "⏳ Migratsiyalarni bazaga qo‘llayapman..."
dotnet ef database update

echo "🚀 Ilovani ishga tushiryapman..."
dotnet watch run --project VazirlikWeb.csproj --urls=http://0.0.0.0:8080
