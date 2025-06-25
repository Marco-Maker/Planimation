using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class ElevatorPlanExecutorEvent: MonoBehaviour
{
    public float actionSpeedMultiplier = 1.0f;
    private Dictionary<string, GameObject> people;
    private Dictionary<string, GameObject> floors;
    private Dictionary<string, GameObject> elevators;
    private Dictionary<string, Vector3> initialPositions = new Dictionary<string, Vector3>();

    private Dictionary<string, float> atElevator = new();  // Stato numerico
    private List<TimedAction> plan = new();

    [TextArea(10, 30)]
    public string planText;
    private string planFilePath;

    private class TimedAction
    {
        public float startTime;
        public string action;
    }

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

        var allLines = File.ReadAllLines(planFilePath);
        plan.Clear();

        foreach (string line in allLines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || !trimmed.Contains(": (")) continue;

            var parts = trimmed.Split(new[] { ": (" }, System.StringSplitOptions.None);
            if (parts.Length < 2) continue;

            if (float.TryParse(parts[0].Trim(), out float startTime))
            {
                string action = parts[1].Trim(')', ' ');
                plan.Add(new TimedAction { startTime = startTime, action = action.ToLower() });
            }
        }

        planText = string.Join("\n", plan.Select(p => $"{p.startTime}: ({p.action})"));
    }

    void FindSceneObjects()
    {
        people = GameObject.FindGameObjectsWithTag("Person").ToDictionary(p => p.name.ToLower());
        floors = GameObject.FindGameObjectsWithTag("Floor").ToDictionary(f => f.name.ToLower());
        elevators = GameObject.FindGameObjectsWithTag("Elevator").ToDictionary(e => e.name.ToLower());

        foreach (var person in people)
            initialPositions[person.Key] = person.Value.transform.position;

        foreach (var elevator in elevators.Keys)
            atElevator[elevator] = 0f;  // Inizializza piano corrente
    }

    IEnumerator ExecutePlan()
    {
        float currentTime = 0f;
        foreach (var ta in plan.OrderBy(p => p.startTime))
        {
            float waitTime = (ta.startTime - currentTime) * actionSpeedMultiplier;
            if (waitTime > 0)
                yield return new WaitForSeconds(waitTime);
            currentTime = ta.startTime;

            yield return ExecuteAction(ta.action);
        }
    }

    IEnumerator ExecuteAction(string action)
    {
        string[] parts = action.Split(' ');

        switch (parts[0])
        {
            case "move-up":
                yield return MoveElevator(parts[1], 1);
                break;

            case "move-down":
                yield return MoveElevator(parts[1], -1);
                break;

            case "load":
                yield return LoadPerson(parts[1], parts[2]);
                break;

            case "unload":
                yield return UnloadPerson(parts[1], parts[2]);
                break;

            case "reached":
                Debug.Log($"{parts[1]} has reached their target.");
                break;

            default:
                Debug.LogWarning($"Unknown action: {action}");
                break;
        }
    }

    IEnumerator MoveElevator(string elevatorName, int direction)
    {
        if (!elevators.TryGetValue(elevatorName.ToLower(), out GameObject elevator))
        {
            Debug.LogWarning($"Elevator '{elevatorName}' not found.");
            yield break;
        }

        atElevator[elevatorName] += direction;

        // Trova il piano "visivo" più vicino a quello numerico
        var targetFloor = floors
            .OrderBy(f => Mathf.Abs(float.Parse(f.Key) - atElevator[elevatorName]))
            .First().Value;

        float cabinOffset = -0.5f;

        Vector3 targetPosition = new Vector3(
            elevator.transform.position.x,
            targetFloor.transform.position.y + cabinOffset,
            elevator.transform.position.z
        );

        Debug.Log($"[MoveElevator] {elevatorName} moving {(direction > 0 ? "up" : "down")} to floor {atElevator[elevatorName]}");
        yield return MoveToPosition(elevator, targetPosition);
    }

    IEnumerator LoadPerson(string personName, string elevatorName)
    {
        if (!TryGetPerson(personName, out GameObject person)) yield break;
        if (!elevators.TryGetValue(elevatorName.ToLower(), out GameObject elevator)) yield break;

        Transform inside = elevator.transform.Find("Inside");
        Transform outside = elevator.transform.Find("Outside");

        if (inside == null || outside == null)
        {
            Debug.LogWarning($"Missing 'Inside' or 'Outside' in elevator {elevatorName}");
            yield break;
        }

        person.GetComponentInChildren<PersonMovement>()?.SetMoving(true);

        Vector3 outsidePos = outside.position;
        outsidePos.y = person.transform.position.y;
        yield return MoveToPosition(person, outsidePos);

        Vector3 insidePos = inside.position;
        insidePos.y = person.transform.position.y;
        yield return MoveToPosition(person, insidePos);

        person.transform.SetParent(elevator.transform);
        Vector3 insideLocal = elevator.transform.InverseTransformPoint(person.transform.position);
        insideLocal.y = 0.5f;
        person.transform.localPosition = insideLocal;

        Debug.Log($"{personName} loaded into {elevatorName}");
        person.GetComponentInChildren<PersonMovement>()?.SetMoving(false);
    }

    IEnumerator UnloadPerson(string personName, string elevatorName)
    {
        if (!TryGetPerson(personName, out GameObject person)) yield break;
        if (!elevators.TryGetValue(elevatorName.ToLower(), out GameObject elevator)) yield break;

        Transform outside = elevator.transform.Find("Outside");

        person.GetComponentInChildren<PersonMovement>()?.SetMoving(true);

        Vector3 exitPos = outside.position;
        exitPos.y = person.transform.position.y;
        yield return MoveToPosition(person, exitPos);

        person.transform.SetParent(null);
        person.transform.position = new Vector3(exitPos.x, exitPos.y, exitPos.z);

        Debug.Log($"{personName} unloaded from {elevatorName}");

        string personKey = people.Keys.FirstOrDefault(k => k.Contains(personName.ToLower()));
        if (personKey != null && initialPositions.TryGetValue(personKey, out var originalPos))
        {
            Vector3 target = new Vector3(originalPos.x, person.transform.position.y, originalPos.z);
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

    bool TryGetPerson(string name, out GameObject person)
    {
        person = null;
        foreach (var p in people)
        {
            if (p.Key.Contains(name.ToLower()))
            {
                person = p.Value;
                return true;
            }
        }
        Debug.LogWarning($"Missing person: {name}");
        return false;
    }
}

