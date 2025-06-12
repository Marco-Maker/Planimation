using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorProblemGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> floorsPrefab;
    private int floorCounter = 0;
    [SerializeField] private List<GameObject> personPrefab;
    [SerializeField] private GameObject elevatorDoorPrefab;
    [SerializeField] private GameObject elevatorPrefab;

    [SerializeField] private float floorHeight = 5f;
    [SerializeField] private float columnWidth = 10f;
    [SerializeField] private float elevatorOffsetX = -3f;
    [SerializeField] private float personOffsetX = 2f;

    void Awake()
    {
        GenerateScene();
    }

    void GenerateScene()
    {
        var planInfo = PlanInfo.GetInstance();

        Dictionary<string, string> atPersonMap = new Dictionary<string, string>();
        Dictionary<string, string> atElevatorMap = new Dictionary<string, string>();
        Dictionary<string, string> targetMap = new Dictionary<string, string>();
        Dictionary<string, List<string>> belowMap = new Dictionary<string, List<string>>();
        Dictionary<string, string> aboveMap = new Dictionary<string, string>();
        Dictionary<string, string> inElevatorMap = new Dictionary<string, string>();

        // Predicati
        foreach (var p in planInfo.GetPredicates())
        {
            if (p.name == "at-person" && p.values.Count == 2)
                atPersonMap[p.values[0]] = p.values[1];
            else if (p.name == "at-elevator" && p.values.Count == 2)
                atElevatorMap[p.values[0]] = p.values[1];
            else if (p.name == "target" && p.values.Count == 2)
                targetMap[p.values[0]] = p.values[1];
            else if (p.name == "in" && p.values.Count == 2)
                inElevatorMap[p.values[1]] = p.values[0];
            else if (p.name == "above" && p.values.Count == 2)
            {
                string upper = p.values[0];
                string lower = p.values[1];

                if (!belowMap.ContainsKey(lower))
                    belowMap[lower] = new List<string>();
                belowMap[lower].Add(upper);

                aboveMap[upper] = lower;
            }
        }
        foreach (var p in planInfo.GetFunctions())
        {
            if (p.name == "floors" && p.values.Count == 1)
            {
                int totalFloors = Int32.Parse(p.values[0]);

                for (int i = 1; i <= totalFloors; i++)
                {
                    string current = i.ToString();
                    string lower = (i - 1).ToString();

                    if (!belowMap.ContainsKey(current))
                        belowMap[current] = new List<string>();
                    if (!aboveMap.ContainsKey(current))
                        aboveMap[current] = null;

                    if (i > 1)
                    {
                        belowMap[lower].Add(current);
                        aboveMap[current] = lower;
                    }
                }
            }
            else if (p.name == "at-person" && p.values.Count == 2)
            {
                atPersonMap[p.values[0]] = p.values[1];
            }
            else if (p.name == "at-elevator" && p.values.Count == 2)
            {
                atElevatorMap[p.values[0]] = p.values[1];
            }
        }

        // Trova i floor radice (quelli più in basso)
        HashSet<string> allFloors = new HashSet<string>();
        foreach (var kvp in belowMap)
        {
            allFloors.Add(kvp.Key);
            foreach (var up in kvp.Value)
                allFloors.Add(up);
        }

        HashSet<string> upperFloors = new HashSet<string>();
        foreach (var kvp in aboveMap)
        {
            if (kvp.Value != null)
                upperFloors.Add(kvp.Key);
        }
        List<string> rootFloors = new List<string>();
        foreach (var f in allFloors)
        {
            if (!upperFloors.Contains(f))
                rootFloors.Add(f);
        }

        Dictionary<string, GameObject> floorGameObjects = new Dictionary<string, GameObject>();
        Dictionary<string, int> floorColumn = new Dictionary<string, int>();
        Dictionary<string, int> floorLevel = new Dictionary<string, int>();

        int currentColumn = 0;

        // DFS per posizionare i floor su colonne distinte se necessario
        void PlaceFloors(string floor, int level, int column)
        {
            Vector3 pos = new Vector3(column * columnWidth, level * floorHeight, 0);
            GameObject floorGO = Instantiate(floorsPrefab[floorCounter%floorsPrefab.Count], pos, Quaternion.identity, transform);
            floorCounter++;
            floorGO.name = floor;
            floorGameObjects[floor] = floorGO;
            floorColumn[floor] = column;
            floorLevel[floor] = level;

            if (!belowMap.ContainsKey(floor)) return;

            List<string> aboveList = belowMap[floor];
            for (int i = 0; i < aboveList.Count; i++)
            {
                string nextFloor = aboveList[i];
                int nextColumn = (aboveList.Count > 1) ? currentColumn++ : column;
                PlaceFloors(nextFloor, level + 1, nextColumn);
            }
        }

        foreach (var root in rootFloors)
        {
            PlaceFloors(root, 0, currentColumn++);
        }

        // 🔹 Mappa per assegnare una colonna orizzontale a ciascun elevator
        Dictionary<string, int> elevatorColumnMap = new Dictionary<string, int>();
        int elevatorIndex = 0;
        foreach (var elevator in atElevatorMap.Keys)
        {
            elevatorColumnMap[elevator] = elevatorIndex++;
        }

        // 🔹 Posiziona le porte degli ascensori su tutti i piani
        foreach (var elevator in atElevatorMap.Keys)
        {
            int columnOffset = elevatorColumnMap[elevator];

            foreach (var floor in floorGameObjects.Keys)
            {
                GameObject floorGO = floorGameObjects[floor];

                float xOffset = elevatorOffsetX + columnOffset * 1f; // Spaziatura orizzontale tra colonne
                Vector3 doorPos = floorGO.transform.position + new Vector3(xOffset, -0.6f, 0.5f);

                GameObject doorGO = Instantiate(elevatorDoorPrefab, doorPos, Quaternion.identity, floorGO.transform);
                doorGO.name = $"door_{elevator}_on_{floor}";
            }
        }

        // 🔹 Posiziona l'elevator solo sul piano in cui si trova
        foreach (var kvp in atElevatorMap)
        {
            string elevator = kvp.Key;
            string floor = kvp.Value;
            if (!floorGameObjects.ContainsKey(floor)) continue;

            GameObject floorGO = floorGameObjects[floor];
            int columnOffset = elevatorColumnMap[elevator];
            float xOffset = elevatorOffsetX + columnOffset * 1f;

            Vector3 pos = floorGO.transform.position + new Vector3(xOffset, -0.6f, 0.5f);

            GameObject elevatorGO = Instantiate(elevatorPrefab, pos, Quaternion.identity, transform);
            elevatorGO.name = elevator;
        }

        //Stampa il contenuto di inElevatorMap, le chiavi e stessa cosa per atPersonMap
        Debug.Log("In Elevator Map:");
        foreach (var kvp in inElevatorMap)
        {
            Debug.Log($"Person: {kvp.Key}, Elevator: {kvp.Value}");
        }
        Debug.Log("At Person Map:");
        foreach (var kvp in atPersonMap)
        {
            Debug.Log($"Person: {kvp.Key}, Floor: {kvp.Value}");
        }

        foreach (var person in inElevatorMap.Keys)
        {
            string personName = person;
            GameObject personGO = null;

            int randomIndex = UnityEngine.Random.Range(0, this.personPrefab.Count);
            GameObject personPrefab = this.personPrefab[randomIndex];

          
            Debug.Log($"Posizionamento persona {personName} dentro un ascensore");
            // La persona è dentro un ascensore
            string elevatorName = inElevatorMap[personName];

            // L'ascensore deve esistere
            GameObject elevatorGO = GameObject.Find(elevatorName);
            if (elevatorGO != null)
            {
                Debug.Log($"Posizionamento persona {personName} dentro ascensore {elevatorName}");
                Vector3 pos = elevatorGO.transform.position + new Vector3(0, 0.5f, 0);
                personGO = Instantiate(personPrefab, pos, Quaternion.identity, elevatorGO.transform);
                Debug.Log($"Posizionamento persona {personName} dentro ascensore {elevatorName} alla posizione {pos}");
            }
            else
            {
                Debug.LogWarning($"Ascensore {elevatorName} non trovato per la persona {personName}");
            }

            if (personGO != null)
            {
                personGO.name = personName;
                if (targetMap.ContainsKey(personName))
                    personGO.name += $"_target:{targetMap[personName]}";
            }
        }


        // 🔹 Posiziona le persone sul loro piano
        // 🔹 Posiziona le persone
        foreach (var person in atPersonMap.Keys)
        {
            string personName = person;
            GameObject personGO = null;

            int randomIndex = UnityEngine.Random.Range(0, this.personPrefab.Count);
            GameObject personPrefab = this.personPrefab[randomIndex];

            // La persona è su un piano
            string floorName = atPersonMap[personName];
            if (!floorGameObjects.ContainsKey(floorName)) continue;

            GameObject floorGO = floorGameObjects[floorName];
            Vector3 pos = floorGO.transform.position + new Vector3(personOffsetX, 0, 0);

            personGO = Instantiate(personPrefab, pos, Quaternion.identity, floorGO.transform);
            Debug.Log($"Posizionamento persona {personName} su piano {floorName} alla posizione {pos}");
            

            if (personGO != null)
            {
                personGO.name = personName;
                if (targetMap.ContainsKey(personName))
                    personGO.name += $"_target:{targetMap[personName]}";
            }
        }


        Debug.Log("Finito di generare la mappa");
    }
}
