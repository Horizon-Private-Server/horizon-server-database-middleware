#! /bin/sh

cp /appsettings.json /code/Horizon.Database/appsettings.json
cp /appsettings.json /code/out/appsettings.json

# # Start Middleware
echo "Starting Database Middleware ..."
dotnet Horizon.Database.dll
