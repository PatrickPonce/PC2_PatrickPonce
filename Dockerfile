# Usa la imagen oficial de .NET para la compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Establece el directorio de trabajo en /app
WORKDIR /app

# Copia los archivos del proyecto y restaura las dependencias
COPY *.csproj .
RUN dotnet restore

# Copia el resto de los archivos y compila
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Usa una imagen de tiempo de ejecución para el servicio
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Define el punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "PC2_PatrickPonce.dll"]