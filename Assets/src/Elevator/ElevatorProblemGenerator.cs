using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorProblemGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> floorsPrefab;
    private int floorCounter = 0;
    [SerializeField] private List<GameObject> personPrefab;
    [SerializeField] private GameObject elevatorPrefab;

    [SerializeField] private float floorHeight = 5f;
    [SerializeField] private float columnWidth = 10f;
    [SerializeField] private float elevatorOffsetX = -3f;
    [SerializeField] private float personOffsetX = 2f;

    void Start()
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

        // Predicati
        foreach (var p in planInfo.GetPredicates())
        {
            if (p.name == "at-person" && p.values.Count == 2)
                atPersonMap[p.values[0]] = p.values[1];
            else if (p.name == "at-elevator" && p.values.Count == 2)
                atElevatorMap[p.values[0]] = p.values[1];
            else if (p.name == "target" && p.values.Count == 2)
                targetMap[p.values[0]] = p.values[1];
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

        // Trova i floor radice (quelli più in basso)
        HashSet<string> allFloors = new HashSet<string>();
        foreach (var kvp in belowMap)
        {
            allFloors.Add(kvp.Key);
            foreach (var up in kvp.Value)
                allFloors.Add(up);
        }

        HashSet<string> upperFloors = new HashSet<string>(aboveMap.Keys);
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

        // 🔹 Posiziona ogni elevator su tutti i piani, affiancati
        foreach (var elevator in atElevatorMap.Keys)
        {
            int columnOffset = elevatorColumnMap[elevator];

            foreach (var floor in floorGameObjects.Keys)
            {
                GameObject floorGO = floorGameObjects[floor];

                float xOffset = elevatorOffsetX + columnOffset * 1f; // Spaziatura orizzontale tra colonne
                Vector3 pos = floorGO.transform.position + new Vector3(xOffset, -0.6f, 0.5f);

                GameObject elevatorGO = Instantiate(elevatorPrefab, pos, Quaternion.identity, floorGO.transform);
                elevatorGO.name = $"{elevator}_on_{floor}";
            }
        }

        // 🔹 Posiziona le persone sul loro piano
        foreach (var kvp in atPersonMap)
        {
            string personName = kvp.Key;
            string floorName = kvp.Value;

            if (!floorGameObjects.ContainsKey(floorName)) continue;

            GameObject floorGO = floorGameObjects[floorName];
            Vector3 pos = floorGO.transform.position + new Vector3(personOffsetX, 0, 0);
            
            int randomIndex = Random.Range(0, this.personPrefab.Count);
            GameObject personPrefab = this.personPrefab[randomIndex];

            GameObject personGO = Instantiate(personPrefab, pos, Quaternion.identity, floorGO.transform);
            personGO.name = personName;

            if (targetMap.ContainsKey(personName))
                personGO.name += $"_target:{targetMap[personName]}";
        }
    }
}
