# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY LoopAPI.csproj ./
RUN dotnet restore "LoopAPI.csproj"

# Copy everything else
COPY . .
RUN dotnet publish "LoopAPI.csproj" -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "LoopAPI.dll"]
