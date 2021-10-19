FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["AOS_Lab_1/AOS_Lab_1.csproj", "AOS_Lab_1/"]
RUN dotnet restore "AOS_Lab_1/AOS_Lab_1.csproj"
COPY . .
WORKDIR "/src/AOS_Lab_1"
RUN dotnet build "AOS_Lab_1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AOS_Lab_1.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AOS_Lab_1.dll"]
