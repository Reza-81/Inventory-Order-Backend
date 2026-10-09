FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["InventoryOrder.Api/InventoryOrder.Api.csproj", "InventoryOrder.Api/"]
COPY ["InventoryOrder.Application/InventoryOrder.Application.csproj", "InventoryOrder.Application/"]
COPY ["InventoryOrder.Domain/InventoryOrder.Domain.csproj", "InventoryOrder.Domain/"]
COPY ["InventoryOrder.Infrastructure/InventoryOrder.Infrastructure.csproj", "InventoryOrder.Infrastructure/"]
RUN dotnet restore "InventoryOrder.Api/InventoryOrder.Api.csproj"

COPY . .
RUN dotnet publish "InventoryOrder.Api/InventoryOrder.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InventoryOrder.Api.dll"]
