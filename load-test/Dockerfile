FROM httpd:2.4

RUN apt-get update && \
    apt-get install -y siege dos2unix && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /tests

CMD ["sleep", "infinity"]