# 1) Immagine base
FROM ubuntu:20.04
ENV DEBIAN_FRONTEND=noninteractive

# 2) Installa tool di build, CMake, Ninja, COIN-OR, GSL, Flask e Git
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
    pip3 install flask && \
    rm -rf /var/lib/apt/lists/*

# 3) Clona il sorgente OPTIC
WORKDIR /opt
RUN git clone https://github.com/KavrakiLab/optic.git

# 4) Genera la build con CMake + Ninja
WORKDIR /opt/optic
RUN mkdir build && cd build && \
    cmake -G Ninja .. && \
    ninja && \
    echo "OPTIC compilato con Ninja" 

# 5) Copia l’eseguibile in /app
#    Il solver optic-clp si trova in build/src/optic/optic-clp
RUN mkdir /app && \
    cp /opt/optic/build/src/optic/optic-clp /app/optic && \
    chmod +x /app/optic

# 6) Aggiungi il server Flask
WORKDIR /app
COPY server.py .

# 7) Espone la porta e avvia Flask
EXPOSE 5000
CMD ["python3", "server.py"]
