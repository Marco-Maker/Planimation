using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
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
        foreach (var floor in floors)
        {
            //Debug.Log($"Found floor: {floor.Key}");
        }
        foreach (var elevator in elevators)
        {
            Debug.Log($"Found elevator: {elevator.Key}");
        }
        Debug.Log("_________END_______________");
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
        GameObject.FindWithTag("ActionText").GetComponent<TextMeshProUGUI>().text = action;

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
        // Trova l’elevator mobile per nome (ignorando floor)
        if (!elevators.TryGetValue(elevatorName.ToLower(), out GameObject elevator))
        {
            Debug.LogWarning($"Elevator '{elevatorName}' not found.");
            yield break;
        }

        if (!floors.TryGetValue(to.ToLower(), out GameObject targetFloor))
        {
            Debug.LogWarning($"Target floor '{to}' not found.");
            yield break;
        }

        float cabinOffset = -0.5f; // Modifica in base a dove vuoi l'allineamento

        Vector3 targetPosition = new Vector3(
            elevator.transform.position.x,
            targetFloor.transform.position.y + cabinOffset,
            elevator.transform.position.z
        );

        Debug.Log($"[MoveElevator] Moving {elevatorName} from {from} to {to}");
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

        if (!elevators.TryGetValue(elevatorName.ToLower(), out GameObject elevator))
        {
            Debug.LogWarning($"Missing elevator: {elevatorName}.");
            yield break;
        }

        Transform inside = elevator.transform.Find("Inside");
        Transform outside = elevator.transform.Find("Outside");

        if (inside == null || outside == null)
        {
            Debug.LogWarning($"Elevator {elevatorName} missing 'Inside' or 'Outside' Transform.");
            yield break;
        }

        person.GetComponentInChildren<PersonMovement>()?.SetMoving(true);

        // Cammina verso l'ingresso (outside)
        Vector3 outsidePos = outside.position;
        outsidePos.y = person.transform.position.y;
        Debug.Log($"{personName} walking to {elevatorName} (outside)");
        yield return MoveToPosition(person, outsidePos);

        // Cammina dentro la cabina (inside)
        Vector3 insidePos = inside.position;
        insidePos.y = person.transform.position.y;
        Debug.Log($"{personName} walking inside {elevatorName}");
        yield return MoveToPosition(person, insidePos);

        // Imposta parent dopo che è già dentro
        person.transform.SetParent(elevator.transform);
        Vector3 insideLocal = elevator.transform.InverseTransformPoint(person.transform.position);
        insideLocal.y = 0.5f; // offset verticale
        person.transform.localPosition = insideLocal;

        Debug.Log($"{personName} loaded into {elevatorName}");

        person.GetComponentInChildren<PersonMovement>()?.SetMoving(false);
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

        if (!elevators.TryGetValue(elevatorName.ToLower(), out GameObject elevator))
        {
            Debug.LogWarning($"Missing elevator: {elevatorName}.");
            yield break;
        }

        if (!floors.TryGetValue(floorName.ToLower(), out GameObject floor))
        {
            Debug.LogWarning($"Missing floor: {floorName}");
            yield break;
        }

        Transform inside = elevator.transform.Find("Inside");
        Transform outside = elevator.transform.Find("Outside");

        if (inside == null || outside == null)
        {
            Debug.LogWarning($"Elevator {elevatorName} missing 'Inside' or 'Outside' Transform.");
            yield break;
        }

        person.GetComponentInChildren<PersonMovement>()?.SetMoving(true);

        // Cammina verso Outside prima di uscire
        Vector3 exitPos = outside.position;
        exitPos.y = person.transform.position.y;
        Debug.Log($"{personName} walking to outside of {elevatorName}");
        yield return MoveToPosition(person, exitPos);

        // Scende dall'elevator
        person.transform.SetParent(null);
        Vector3 newWorldPos = exitPos;
        newWorldPos.y = floor.transform.position.y;
        person.transform.position = newWorldPos;

        Debug.Log($"{personName} unloaded at {floorName}");

        // Cammina verso posizione iniziale
        string personKey = people.Keys.FirstOrDefault(k => k.Contains(personName.ToLower()));
        if (personKey != null && initialPositions.TryGetValue(personKey, out var originalPos))
        {
            Vector3 target = new Vector3(originalPos.x, newWorldPos.y, originalPos.z);
            Debug.Log($"{personName} walking to original position");
            yield return MoveToPosition(person, target);
        }

        person.GetComponentInChildren<PersonMovement>()?.SetMoving(false);
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
