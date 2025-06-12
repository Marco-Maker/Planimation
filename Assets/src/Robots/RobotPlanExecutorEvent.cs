using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RobotPlanExecutorEvent : MonoBehaviour
{
    public float actionDelay = 0.5f;
    public string planText;
    public float speed = 1f;
    public float battery = 100f;

    private Dictionary<string, GameObject> gardens;
    private Dictionary<string, GameObject> robots;
    private Dictionary<string, GameObject> objects;

    private string planFilePath;
    private float robotHeight;
    private bool isMoving = false;
    private bool isCharging = false;
    private float movedDistance = 0f;
    private Vector3 moveTarget;
    private string currentRobot;
    private string currentFrom, currentTo;

    void Start()
    {
        planFilePath = "." + Const.PDDL_FOLDER + Const.OUTPUT_PLAN;
        LoadPlanFromFile();
        FindSceneObjects();
        robotHeight = GameObject.FindGameObjectWithTag("Robot").transform.position.y;
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
        var planLines = allLines.Where(line => line.Trim().Contains(": (") && line.Trim().EndsWith(")")).ToList();
        planText = string.Join("\n", planLines);
    }

    void FindSceneObjects()
    {
        gardens = GameObject.FindGameObjectsWithTag("Garden").ToDictionary(r => r.name.ToLower());
        robots = GameObject.FindGameObjectsWithTag("Robot").ToDictionary(r => r.name.ToLower());
        objects = GameObject.FindGameObjectsWithTag("Ball").ToDictionary(o => o.name.ToLower());
    }

    IEnumerator ExecutePlan()
    {
        foreach (var rawLine in planText.Split('\n'))
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
            case "pick":
                yield return PickObject(parts[1], parts[2], parts[3]);
                break;

            case "drop":
                yield return DropObject(parts[1], parts[2], parts[3]);
                break;

            case "startmove":
                yield return StartMove(parts[1], parts[2], parts[3]);
                break;

            case "reprisemovement":
                yield return RepriseMovement(parts[1], parts[2], parts[3]);
                break;

            case "startcharge":
                StartCharging(parts[1]);
                break;

            case "stopcharge":
                StopCharging(parts[1]);
                break;

            default:
                Debug.LogWarning($"Unknown action: {action}");
                break;
        }
    }

    IEnumerator StartMove(string robotName, string from, string to)
    {
        if (!robots.TryGetValue(robotName, out var robot) || !gardens.TryGetValue(to, out var target))
        {
            Debug.LogWarning($"Missing robot or garden: {robotName}, {to}");
            yield break;
        }

        Debug.Log($"{robotName} starts moving from {from} to {to}");

        isMoving = true;
        currentRobot = robotName;
        currentFrom = from;
        currentTo = to;
        moveTarget = new Vector3(target.transform.position.x, robotHeight, target.transform.position.z);
        movedDistance = 0f;

        while (isMoving)
        {
            float step = speed * Time.deltaTime;
            robot.transform.position = Vector3.MoveTowards(robot.transform.position, moveTarget, step);
            movedDistance += step;
            battery -= 2f * Time.deltaTime;

            // Trigger event: arrived
            if (Vector3.Distance(robot.transform.position, moveTarget) < 0.01f)
            {
                Debug.Log($"{robotName} arrived at {to}");
                isMoving = false;
            }

            // Trigger event: batteryDead
            if (battery <= 10f)
            {
                Debug.Log($"{robotName} battery dead.");
                isMoving = false;
            }

            yield return null;
        }
    }

    IEnumerator RepriseMovement(string robotName, string from, string to)
    {
        Debug.Log($"{robotName} resumes movement from {from} to {to}");
        yield return StartMove(robotName, from, to);
    }

    void StartCharging(string robotName)
    {
        Debug.Log($"{robotName} starts charging");
        isCharging = true;
        StartCoroutine(ChargingProcess(robotName));
    }

    void StopCharging(string robotName)
    {
        Debug.Log($"{robotName} stops charging");
        isCharging = false;
    }

    IEnumerator ChargingProcess(string robotName)
    {
        while (isCharging && battery < 100f)
        {
            battery += (100f - battery) * 0.2f * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator PickObject(string objectName, string location, string robotName)
    {
        if (!objects.TryGetValue(objectName, out var obj) || !robots.TryGetValue(robotName, out var robot))
        {
            Debug.LogWarning($"Missing object or robot: {objectName}, {robotName}");
            yield break;
        }

        Debug.Log($"{robotName} picks up {objectName}");
        obj.transform.SetParent(robot.transform);
        obj.transform.localPosition = new Vector3(0, 1f, 0);
        yield return null;
    }

    IEnumerator DropObject(string objectName, string location, string robotName)
    {
        if (!objects.TryGetValue(objectName, out var obj) || !gardens.TryGetValue(location, out var garden))
        {
            Debug.LogWarning($"Missing object or location: {objectName}, {location}");
            yield break;
        }

        Debug.Log($"{robotName} drops {objectName} at {location}");
        obj.transform.SetParent(null);
        obj.transform.position = new Vector3(garden.transform.position.x, robotHeight + 0.5f, garden.transform.position.z);
        yield return null;
    }
}
