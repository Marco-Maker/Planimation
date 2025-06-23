## OPTIC server for PDDL planning
# This server provides an API to run the OPTIC planner on PDDL domain and problem files.
# It uses Flask to create a simple HTTP server that listens for POST requests with PDDL data.

from flask import Flask, request, jsonify
import subprocess, tempfile, os

app = Flask(__name__)

@app.route('/plan', methods=['POST'])
def plan():
    data = request.get_json()
    domain  = data['domain_pddl']
    problem = data['problem_pddl']
    with tempfile.TemporaryDirectory() as td:
        dom = os.path.join(td, 'domain.pddl')
        prob = os.path.join(td, 'problem.pddl')
        open(dom,  'w').write(domain)
        open(prob, 'w').write(problem)
        proc = subprocess.run(
            ['/app/optic', dom, prob],
            capture_output=True, text=True
        )
    return jsonify({
        'stdout': proc.stdout,
        'stderr': proc.stderr,
        'returncode': proc.returncode
    })

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)


# To run the server, use the command:
# docker run -p 5000:5000 optic-server
# or
# docker run -p 5000:5000 -v /path/to/optic:/app optic-server
# To test the server, you can use curl or any HTTP client to send a POST request:
# curl -X POST http://localhost:5000/plan -H "Content-Type: application/json" -d '{"domain_pddl": "your_domain_pddl", "problem_pddl": "your_problem_pddl"}'


'''
curl -X POST http://localhost:5000/plan \
  -H "Content-Type: application/json" \
  -d '{"domain_pddl":"(define (domain d))","problem_pddl":"(define (problem p))"}'

'''