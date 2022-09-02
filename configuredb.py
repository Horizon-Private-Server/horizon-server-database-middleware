print("running")
import os
import pyodbc
import pandas as pd
import time
import subprocess

USER = os.getenv('DB_USER')
PASSWORD = os.getenv('DB_PASSWORD')
SERVER = os.getenv('DB_SERVER')
MIDDLEWARE_USER = os.getenv('MIDDLEWARE_USER')
MIDDLEWARE_PASSWORD = os.getenv('MIDDLEWARE_PASSWORD')
MIDDLEWARE_SERVER_IP = os.getenv('MIDDLEWARE_SERVER_IP')
APP_ID = os.getenv("APP_ID")
APP_NAME = os.getenv("APP_NAME")

print(f"USER: {USER}")
print(f"PASSWORD: {PASSWORD}")
print(f"SERVER: {SERVER}")
print(f"MIDDLEWARE_USER: {MIDDLEWARE_USER}")
print(f"APP_ID: {APP_ID}")
print(f"APP_NAME: {APP_NAME}")

#--------------- Start the middleware
print("Starting middleware process ...")
process = subprocess.Popen(['dotnet', 'Horizon.Database.dll'])
time.sleep(3)

print(f"Creating admin user ...")
curl_command = f'curl --insecure -X POST "{MIDDLEWARE_SERVER_IP}/Account/createAccount" -H  "accept: */*" -H  "Content-Type: application/json-patch+json" -d "{{\\"AccountName\\":\\"{MIDDLEWARE_USER}\\",\\"AccountPassword\\":\\"{MIDDLEWARE_PASSWORD}\\",\\"MachineId\\":\\"1\\",\\"MediusStats\\":\\"1\\",\\"AppId\\":0,\\"PasswordPreHashed\\":false}}"'
print(curl_command)
os.system(curl_command)


print("Adding role and app id ...")
cnxn_str = f"DRIVER={{ODBC Driver 17 for SQL Server}};Server={SERVER};Database=Medius_Database;UID={USER};PWD={PASSWORD};"
print(cnxn_str)
cnxn = pyodbc.connect(cnxn_str)

cursor = cnxn.cursor()

account_id = pd.read_sql(f"SELECT account_id FROM accounts.account where account_name = '{MIDDLEWARE_USER}'", cnxn).values[0][0]

role_id = pd.read_sql("SELECT role_id FROM keys.roles where role_name = 'database'", cnxn).values[0][0]

print(account_id)
print(role_id)
print(type(role_id))

# If the account_id + role_id already exists, don't add it
if pd.read_sql(f"select count(*) from accounts.user_role where account_id = {account_id} and role_id = {role_id}", cnxn).values[0][0] == 0:
    cursor.execute(f"INSERT INTO accounts.user_role VALUES({account_id}, {role_id}, GETDATE(), GETDATE(), null)")


#### Insert APP ID into dim table
if pd.read_sql(f"select count(*) from keys.dim_app_ids where app_id = {APP_ID}", cnxn).values[0][0] == 0:
    cursor.execute(f"INSERT INTO keys.dim_app_ids VALUES({APP_ID}, '{APP_NAME}', 1)")


cnxn.commit()

cursor.close()
cnxn.close()

print("Done!")

print("Communicating with process ...")
process.communicate()
