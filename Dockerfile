FROM ubuntu:jammy

# Install the apt-get packages
RUN apt-get update
RUN apt-get install -y libicu-dev libleveldb-dev screen

COPY ./out /opt/neo-cli
RUN ln -s /opt/neo-cli/neo-cli /usr/bin
