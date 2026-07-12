FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Identity/LMS.Identity.Api/LMS.Identity.Api.csproj", "Services/Identity/LMS.Identity.Api/"]
COPY ["src/Services/Identity/LMS.Identity.Application/LMS.Identity.Application.csproj", "Services/Identity/LMS.Identity.Application/"]
COPY ["src/Services/Identity/LMS.Identity.Domain/LMS.Identity.Domain.csproj", "Services/Identity/LMS.Identity.Domain/"]
COPY ["src/Services/Identity/LMS.Identity.Infrastructure/LMS.Identity.Infrastructure.csproj", "Services/Identity/LMS.Identity.Infrastructure/"]
COPY ["src/Shared/LMS.Shared.Contracts/LMS.Shared.Contracts.csproj", "Shared/LMS.Shared.Contracts/"]
RUN dotnet restore "Services/Identity/LMS.Identity.Api/LMS.Identity.Api.csproj"
COPY src/ .
RUN dotnet publish "Services/Identity/LMS.Identity.Api/LMS.Identity.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LMS.Identity.Api.dll"]
