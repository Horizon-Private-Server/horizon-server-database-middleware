#! /bin/sh
# Running create scripts
sed -i "s|https://localhost:10000|${MIDDLEWARE_SERVER}|g" /code/out/appsettings.json

echo "Running configuredb.py ..."
#python3 -u /code/configuredb.py

#sleep 2


# # Start DME
echo "Starting Database Middleware ..."
dotnet Horizon.Database.dll
