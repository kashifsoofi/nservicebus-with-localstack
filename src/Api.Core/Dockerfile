FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /build

# copy csproj and restore as distinct layers
COPY *.sln nuget.config ./

# Copy the main source project files
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done

RUN dotnet sln FullDuplex.Core.sln remove docker-compose.dcproj

# Restore to cache the layers
RUN dotnet restore

# Copy all the source code and build
COPY ./src ./src

RUN dotnet build -c Release --no-restore

FROM build AS publish
RUN dotnet publish "./src/Api.Core/Api.Core.csproj" -c Release -o /app --no-restore

# Api image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=publish /app ./
ENTRYPOINT ["dotnet", "Api.Core.dll"]