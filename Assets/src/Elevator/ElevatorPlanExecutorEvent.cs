using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ElevatorPlanExecutorEvent : MonoBehaviour
{
    public float actionSpeedMultiplier = 1.0f;
    private Dictionary<string, GameObject> people;
    private Dictionary<string, GameObject> floors;
    private Dictionary<string, GameObject> elevators;
    private Dictionary<string, Vector3> initialPositions = new();

    private Dictionary<string, float> atElevator = new();
    private Dictionary<string, float> elevatorLoad = new();
    private Dictionary<string, float> elevatorMaxLoad = new();
    private Dictionary<string, int> elevatorCapacity = new();
    private Dictionary<string, int> elevatorPassengers = new();
    private Dictionary<string, float> elevatorDistance = new();
    private Dictionary<string, float> personWeight = new();
    private Dictionary<string, float> personTarget = new();
    private Dictionary<string, bool> personReached = new();

    private List<TimedAction> plan = new();
    [TextArea(10, 30)] public string planText;
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

        float lastTime = 0f;

        foreach (string line in allLines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed.Contains("-----waiting----"))
            {
                // Estrai il tempo target tra le parentesi quadre
                int startBracket = trimmed.IndexOf('[');
                int endBracket = trimmed.IndexOf(']');

                Debug.Log($"Processing waiting line: {startBracket} {endBracket}");

                if (startBracket != -1 && endBracket != -1)
                {
                    string timeStr = trimmed.Substring(startBracket + 1, endBracket - startBracket - 1);
                    Debug.Log($"Extracted time string: '{timeStr}' from line: {trimmed}");
                    float targetTime = float.Parse(timeStr.Trim().Split(".")[0]);
                    Debug.Log($"Parsed target time: {targetTime}");
                    float waitDuration = targetTime - lastTime;
                    if (waitDuration > 0)
                    {
                        plan.Add(new TimedAction { startTime = lastTime, action = $"wait {waitDuration}" });
                    }
                    // Aggiorna lastTime al tempo target
                    lastTime = targetTime;
                }
            }
            else if (trimmed.Contains(": ("))
            {
                var parts = trimmed.Split(new[] { ": (" }, System.StringSplitOptions.None);
                if (parts.Length < 2) continue;

                Debug.Log($"Processing action line: {parts[0]} {parts[1]}");

                float startTime = float.Parse(parts[0].Trim().Split(".")[0]);

                Debug.Log($"Parsed start time: {startTime}");

                string action = parts[1].Trim(')', ' ');
                plan.Add(new TimedAction { startTime = startTime, action = action.ToLower() });

                // Aggiorna il tempo dell'ultima azione valida
                lastTime = startTime;
                
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

        foreach (var f in PlanInfo.GetInstance().GetFunctions())
        {
            switch (f.name)
            {
                case "at-elevator":
                    atElevator[f.values[0]] = float.Parse(f.values[1]);
                    break;
                case "weight":
                    personWeight[f.values[0]] = float.Parse(f.values[1]);
                    break;
                case "capacity":
                    elevatorCapacity[f.values[0]] = int.Parse(f.values[1]);
                    break;
                case "distance-run":
                    elevatorDistance[f.values[0]] = float.Parse(f.values[1]);
                    break;
                case "max-load":
                    elevatorMaxLoad[f.values[0]] = float.Parse(f.values[1]);
                    break;
                case "passengers":
                    elevatorPassengers[f.values[0]] = int.Parse(f.values[1]);
                    break;
                case "load":
                    elevatorLoad[f.values[0]] = float.Parse(f.values[1]);
                    break;
                case "target":
                    personTarget[f.values[0]] = float.Parse(f.values[1]);
                    break;
            }
        }
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
            case "startmovingup":
                yield return MoveElevator(parts[1], 1);
                break;
            case "startmovingdown":
                yield return MoveElevator(parts[1], -1);
                break;
            case "load":
                yield return LoadPerson(parts[1], parts[2]);
                break;
            case "unload":
                yield return UnloadPerson(parts[1], parts[2]);
                break;
            case "reached":
                personReached[parts[1]] = true;
                Debug.Log($"{parts[1]} has reached their target floor.");
                break;
            default:
                Debug.Log($"Unknown action: {action}");
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

        string key = elevatorName;
        if (!elevatorDistance.ContainsKey(key)) elevatorDistance[key] = 0;

        float speed = 10f;
        float maxLoad = elevatorMaxLoad.ContainsKey(key) ? elevatorMaxLoad[key] : 1;
        float load = elevatorLoad.ContainsKey(key) ? elevatorLoad[key] : 0;

        while (elevatorDistance[key] < 100)
        {
            float adjustedSpeed = Math.Abs(speed * (1 - (3 * (load / maxLoad))));
            elevatorDistance[key] += adjustedSpeed * Time.deltaTime;
            yield return null;
        }

        elevatorDistance[key] = 0;
        atElevator[key] += direction;

        var targetFloor = floors
            .OrderBy(f => Mathf.Abs(float.Parse(f.Key) - atElevator[key]))
            .First().Value;

        Vector3 targetPosition = new Vector3(
            elevator.transform.position.x,
            targetFloor.transform.position.y - 0.5f,
            elevator.transform.position.z
        );

        yield return MoveToPosition(elevator, targetPosition);
    }

    IEnumerator LoadPerson(string personName, string elevatorName)
    {
        if (!TryGetPerson(personName, out GameObject person)) yield break;
        if (!elevators.TryGetValue(elevatorName.ToLower(), out GameObject elevator)) yield break;

        if (elevatorPassengers[elevatorName] + 1 > elevatorCapacity[elevatorName] ||
            elevatorLoad[elevatorName] + personWeight[personName] > elevatorMaxLoad[elevatorName])
        {
            Debug.LogWarning($"{personName} cannot enter {elevatorName} due to limits.");
            yield break;
        }

        Transform inside = elevator.transform.Find("Inside");
        Transform outside = elevator.transform.Find("Outside");

        if (inside == null || outside == null) yield break;

        person.GetComponentInChildren<PersonMovement>()?.SetMoving(true);

        Vector3 outsidePos = outside.position;
        outsidePos.y = person.transform.position.y;
        yield return MoveToPosition(person, outsidePos);

        Vector3 insidePos = inside.position;
        insidePos.y = outsidePos.y;
        yield return MoveToPosition(person, insidePos);

        person.transform.SetParent(elevator.transform);
        Vector3 insideLocal = elevator.transform.InverseTransformPoint(person.transform.position);
        insideLocal.y = 0.5f;
        person.transform.localPosition = insideLocal;

        elevatorPassengers[elevatorName]++;
        elevatorLoad[elevatorName] += personWeight[personName];

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

        elevatorPassengers[elevatorName]--;
        elevatorLoad[elevatorName] -= personWeight[personName];

        Debug.Log($"{personName} unloaded from {elevatorName}");

        string personKey = people.Keys.FirstOrDefault(k => k.Contains(personName.ToLower()));
        if (personKey != null && initialPositions.TryGetValue(personKey, out var originalPos))
        {
            Vector3 target = new Vector3(originalPos.x, person.transform.position.y, originalPos.z);
            yield return MoveToPosition(person, target);
        }

        Debug.Log("Arrived");

        person.GetComponentInChildren<PersonMovement>()?.SetMoving(false);

        Debug.Log("Moving to next");
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
