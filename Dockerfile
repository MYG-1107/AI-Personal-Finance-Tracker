FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["AIPersonalFinanceTracker.Shared/AIPersonalFinanceTracker.Shared.csproj", "AIPersonalFinanceTracker.Shared/"]
COPY ["AIPersonalFinanceTracker.ML/AIPersonalFinanceTracker.ML.csproj", "AIPersonalFinanceTracker.ML/"]
COPY ["AIPersonalFinanceTracker.Client/AIPersonalFinanceTracker.Client.csproj", "AIPersonalFinanceTracker.Client/"]
COPY ["AIPersonalFinanceTracker.Api/AIPersonalFinanceTracker.Api.csproj", "AIPersonalFinanceTracker.Api/"]
RUN dotnet restore "AIPersonalFinanceTracker.Api/AIPersonalFinanceTracker.Api.csproj"

# Copy source code and build publish output
COPY . .
RUN dotnet publish "AIPersonalFinanceTracker.Api/AIPersonalFinanceTracker.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "AIPersonalFinanceTracker.Api.dll"]
