# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the solution and project files to the container
COPY ["solarmhc/solarmhc.Web.csproj", "solarmhc/"]
COPY ["WebScraper/solarmhc.Models.csproj", "WebScraper/"]

# Restore dependencies
RUN dotnet restore "solarmhc/solarmhc.Web.csproj"

# Copy the remaining source code to the container
COPY . .

# Set the working directory to the solarmhc project folder
WORKDIR "/src/solarmhc"

# Build the project
RUN dotnet build "solarmhc.Web.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "solarmhc.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Expose the necessary ports
EXPOSE 80
EXPOSE 443

# Copy the published files to the runtime image
COPY --from=publish /app/publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "solarmhc.Web.dll"]
