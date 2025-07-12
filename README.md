# Planimation – A PDDL Plan Visualizer
<div align="center">
  <img src="/Images/ProjectLogo.png" alt="ProjectLogo" width="400"/>
</div>



## 🧠 What the Project Is About

**Planimation** is a visualizer for plans expressed in **PDDL (Planning Domain Definition Language)**, developed using Unity.  
It supports a variety of planning problems, grouped by both the scenario and the PDDL version used for encoding (e.g., standard, PDDL+ or PDDL 2.1).

What makes this project unique is the **user-generated problem instances**: while domains are predefined and fixed, problems are dynamically created by the user at runtime.  
This allows for flexible testing, custom problem generation, and exploration of planning behaviors in different domains and versions.

---

## 🚀 How to Try the Project

In the repository (specifically in the folder `Build/PlanimationDemo/`), you will find a **fully working demo** you can run directly without needing Unity.

If you'd like to modify or extend the project, you can run it through Unity:

1. Download and install Unity Hub: https://unity.com/unity-hub  
2. Clone this repository:
   ```bash
   git clone https://github.com/Marco-Maker/Planimation.git
   ```
3. Open Unity Hub and click Add, then select the cloned project folder.
4. Open the project and wait for Unity to import all packages and elaborate the assets.

---

## 🧩 Problems Covered

The project currently supports **three planning domains**:

### 1. Robots
- **Goal**: Move objects (balls) from one location to another.
- A group of robots is responsible for transporting the balls to reach the final configuration.

### 2. Logistics
- **Goal**: Transport one or more packages to specific destinations.
- Involves loading and unloading packages across locations using trucks or similar agents.

### 3. Elevator
- **Goal**: Reach a designated floor.
- Simulates the movement of an elevator between floors, managing floor requests.

You can select the **domain** from the **first interface**, which is shown here : 

<div align="center">
  <img src="/Images/FirstInterface.png" alt="FirstSelectionInterface" width="900"/>
</div>


Once a domain is selected, you can choose the **problem version** using the **second interface**, including:

- `Normal`: the simplest version, based on standard STRIPS planning.
- `PDDL+`: includes continuous processes and events.
- `PDDL 2.1`: supports durative actions and time-based constraints.

---

## 👁️ Visualization

In the main simulation scene, the plan is animated step by step.

Features include:

- Interactive control: `Play`, `Back to Menu`, `Show plan`
- Highlighting of the current action of the plan being executed
- Object focus, to move the camera on a specific object relevant to the problem instance


Each domain has a tailored visualization style, with different scenarios and agents.

---


## 🖼️ Assets Attribution

Some of the visual assets used in this project are provided by **Kenney**.  
You can find his amazing open-source asset packs here:  
👉 [https://kenney.nl/assets](https://kenney.nl/assets)

All assets are used under the **CC0 license**,  specified on the site.

---
## 🧮 Planning Solvers Used

This project relies on external planners to generate PDDL plans at run-time, right after the user finishes the construction of the problem.

### ✔️ ENHSP (Enhanced Numeric Heuristic Search Planner)
- Website: https://github.com/aiplan4eu/enhsp
- Used for solving both classical and numeric planning problems.


### 🧪 OPTIC (Optimising Preferences and Time-Dependent Costs) via Docker
- Website: https://github.com/KavrakiLab/optic.git
- Used for solving problems in **PDDL 2.1** (durative actions and time-based constraints).

We provide a fully working Docker-based server for the OPTIC planner, accessible via a simple Flask API. This allows Planimation to request plans at runtime by sending domain/problem definitions directly.

> ⚠️ **Important Note**: Planimation already sends the PDDL domain and problem definitions to the planner server at runtime.  
> You do **not** need to generate the plan manually — the server will be contacted directly by the application after the user builds the problem.
> Most of the following notions are examples: you simply need to start the Docker server, with one of the methods described.

#### 🐳 Docker Setup
To build and run the Docker container:
1. Clone the repo and move to the folder containing the `Dockerfile` and `server.py`, the `optic-docker` folder.
2. Run the following command to build the image:
   ```bash
   docker build -t optic-server .
   ```
3. To start the container manually:
   ```bash
    docker run -p 5000:5000 optic-server
   ```

- Once running, the planner will be available at `http://localhost:5000/plan` .
- You can test the endpoint using `curl http://localhost:5000/ping` .
- Expected response:
    ```json
    { "status": "ok" }
    ```
#### 🔁 Automated Run with PowerShell
On Windows, you can use the provided script `runDocker.ps1` to automatize the processes of:
- Building the Docker image
- Removing any existing container with the same name (useful for multiple runs or different tryouts)
- Launching the container in background
- Retry `/ping` until the Flask server is ready

To run the script: 
    ```powershell
    .\runDocker.ps1
    ```
✅ Once the server is up, you can send a POST request to /plan with your PDDL content. 
  ```bash
    curl -X POST http://localhost:5000/plan ^
    -H "Content-Type: application/json" ^
    -d "{\"domain_pddl\": \"(define ...)\", \"problem_pddl\": \"(define ...)\"}"
  ```

#### 📁 Folder Contents
- `Dockerfile`: sets up Ubuntu, builds OPTIC, exposes a Flask server
- `server.py`: handles /ping and /plan routes
- `runDocker.ps1`: PowerShell automation script

#### 🔒 Other notes
- The planner executable is located in `/app/optic` inside the container
- All `.pddl` content is sent via HTTP request body — no need to mount local files
- Output includes `stdout`, `stderr`, and `returncode` for debugging

#### 📦 Sample JSON Payload
Example of how the requests should be sent: 
  ```json
  {
  "domain_pddl": "(define (domain ...))",
  "problem_pddl": "(define (problem ...))"
  }
  ```

And the response:
  ```json
    {
      "stdout": "0: (move r1 a b)\n1: ...",
      "stderr": "",
      "returncode": 0
    }
  ```



---

## 👥 Credits

This project was developed by:

- **Arnieri Nicole** [@nicolearnieri](https://github.com/nicolearnieri) 
- **Martino Marco** [@Marco-Maker](https://github.com/Marco-Maker)   

As part of the final project for the *Automated Planning* module in the course **“Intelligent Systems and Automated Planning”** (A.Y. 2024–2025), part of the **Master's Degree in Artificial Intelligence and Computer Science** at **UNICAL – University of Calabria**.

Part of the problems' definitions were provided as part of the course material.
