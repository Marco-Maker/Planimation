# 1) Base Linux con tool di build
FROM ubuntu:20.04

ENV DEBIAN_FRONTEND=noninteractive

# 2) Installa tutti i compilatori e le librerie necessarie
RUN apt-get update && \
    apt-get install -y \
      build-essential \
      bison \
      flex \
      libboost-all-dev \
      python3 \
      python3-pip && \
    rm -rf /var/lib/apt/lists/*

# 3) Copia l'intera directory del progetto nel container
WORKDIR /opt
COPY . .

# 4) Compila OPTIC
WORKDIR /opt/optic-clp
RUN make

# 5) Individua il binario prodotto e copialo in /app con nome "optic"
#    Provo due nomi comuni; adatta se il tuo target è diverso
RUN mkdir /app && \
    if [ -f ./optic ]; then cp ./optic /app/optic; \
    elif [ -f ./solver-clp ]; then cp ./solver-clp /app/optic; \
    elif [ -f ./opticMain ]; then cp ./opticMain /app/optic; \
    else echo "Errore: binario non trovato" && exit 1; fi

# 6) Copia i file PDDL di test in /app
WORKDIR /app
COPY domain.pddl problem.pddl /app/

# 7) Rendi eseguibile il planner
RUN chmod +x optic

# 8) (Opzionale) Espone la porta 5000 per un server REST futuro
EXPOSE 5000

# 9) Comando di default: esegue il planner sui file di test
CMD ["./optic", "domain.pddl", "problem.pddl"]
