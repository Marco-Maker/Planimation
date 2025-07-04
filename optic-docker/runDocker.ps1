# script powerShell per automatizzare la costruzione e l'esecuzione di un container Docker + successivo ping per verificare che il serverino sia on and running

# 1. Nome immagine e container
$IMAGE_NAME = "optic-server"
$CONTAINER_NAME = "optic_server"
$PORT = 5000

Write-Host "`n🔧 Costruzione dell'immagine Docker..."
docker build -t $IMAGE_NAME . || exit 1

# 2. Controlla se esiste già un container attivo
$existing = docker ps -aq -f name="^$CONTAINER_NAME$"
if ($existing) {
    Write-Host "🧹 Rimozione del container esistente..."
    docker rm -f $CONTAINER_NAME | Out-Null
}

# 3. Avvia il container in background
Write-Host "🚀 Avvio del container '$CONTAINER_NAME' sulla porta $PORT..."
docker run -d --name $CONTAINER_NAME -p $PORT:5000 $IMAGE_NAME

# 4. Attendi che Flask sia pronto
Write-Host "⏳ Attendo che il server sia pronto..."
Start-Sleep -Seconds 2

# 5. Ping del server
$response = $null
try {
    $response = Invoke-RestMethod -Uri "http://localhost:$PORT/ping" -TimeoutSec 10
} catch {
    Write-Host "❌ Server non raggiungibile. Controlla i log con:"
    Write-Host "   docker logs $CONTAINER_NAME"
    exit 1
}

if ($response.status -eq "ok") {
    Write-Host "✅ Server Flask è online su http://localhost:$PORT"
} else {
    Write-Host "⚠️ Risposta inaspettata: $($response | ConvertTo-Json)"
}


## come eseguire:
# 