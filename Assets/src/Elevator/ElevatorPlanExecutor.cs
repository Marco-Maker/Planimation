using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ElevatorPlanExecutor : MonoBehaviour
{
    public float actionDelay = 1.0f;
    private Dictionary<string, GameObject> people;
    private Dictionary<string, GameObject> floors;
    private Dictionary<string, GameObject> elevators;
    private Dictionary<string, Vector3> initialPositions = new Dictionary<string, Vector3>();


    [TextArea(10, 30)]
    public string planText;
    private string planFilePath;

    void Start()
    {
        planFilePath = "." + Const.PDDL_FOLDER + Const.OUTPUT_PLAN;
        LoadPlanFromFile();
        FindSceneObjects();
        StartCoroutine(ExecutePlan());
    }

    void LoadPlanFromFile()
    {
        if (!File.Exists(planFilePath))
        {
            Debug.LogError($"Plan file not found at path: {planFilePath}");
            return;
        }

        string[] allLines = File.ReadAllLines(planFilePath);
        List<string> planLines = new List<string>();

        foreach (string line in allLines)
        {
            string trimmed = line.Trim();
            if (trimmed.Contains(": (") && trimmed.EndsWith(")"))
            {
                planLines.Add(trimmed);
            }
        }

        planText = string.Join("\n", planLines);
        //Debug.Log($"Plan loaded from {planFilePath} with {planLines.Count} actions.");
    }

    void FindSceneObjects()
    {
        people = GameObject.FindGameObjectsWithTag("Person").ToDictionary(p => p.name.ToLower());
        floors = GameObject.FindGameObjectsWithTag("Floor").ToDictionary(f => f.name.ToLower());
        elevators = GameObject.FindGameObjectsWithTag("Elevator").ToDictionary(e => e.name.ToLower());
        //Debug.Log("_________INIT_______________");
        foreach (var person in people)
        {
            //Debug.Log($"Found person: {person.Key}");
            initialPositions[person.Key] = person.Value.transform.position;
        }
        /*
        foreach (var floor in floors)
        {
            Debug.Log($"Found floor: {floor.Key}");
        }
        foreach (var elevator in elevators)
        {
            Debug.Log($"Found elevator: {elevator.Key}");
        }
        Debug.Log("_________END_______________");*/
    }

    IEnumerator ExecutePlan()
    {
        var lines = planText.Split('\n');
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || !line.Contains(": (")) continue;

            string action = line.Substring(line.IndexOf(": (") + 3).TrimEnd(')');
            yield return ExecuteAction(action.ToLower());
            yield return new WaitForSeconds(actionDelay);
        }
    }

    IEnumerator ExecuteAction(string action)
    {
        string[] parts = action.Split(' ');

        switch (parts[0])
        {
            case "move-up":
            case "move-down":
                //Debug.Log(action.ToString());
                yield return MoveElevator(parts[1], parts[2], parts[3]);
                break;

            case "load":
                //Debug.Log(action.ToString());
                yield return LoadPerson(parts[1], parts[2], parts[3]);
                break;

            case "unload":
                //Debug.Log(action.ToString());
                yield return UnloadPerson(parts[1], parts[2], parts[3]);
                break;

            case "reached":
                //Debug.Log($"{parts[1]} has reached the goal at {parts[2]}.");
                break;

            default:
                Debug.LogWarning($"Unknown action: {action}");
                break;
        }
    }

    IEnumerator MoveElevator(string elevatorName, string from, string to)
    {
        bool found = false;
        GameObject elevator = null;
        foreach (var elev in elevators)
        {
            if (elev.Key.Contains(elevatorName) && elev.Key.Contains(from))
            {
                found = true;
                elevator = elev.Value;
            }
        }
        if (!found)
        {
            Debug.LogWarning($"Missing elevator: {elevatorName}.");
            yield break;
        }
        if (!floors.ContainsKey(to))
        {
            Debug.LogWarning($"Missing floor: {to}");
            yield break;
        }

        Vector3 targetPosition = new Vector3(
            elevator.transform.position.x,
            floors[to].transform.position.y,
            elevator.transform.position.z
        );

        Debug.Log($"Moving {elevatorName} from {from} to {to}");
        yield return MoveToPosition(elevator, targetPosition);
    }

    IEnumerator LoadPerson(string personName, string elevatorName, string floorName)
    {
        bool foundP = false;
        GameObject person = null;
        foreach (var p in people)
        {
            if (p.Key.Contains(personName))
            {
                foundP = true;
                person = p.Value;
            }
        }
        if (!foundP)
        {
            Debug.LogWarning($"Missing person: {personName}.");
            yield break;
        }

        bool foundE = false;
        GameObject elevator = null;
        foreach (var elev in elevators)
        {
            if (elev.Key.Contains(elevatorName) && elev.Key.Contains(floorName))
            {
                foundE = true;
                elevator = elev.Value;
            }
        }
        if (!foundE)
        {
            Debug.LogWarning($"Missing elevator: {elevatorName}.");
            yield break;
        }

        person.GetComponentInChildren<PersonMovement>().SetMoving(true);

        // Cammina verso l'elevatore prima di salire
        Vector3 elevatorEntry = elevator.transform.position;
        elevatorEntry.y = person.transform.position.y; // Mantieni altezza della persona

        Debug.Log($"{personName} is walking to the elevator {elevatorName}");
        yield return MoveToPosition(person, elevatorEntry);

        // Poi sale sull'elevatore
        person.transform.SetParent(elevator.transform);
        person.transform.localPosition = Vector3.zero;

        Debug.Log($"{personName} loaded into {elevatorName}");

        person.GetComponentInChildren<PersonMovement>().SetMoving(false);
    }

    IEnumerator UnloadPerson(string personName, string elevatorName, string floorName)
    {
        bool foundP = false;
        GameObject person = null;
        foreach (var p in people)
        {
            if (p.Key.Contains(personName))
            {
                foundP = true;
                person = p.Value;
            }
        }
        if (!foundP)
        {
            Debug.LogWarning($"Missing person: {personName}.");
            yield break;
        }

        if (!floors.TryGetValue(floorName, out var floor))
        {
            Debug.LogWarning($"Missing floor: {floorName}");
            yield break;
        }

        person.transform.SetParent(null);
        person.GetComponentInChildren<PersonMovement>().SetMoving(true);

        // Porta la persona all'altezza del piano
        Vector3 floorPos = floor.transform.position;
        person.transform.position = new Vector3(
            person.transform.position.x,
            floorPos.y,
            person.transform.position.z
        );

        Debug.Log($"{personName} unloaded from {elevatorName} at {floorName}");

        // Cammina verso la posizione iniziale (stessa X e Z di partenza)
        string personKey = people.Keys.FirstOrDefault(k => k.Contains(personName));
        if (personKey != null && initialPositions.TryGetValue(personKey, out var originalPos))
        {
            Vector3 target = new Vector3(originalPos.x, floorPos.y, originalPos.z);
            Debug.Log($"{personName} is walking to original position on {floorName}");
            yield return MoveToPosition(person, target);
        }
        person.GetComponentInChildren<PersonMovement>().SetMoving(false);

        Debug.Log($"{personName} reached original position on {floorName}");
    }


    IEnumerator MoveToPosition(GameObject obj, Vector3 target, float speed = 2f)
    {
        while (Vector3.Distance(obj.transform.position, target) > 0.01f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }
}
