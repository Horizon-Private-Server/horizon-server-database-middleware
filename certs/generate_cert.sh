openssl genpkey -algorithm RSA -out private.key
openssl req -new -key private.key -out server.csr
openssl x509 -req -days 1000 -in server.csr -signkey private.key -out server.crt

# Verify
echo "Verifying ..."
openssl x509 -in server.crt -noout -text

# Create pfx
echo "Creating pfx ..."
openssl pkcs12 -export -out server.pfx -inkey private.key -in server.crt
