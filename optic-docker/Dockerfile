# 1) Immagine base
FROM ubuntu:20.04
ENV DEBIAN_FRONTEND=noninteractive

# 2) Installazione tool di build, Boost e Flask
RUN apt-get update && \
    apt-get install -y \
      build-essential \
      bison \
      flex \
      libboost-all-dev \
      libstdc++-10-dev \
      python3 \
      python3-pip && \
    pip3 install flask && \
    rm -rf /var/lib/apt/lists/*

# 3) Copia sorgenti OPTIC
WORKDIR /opt
COPY optic-clp/ ./optic-clp/

# 4) Compilazione OPTIC
WORKDIR /opt/optic-clp
RUN make CC=gcc CXX=g++ LD=g++ \
    && echo "Build OPTIC completata" \
    || true \
    && if [ ! -f optic ]; then \
         echo "Link manuale fallback…"; \
         OBJ_LIST=$(find . -name '*.o' -print); \
         g++ -o optic $OBJ_LIST obj/libOpticCommon.a -Wl,--start-group lib/*.a -Wl,--end-group; \
       fi

# 5) Installazione binario in /app
RUN mkdir /app && \
    cp /opt/optic-clp/optic /app/optic && \
    chmod +x /app/optic

# 6) Server Flask
WORKDIR /app
COPY server.py .

# 7) Espone la porta e avvia
EXPOSE 5000
CMD ["python3", "server.py"]
