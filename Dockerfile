# Use official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file and project files first (for better caching)
COPY ["student_management_system.sln", "."]
COPY ["src/SmsRazor.DAL/SmsRazor.DAL.csproj", "src/SmsRazor.DAL/"]
COPY ["src/SmsRazor.BLL/SmsRazor.BLL.csproj", "src/SmsRazor.BLL/"]
COPY ["src/SmsRazor.WebApp/SmsRazor.WebApp.csproj", "src/SmsRazor.WebApp/"]

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the WebApp
WORKDIR "/src/src/SmsRazor.WebApp"
RUN dotnet publish "SmsRazor.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use official ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copy published files from build stage
COPY --from=build /app/publish .

# Set environment variable to listen on port 80
ENV ASPNETCORE_URLS=http://+:80

# Entry point
ENTRYPOINT ["dotnet", "SmsRazor.WebApp.dll"]
