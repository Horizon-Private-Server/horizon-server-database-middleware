import os
import requests



data = {
    "AccountName": os.getenv("MIDDLEWARE_ADMIN_USERNAME"),
    "Password": os.getenv("MIDDLEWARE_ADMIN_PASSWORD")
  }

response = requests.post('https://216.146.25.143:10001/Account/authenticate', json=data, verify=False)
if response.status_code == 200:
    token = response.json()["Token"]
    header = { "Authorization": f"Bearer {token}" }

    print("Success auth!")
else:
    print(response.status_code)


accountName = input()
accountPassword = input()


payload = {
  "AccountName": accountName,
  "AccountPassword": accountPassword,
  "MachineId": "",
  "MediusStats": "",
  "AppId": 0,
  "PasswordPreHashed": False,
  "ResetPasswordOnNextLogin": False
}


response = requests.post(f'https://{os.getenv("MIDDLEWARE_ADMIN_IP")}:10001/Account/createAccount', json=payload, headers=header, verify=False)
if response.status_code == 200:
    print("Success!")
    print(response)
else:
    print(response.status_code)
