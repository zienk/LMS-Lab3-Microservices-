FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ApiGateway/LMS.ApiGateway/LMS.ApiGateway.csproj", "ApiGateway/LMS.ApiGateway/"]
RUN dotnet restore "ApiGateway/LMS.ApiGateway/LMS.ApiGateway.csproj"
COPY src/ApiGateway/LMS.ApiGateway/ ApiGateway/LMS.ApiGateway/
WORKDIR "/src/ApiGateway/LMS.ApiGateway"
RUN dotnet publish "LMS.ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LMS.ApiGateway.dll"]
