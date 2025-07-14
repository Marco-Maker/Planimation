import json
from flask import Flask, request, jsonify
from flask_cors import CORS
import subprocess, tempfile, os, shutil

app = Flask(__name__)
CORS(app)

def extract_text_field(field):
    """
    Se field è già una str -> la restituisce.
    Se è dict con chiave 'value' -> ritorna field['value'].
    Se è lista di righe -> le unisce con newline.
    Altrimenti, dump JSON.
    """
    if isinstance(field, str):
        return field
    if isinstance(field, dict) and 'value' in field and isinstance(field['value'], str):
        return field['value']
    if isinstance(field, list):
        return "\n".join(str(x) for x in field)
    # fallback generico
    return json.dumps(field)

@app.route('/plan', methods=['POST'])
def plan():
    print("🟢 Richiesta ricevuta su /plan")
    data = request.get_json(force=True)
    print("📦 JSON ricevuto:", data)
    
    if not data:
        return jsonify(stdout="", stderr="Nessun JSON ricevuto", returncode=-1), 400
    # Estrai SEMPRE una stringa corretta, qualunque formato arrivi
    domain_raw  = data.get('domain_pddl', '')
    problem_raw = data.get('problem_pddl', '')

    domain  = extract_text_field(domain_raw)
    problem = extract_text_field(problem_raw)

    with tempfile.TemporaryDirectory() as td:
        dom  = os.path.join(td, 'domain.pddl')
        prob = os.path.join(td, 'problem.pddl')
        with open(dom,  'w') as f: f.write(domain)
        with open(prob, 'w') as f: f.write(problem)

        # Verifica eseguibile
        if not os.path.exists('/app/optic') or not os.access('/app/optic', os.X_OK):
            return jsonify(stdout="", stderr="optic non trovato o non eseguibile", returncode=-1), 500

        # Esegui planner
        cmd = ['/app/optic', dom, prob]
        try:
            proc = subprocess.run(cmd, capture_output=True, text=True )
        except Exception as e:
            print("🔥 Errore durante l'esecuzione di OPTIC:", str(e))
            return jsonify(stdout="", stderr=str(e), returncode=-1), 500

    return jsonify(stdout=proc.stdout, stderr=proc.stderr, returncode=proc.returncode)

@app.route('/ping', methods=['GET'])
def ping():
    return jsonify(status="ok")

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
