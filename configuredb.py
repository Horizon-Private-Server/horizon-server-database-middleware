print("running")
import os
import pyodbc
import pandas as pd
import time
import subprocess
import json

USER = os.getenv('DB_USER')
PASSWORD = os.getenv('DB_PASSWORD')
SERVER = os.getenv('DB_SERVER')
MIDDLEWARE_USER = os.getenv('MIDDLEWARE_USER')
MIDDLEWARE_PASSWORD = os.getenv('MIDDLEWARE_PASSWORD')
MIDDLEWARE_SERVER_IP = os.getenv('MIDDLEWARE_SERVER_IP')

print(f"USER: {USER}")
#print(f"PASSWORD: {PASSWORD}")
print(f"SERVER: {SERVER}")
print(f"MIDDLEWARE_USER: {MIDDLEWARE_USER}")

#===============================================
# Start middleware process
#===============================================
print("Starting middleware process ...")
process = subprocess.Popen(['dotnet', 'Horizon.Database.dll'])
time.sleep(3)

#===============================================
# Create admin middleware user
#===============================================
print(f"Creating admin user ...")
curl_command = f'curl --insecure -X POST "{MIDDLEWARE_SERVER_IP}/Account/createAccount" -H  "accept: */*" -H  "Content-Type: application/json-patch+json" -d "{{\\"AccountName\\":\\"{MIDDLEWARE_USER}\\",\\"AccountPassword\\":\\"{MIDDLEWARE_PASSWORD}\\",\\"MachineId\\":\\"1\\",\\"MediusStats\\":\\"1\\",\\"AppId\\":0,\\"PasswordPreHashed\\":false,\\"ResetPasswordOnNextLogin\\":false}}"'
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

#===============================================
# Update Everything from the config
#===============================================
with open('/code/docker_config.json', 'r') as f:
    config = json.loads(f.read())

#===============================================
# Set EnableEncryption to 0
# Set CreateAccountOnNotFound to 1
#===============================================

#   await PostDbAsync($"api/Keys/setSettings?appId={appId}", settings);
for app in config['apps']:
    appId = app['id']
    print(f"Setting encryption and CreateAccountOnNotFound ...")
    curl_command = f'curl --insecure -X POST "{MIDDLEWARE_SERVER_IP}/api/Keys/setSettings?appId={appId}" -H  "accept: */*" -H  "Content-Type: application/json-patch+json" -d "{{\\"EnableEncryption\\":\\"False\\",\\"CreateAccountOnNotFound\\":\\"True\\",\\"ClientLongTimeoutSeconds\\":50,\\"ClientTimeoutSeconds\\":50,\\"DmeTimeoutSeconds\\":50,\\"KeepAliveGracePeriodSeconds\\":50,\\"GameTimeoutSeconds\\":50}}"'
    print(curl_command)
    os.system(curl_command)



#### Insert APP ID into dim table
# Multiple groups not supported
print("Inserting app ids ...")
for app in config['apps']:
    APP_ID = app['id']
    APP_NAME = app['name']
    APP_GROUP = app['app_group']
    # If it already exists, ignore
    if pd.read_sql(f"select count(*) from keys.dim_app_ids where app_id = {APP_ID}", cnxn).values[0][0] == 0:
        cursor.execute(f"INSERT INTO keys.dim_app_ids VALUES({APP_ID}, '{APP_NAME}', {APP_GROUP})")

#### Insert Channels
print("Inserting channels ...")
for channel in config['channels']:
    id = channel['id']
    app_id = channel['app_id']
    name = channel['name']
    max_players = channel['max_players']
    generic_field_1 = channel['generic_field_1']
    generic_field_2 = channel['generic_field_2']
    generic_field_3 = channel['generic_field_3']
    generic_field_4 = channel['generic_field_4']
    generic_field_filter = channel['generic_field_filter']

    if pd.read_sql(f"select count(*) from world.channels where app_id = {app_id} and name = '{name}'", cnxn).values[0][0] == 0:
        cursor.execute(f"""
            INSERT INTO world.channels VALUES(
                {id},
                {app_id},
                '{name}',
                {max_players},
                {generic_field_1},
                {generic_field_2},
                {generic_field_3},
                {generic_field_4},
                {generic_field_filter}
            )
        """)

#### Insert Locations
print("Inserting locations ...")
for location in config['locations']:
    id = location['id']
    app_id = location['app_id']
    name = location['name']
    # If location exists, ignore
    if pd.read_sql(f"select count(*) from world.locations where app_id = {app_id} and name = '{name}'", cnxn).values[0][0] == 0:
        cursor.execute(f"""
            INSERT INTO world.locations VALUES(
                {id},
                {app_id},
                '{name}'
            )
        """)

cnxn.commit()

cursor.close()
cnxn.close()

print("Done!")

print("Communicating with process ...")
process.communicate()
