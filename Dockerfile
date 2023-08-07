FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["IntraCalendarGrabber/IntraCalendarGrabber.csproj", "IntraCalendarGrabber/"]
RUN dotnet restore "IntraCalendarGrabber/IntraCalendarGrabber.csproj"
COPY . .
WORKDIR "/src/IntraCalendarGrabber"
RUN dotnet build "IntraCalendarGrabber.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IntraCalendarGrabber.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN sed 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/' /etc/ssl/openssl.cnf > /etc/ssl/openssl.cnf.changed && mv /etc/ssl/openssl.cnf.changed /etc/ssl/openssl.cnf
ENTRYPOINT ["dotnet", "IntraCalendarGrabber.dll"]
