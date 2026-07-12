FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 6001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/Student/LMS.Student.Api/LMS.Student.Api.csproj", "Services/Student/LMS.Student.Api/"]
COPY ["src/Services/Student/LMS.Student.Application/LMS.Student.Application.csproj", "Services/Student/LMS.Student.Application/"]
COPY ["src/Services/Student/LMS.Student.Domain/LMS.Student.Domain.csproj", "Services/Student/LMS.Student.Domain/"]
COPY ["src/Services/Student/LMS.Student.Infrastructure/LMS.Student.Infrastructure.csproj", "Services/Student/LMS.Student.Infrastructure/"]
COPY ["src/Services/Student/LMS.Student.Grpc/LMS.Student.Grpc.csproj", "Services/Student/LMS.Student.Grpc/"]
COPY ["src/Shared/LMS.Shared.Contracts/LMS.Shared.Contracts.csproj", "Shared/LMS.Shared.Contracts/"]
COPY ["protos/student.proto", "/protos/student.proto"]
RUN dotnet restore "Services/Student/LMS.Student.Api/LMS.Student.Api.csproj"
COPY src/ .
COPY protos/ /protos/
RUN dotnet publish "Services/Student/LMS.Student.Api/LMS.Student.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LMS.Student.Api.dll"]
