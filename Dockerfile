# Use official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file and project files first (for better caching)
# Note: Source paths are relative to the build context (root of repo)
COPY ["src/SmsRazor.slnx", "src/"]
COPY ["src/SmsRazor.DAL/SmsRazor.DAL.csproj", "src/SmsRazor.DAL/"]
COPY ["src/SmsRazor.BLL/SmsRazor.BLL.csproj", "src/SmsRazor.BLL/"]
COPY ["src/SmsRazor.WebApp/SmsRazor.WebApp.csproj", "src/SmsRazor.WebApp/"]

# Restore dependencies
# We restore the main project explicitly to avoid .slnx compatibility issues in .NET 8 SDK
WORKDIR /app/src
RUN dotnet restore "SmsRazor.WebApp/SmsRazor.WebApp.csproj"

# Copy the rest of the source code
WORKDIR /app
COPY . .

# Build and publish the WebApp
WORKDIR /app/src/SmsRazor.WebApp
RUN dotnet publish "SmsRazor.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use official ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files from build stage
COPY --from=build /app/publish .

# Set environment variable to listen on port 80
ENV ASPNETCORE_URLS=http://+:80

# Entry point
ENTRYPOINT ["dotnet", "SmsRazor.WebApp.dll"]
