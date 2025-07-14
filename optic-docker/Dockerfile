# 1) Immagine base
FROM ubuntu:20.04
ENV DEBIAN_FRONTEND=noninteractive

# 2) Installa tool di build, CMake, Ninja, COIN-OR, GSL, Flask, Flask-CORS e Git
RUN apt-get update && \
    apt-get install -y \
      build-essential \
      cmake \
      ninja-build \
      flex \
      bison \
      libboost-all-dev \
      libstdc++-10-dev \
      coinor-libclp-dev \
      coinor-libcbc-dev \
      coinor-libcoinutils-dev \
      coinor-libosi-dev \
      libgsl-dev \
      python3 \
      python3-pip \
      git && \
    pip3 install flask flask-cors && \
    rm -rf /var/lib/apt/lists/*

# 3) Clona OPTIC e build
WORKDIR /opt
RUN git clone https://github.com/KavrakiLab/optic.git
WORKDIR /opt/optic
RUN mkdir build && cd build && \
    cmake -G Ninja .. && \
    ninja && \
    echo "✅ OPTIC compilato con Ninja"

# 4) Copia il binario corretto in /app
RUN mkdir /app && \
    cp /opt/optic/build/src/optic/optic /app/optic && \
    chmod +x /app/optic

# 5) Aggiungi server Flask
WORKDIR /app
COPY server.py .

EXPOSE 5000
CMD ["python3", "-u", "server.py"]


## come eseguire: 
# 1) docker build -t optic-docker .
# così si crea l'immagine Docker, poi si può eseguire il container:
# 2) docker run -p 5000:5000 optic-docker
# Per testare il server Flask (server.py), puoi usare curl o Postman per inviare richieste POST a http://localhost:5000/ping