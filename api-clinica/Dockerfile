# Etapa 1: compilar y publicar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY PawVetClinica.csproj ./
RUN dotnet restore PawVetClinica.csproj
COPY . ./
RUN dotnet publish PawVetClinica.csproj -c Release -o /app --no-restore

# Etapa 2: imagen liviana solo con el runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PawVetClinica.dll"]
