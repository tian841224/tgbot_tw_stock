FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Install sudo, fonts, and clean up in a single RUN command
RUN apt-get update && \
    apt-get install -y --no-install-recommends sudo fonts-dejavu && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TGBot_TW_Stock_Polling/TGBot_TW_Stock_Polling.csproj", "TGBot_TW_Stock_Polling/"]
RUN dotnet restore "TGBot_TW_Stock_Polling/TGBot_TW_Stock_Polling.csproj"

COPY TGBot_TW_Stock_Polling/. TGBot_TW_Stock_Polling/
WORKDIR "/src/TGBot_TW_Stock_Polling"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install PowerShell
RUN apt-get update && \
    apt-get install -y wget && \
    wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y powershell && \
    rm -rf /var/lib/apt/lists/*

# Install Playwright dependencies
RUN pwsh -Command "./playwright.ps1 install --with-deps chromium"

CMD ASPNETCORE_URLS=http://*:$PORT dotnet TGBot_TW_Stock_Polling.dll