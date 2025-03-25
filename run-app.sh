#!/bin/bash
cd "$(dirname "$0")"
dotnet build ParkIRC.Web/ParkIRC.Web.csproj || exit 1
dotnet run --project ParkIRC.Web/ParkIRC.Web.csproj --environment Development 