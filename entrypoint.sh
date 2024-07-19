#! /bin/sh
sed -i "s|https://localhost:10000|${HORIZON_MIDDLEWARE_SERVER}|g" /code/out/appsettings.json

# # Start DME
echo "Starting Database Middleware ..."
dotnet Horizon.Database.dll
