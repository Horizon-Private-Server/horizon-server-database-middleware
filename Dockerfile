FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder

RUN apt-get update
RUN apt-get install curl gnupg software-properties-common python3 python3-pip -y
RUN curl https://packages.microsoft.com/keys/microsoft.asc > gpg_key.txt && apt-key add gpg_key.txt
RUN curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list | tee /etc/apt/sources.list.d/msprod.list
RUN apt-get update
ENV ACCEPT_EULA=Y
RUN apt-get install mssql-tools unixodbc-dev -y
RUN echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc

RUN dotnet dev-certs https
RUN dotnet dev-certs https

# Install python for some SQL stuff with the middlewaare
RUN pip3 install pandas
RUN pip3 install pyodbc

ARG FUNCTION_DIR=/code
RUN mkdir -p ${FUNCTION_DIR}
WORKDIR ${FUNCTION_DIR}
COPY . ${FUNCTION_DIR}

# Compile
RUN dotnet publish -c Release -o out

WORKDIR ${FUNCTION_DIR}/out
RUN mv ../entrypoint.sh .

RUN sed -i -e 's/\r$//' entrypoint.sh

CMD "/code/out/entrypoint.sh"
