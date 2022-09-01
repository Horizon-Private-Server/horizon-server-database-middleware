#! /bin/sh
echo "Waiting for other containers to start ..."
sleep 10

# Running create scripts
echo "Running create scripts ..."
/opt/mssql-tools/bin/sqlcmd -S ${DB_SERVER} -U ${DB_USER} -P "${DB_PASSWORD}" -i ../scripts/CREATE_DATABASE.sql
/opt/mssql-tools/bin/sqlcmd -S ${DB_SERVER} -U ${DB_USER} -P "${DB_PASSWORD}" -i ../scripts/CREATE_TABLES.sql



sed -i "s|https://localhost:10000|${MIDDLEWARE_SERVER}|g" /code/out/appsettings.json

# Start DME
echo "Starting Database Middleware ..."
dotnet Horizon.Database.dll
