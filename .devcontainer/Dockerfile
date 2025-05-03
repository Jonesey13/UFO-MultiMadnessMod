FROM debian:12.10

RUN apt-get update -y && apt-get install -y wget libicu-dev git unzip

# Install .Net
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y dotnet-sdk-9.0
ENV PATH=$PATH:/root/.dotnet

# Install dotnet script
RUN dotnet tool install -g dotnet-script --version 1.6.0
ENV PATH=$PATH:/root/.dotnet/tools

# Configure git for Dev Containers
RUN git config --global core.autocrlf true