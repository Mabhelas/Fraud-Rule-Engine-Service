# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj into project folder and restore
COPY ["Fraud Rule Engine Service/Fraud Rule Engine Service.csproj", "Fraud Rule Engine Service/"]
RUN dotnet restore "Fraud Rule Engine Service/Fraud Rule Engine Service.csproj"

# copy everything and publish from project folder
COPY . .
WORKDIR "/src/Fraud Rule Engine Service"
RUN dotnet publish "Fraud Rule Engine Service.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Fraud Rule Engine Service.dll"]