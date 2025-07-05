# runDocker.ps1
# Script PowerShell per build, run detached del container Docker e ping con retry al server Flask

# 1. Parametri
$IMAGE_NAME     = "optic-server"
$CONTAINER_NAME = "optic_server"
$PORT           = 5000
$MAX_RETRIES    = 10
$RETRY_DELAY    = 1  # secondi tra i tentativi

# 2. Costruzione dell’immagine
Write-Host ""
Write-Host "=== Costruzione dell'immagine Docker ($IMAGE_NAME) ==="
docker build -t $IMAGE_NAME .
if ($LASTEXITCODE -ne 0) {
    Write-Error "Errore nella build dell'immagine Docker."
    exit 1
}

# 3. Rimozione di un eventuale container esistente
$existing = docker ps -aq -f "name=^$CONTAINER_NAME$"
if ($existing) {
    Write-Host "=== Rimozione del container esistente ($CONTAINER_NAME) ==="
    docker rm -f $CONTAINER_NAME | Out-Null
}

# 4. Avvio del container in modalità detached
Write-Host "=== Avvio del container '$CONTAINER_NAME' in background sulla porta $PORT ==="
docker run -d `
    --name $CONTAINER_NAME `
    -p "${PORT}:5000" `
    $IMAGE_NAME
if ($LASTEXITCODE -ne 0) {
    Write-Error "Errore nell'avvio del container."
    exit 1
}

# 5. Ping al server Flask con retry
Write-Host "=== Ping al server Flask (max ${MAX_RETRIES} tentativi) ==="
$retry = 0
$serverReady = $false

while ($retry -lt $MAX_RETRIES) {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:$PORT/ping" -TimeoutSec 5 -ErrorAction Stop
        if ($response -and $response.status -eq "ok") {
            Write-Host "✅ Server Flask è online su http://localhost:$PORT" -ForegroundColor Green
            $serverReady = $true
            break
        } else {
            Write-Warning "Risposta inattesa: $($response | ConvertTo-Json -Depth 3)"
        }
    } catch {
        Write-Host "Tentativo $($retry + 1)/${MAX_RETRIES}: server non ancora pronto..."
    }
    Start-Sleep -Seconds $RETRY_DELAY
    $retry++
}

if (-not $serverReady) {
    Write-Error "❌ Server non raggiungibile dopo ${MAX_RETRIES} tentativi."
    Write-Host "Controlla i log del container con: docker logs $CONTAINER_NAME"
    exit 1
}

Write-Host "✅ Tutto pronto! Il container risponde correttamente a /ping e /plan."
