from flask import Flask, request, jsonify
from flask_cors import CORS
import subprocess, tempfile, os, shutil

app = Flask(__name__)
CORS(app)

@app.route('/plan', methods=['POST'])
def plan():
    data = request.get_json(force=True)
    print(f"[DEBUG] Received JSON: {data}")

    domain = data.get('domain_pddl', '')
    problem = data.get('problem_pddl', '')

    with tempfile.TemporaryDirectory() as td:
        dom = os.path.join(td, 'domain.pddl')
        prob = os.path.join(td, 'problem.pddl')
        with open(dom,  'w') as f: f.write(domain)
        with open(prob, 'w') as f: f.write(problem)

        # 1) Verifica che /app/optic esista e sia eseguibile
        exists = os.path.exists('/app/optic')
        can_exec = os.access('/app/optic', os.X_OK)
        print(f"[DEBUG] /app/optic exists? {exists}, executable? {can_exec}")
        print(f"[DEBUG] /app contents: {os.listdir('/app')}")

        cmd = ['/app/optic', dom, prob]
        print(f"[DEBUG] Running command: {cmd}")

        try:
            proc = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=60
            )
        except Exception as e:
            print(f"[ERROR] Exception running planner: {e}")
            return jsonify(stdout="", stderr=str(e), returncode=-1), 500

        print(f"[DEBUG] returncode: {proc.returncode}")
        print(f"[DEBUG] Planner stdout:\n{proc.stdout}")
        print(f"[DEBUG] Planner stderr:\n{proc.stderr}")

    return jsonify(
        stdout=proc.stdout,
        stderr=proc.stderr,
        returncode=proc.returncode
    )

@app.route('/ping', methods=['GET'])
def ping():
    return jsonify(status="ok")

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
