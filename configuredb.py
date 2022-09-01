print("running")
import os
import pyodbc
import pandas as pd

USER = os.getenv('DB_USER')
PASSWORD = os.getenv('MSSQL_SA_PASSWORD')
SERVER = os.getenv('DB_SERVER')
MIDDLEWARE_USER = os.getenv('MIDDLEWARE_USER')
APP_ID = os.getenv("APP_ID")
APP_NAME = os.getenv("APP_NAME")


cnxn_str = f"DRIVER={{ODBC Driver 17 for SQL Server}};Server={SERVER};Database=Medius_Database;UID={USER};PWD={PASSWORD};"
            
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
