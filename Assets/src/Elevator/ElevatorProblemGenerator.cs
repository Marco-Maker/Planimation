using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorProblemGenerator : MonoBehaviour
{
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject personPrefab;
    [SerializeField] private GameObject elevatorPrefab;

    [SerializeField] private float floorHeight = 5f;
    [SerializeField] private float elevatorOffsetX = -3f;
    [SerializeField] private float personOffsetX = 2f;

    void Start()
    {
        GenerateScene();
    }

    void GenerateScene()
    {
        var planInfo = PlanInfo.GetInstance();

        // Predicati e mappe di supporto
        Dictionary<string, string> atPersonMap = new Dictionary<string, string>();    // person → floor
        Dictionary<string, string> atElevatorMap = new Dictionary<string, string>();  // elevator → floor
        Dictionary<string, string> targetMap = new Dictionary<string, string>();      // person → target floor
        Dictionary<string, string> floorBelow = new Dictionary<string, string>();     // floor2 → floor1
        Dictionary<string, GameObject> floorGameObjects = new Dictionary<string, GameObject>();
        Dictionary<string, GameObject> elevatorGameObjects = new Dictionary<string, GameObject>();
        Dictionary<string, GameObject> personGameObjects = new Dictionary<string, GameObject>();

        // 🔹 1. Lettura dei predicati
        foreach (var p in planInfo.GetPredicates())
        {
            if (p.name == "at-person" && p.values.Count == 2)
                atPersonMap[p.values[0]] = p.values[1];
            else if (p.name == "at-elevator" && p.values.Count == 2)
                atElevatorMap[p.values[0]] = p.values[1];
            else if (p.name == "target" && p.values.Count == 2)
                targetMap[p.values[0]] = p.values[1];
            else if (p.name == "above" && p.values.Count == 2)
                floorBelow[p.values[0]] = p.values[1];  // floor2 is above floor1
        }

        // 🔹 2. Ordina i piani dal basso verso l’alto usando "above"
        List<string> orderedFloors = new List<string>();
        HashSet<string> allFloors = new HashSet<string>();

        foreach (var rel in floorBelow)
        {
            Debug.Log($"Relazione trovata: {rel.Key} è sopra {rel.Value}");
            allFloors.Add(rel.Key);
            allFloors.Add(rel.Value);
        }

        // Trova il piano più in basso (quello che non è mai sopra un altro)
        string bottomFloor = null;
        foreach (var floor in allFloors)
        {
            if (!floorBelow.ContainsKey(floor))
            {
                bottomFloor = floor;
                break;
            }
        }
        Debug.Log($"Piano più in basso trovato: {bottomFloor}");

        // Ricostruisce l'ordine dal basso verso l'alto
        while (bottomFloor != null)
        {
            Debug.Log($"Aggiungendo {bottomFloor} alla lista ordinata");
            orderedFloors.Add(bottomFloor);
            string nextFloor = null;
            foreach (var kvp in floorBelow)
            {
                if (kvp.Value == bottomFloor)
                {
                    nextFloor = kvp.Key;
                    break;
                }
            }
            bottomFloor = nextFloor;
        }

        // 🔹 3. Posiziona i floor
        for (int i = 0; i < orderedFloors.Count; i++)
        {
            
            string floorName = orderedFloors[i];
            Vector3 position = new Vector3(0, i * floorHeight, 0);
            GameObject floorGO = Instantiate(floorPrefab, position, Quaternion.identity, transform);
            floorGO.name = floorName;
            floorGameObjects[floorName] = floorGO;
        }

        // 🔹 4. Posiziona gli elevator
        foreach (var kvp in atElevatorMap)
        {
            string elevatorName = kvp.Key;
            string floorName = kvp.Value;

            if (!floorGameObjects.ContainsKey(floorName)) continue;

            GameObject floorGO = floorGameObjects[floorName];
            Vector3 position = floorGO.transform.position + new Vector3(elevatorOffsetX, 0, 0);
            GameObject elevatorGO = Instantiate(elevatorPrefab, position, Quaternion.identity, floorGO.transform);
            elevatorGO.name = elevatorName;
            elevatorGameObjects[elevatorName] = elevatorGO;
        }

        // 🔹 5. Posiziona le persone
        foreach (var kvp in atPersonMap)
        {
            Debug.Log($"Posizionando {kvp.Key} su {kvp.Value}");
            string personName = kvp.Key;
            string floorName = kvp.Value;

            if (!floorGameObjects.ContainsKey(floorName)) continue;

            GameObject floorGO = floorGameObjects[floorName];
            Vector3 position = floorGO.transform.position + new Vector3(personOffsetX, 0, 0);
            GameObject personGO = Instantiate(personPrefab, position, Quaternion.identity, floorGO.transform);
            personGO.name = personName;
            personGameObjects[personName] = personGO;

            // (Opzionale) Imposta il piano target come proprietà per uso futuro
            if (targetMap.ContainsKey(personName))
            {
                string targetFloor = targetMap[personName];
                personGO.name += $"_target:{targetFloor}";
            }
        }
    }
}
