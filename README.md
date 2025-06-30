# Planimation – A PDDL Plan Visualizer

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
![First Selection Interface](metterepath.png)

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

<!--
PARTE DI OPTIC/ BOOOOH
### 🧪 OPTIC (To be confirmed)
- If used with Docker:
  - Docker image setup instructions will be provided.
  - Example run command:

  DA COMPLETAREEEE
-->

---

## 👥 Credits

This project was developed by:

- **Arnieri Nicole** [@nicolearnieri](https://github.com/nicolearnieri) 
- **Martino Marco** [@Marco-Maker](https://github.com/Marco-Maker)   

As part of the final project for the *Automated Planning* module in the course **“Intelligent Systems and Automated Planning”** (A.Y. 2024–2025), part of the **Master's Degree in Artificial Intelligence and Computer Science** at **UNICAL – University of Calabria**.

Most problem definitions were provided as part of the course material.
