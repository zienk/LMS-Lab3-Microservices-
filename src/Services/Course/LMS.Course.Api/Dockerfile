FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Course/LMS.Course.Api/LMS.Course.Api.csproj", "Services/Course/LMS.Course.Api/"]
COPY ["src/Services/Course/LMS.Course.Application/LMS.Course.Application.csproj", "Services/Course/LMS.Course.Application/"]
COPY ["src/Services/Course/LMS.Course.Domain/LMS.Course.Domain.csproj", "Services/Course/LMS.Course.Domain/"]
COPY ["src/Services/Course/LMS.Course.Infrastructure/LMS.Course.Infrastructure.csproj", "Services/Course/LMS.Course.Infrastructure/"]
COPY ["src/Services/Course/LMS.Course.GrpcClient/LMS.Course.GrpcClient.csproj", "Services/Course/LMS.Course.GrpcClient/"]
COPY ["src/Shared/LMS.Shared.Contracts/LMS.Shared.Contracts.csproj", "Shared/LMS.Shared.Contracts/"]
COPY ["protos/student.proto", "/protos/student.proto"]
RUN dotnet restore "Services/Course/LMS.Course.Api/LMS.Course.Api.csproj"
COPY src/ .
COPY protos/ /protos/
RUN dotnet publish "Services/Course/LMS.Course.Api/LMS.Course.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LMS.Course.Api.dll"]
