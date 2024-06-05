# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the solution and project files to the container
COPY ["solarmhc/solarmhc.csproj", "solarmhc/"]
COPY ["WebScraper/WebScraper.csproj", "WebScraper/"]

# Restore dependencies
RUN dotnet restore "solarmhc/solarmhc.csproj"

# Copy the remaining source code to the container
COPY . .

# Set the working directory to the solarmhc project folder
WORKDIR "/src/solarmhc"

# Build the project
RUN dotnet build "solarmhc.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "solarmhc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Install Chrome and ChromeDriver
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    unzip \
    && wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list' \
    && apt-get update && apt-get install -y \
    google-chrome-stable \
    && wget -O /tmp/chromedriver.zip https://chromedriver.storage.googleapis.com/114.0.5735.90/chromedriver_linux64.zip \
    && unzip /tmp/chromedriver.zip -d /usr/local/bin/ \
    && rm /tmp/chromedriver.zip \
    && apt-get clean

# Expose the necessary ports
EXPOSE 80
EXPOSE 443

# Copy the published files to the runtime image
COPY --from=publish /app/publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "solarmhc.dll"]
