using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class RobotPlanExecutorTemporal : MonoBehaviour
{
    public float defaultActionDelay = 1.0f;
    public string planText;

    private Dictionary<string, GameObject> rooms;
    private Dictionary<string, GameObject> robots;
    private Dictionary<string, GameObject> objects;

    private string planFilePath;
    private float robotHeight;

    private class TimedAction
    {
        public float startTime;
        public float duration;
        public string action;
    }

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
        var planLines = allLines
            .Where(line => line.Trim().Length > 0 && char.IsDigit(line.Trim()[0]))
            .ToList();

        planText = string.Join("\n", planLines);
    }

    void FindSceneObjects()
    {
        rooms = GameObject.FindGameObjectsWithTag("Room").ToDictionary(r => r.name.ToLower());
        robots = GameObject.FindGameObjectsWithTag("Robot").ToDictionary(r => r.name.ToLower());
        objects = GameObject.FindGameObjectsWithTag("Ball").ToDictionary(o => o.name.ToLower());
    }

    List<TimedAction> ParsePlan()
    {
        var actions = new List<TimedAction>();
        var lines = planText.Split('\n');

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            try
            {
                // es: 0.000: (move r1 room1 room2) [5.000]
                int colonIndex = line.IndexOf(':');
                float start = float.Parse(line.Substring(0, colonIndex), CultureInfo.InvariantCulture);

                string afterColon = line.Substring(colonIndex + 1).Trim();
                int bracketIndex = afterColon.LastIndexOf('[');
                float duration = 0f;

                string actionPart = afterColon;
                if (bracketIndex >= 0)
                {
                    string durationStr = afterColon.Substring(bracketIndex + 1).Trim(' ', ']');
                    duration = float.Parse(durationStr, CultureInfo.InvariantCulture);
                    actionPart = afterColon.Substring(0, bracketIndex).Trim();
                }

                string action = actionPart.Trim('(', ')').ToLower();

                actions.Add(new TimedAction
                {
                    startTime = start,
                    duration = duration,
                    action = action
                });
            }
            catch
            {
                Debug.LogWarning("Failed to parse plan line: " + line);
            }
        }

        return actions;
    }

    IEnumerator ExecutePlan()
    {
        var actions = ParsePlan();

        float currentTime = 0f;
        foreach (var timedAction in actions.OrderBy(a => a.startTime))
        {
            float waitTime = timedAction.startTime - currentTime;
            if (waitTime > 0f)
                yield return new WaitForSeconds(waitTime);

            currentTime = timedAction.startTime;

            yield return ExecuteAction(timedAction.action, timedAction.duration);
        }
    }

    IEnumerator ExecuteAction(string action, float duration)
    {
        string[] parts = action.Split(' ');

        switch (parts[0])
        {
            case "move":
                yield return MoveRobot(parts[1], parts[2], parts[3], duration);
                break;

            case "pick":
                yield return PickObject(parts[1], parts[2], parts[3]);
                break;

            case "drop":
                yield return DropObject(parts[1], parts[2], parts[3]);
                break;

            default:
                Debug.LogWarning($"Unknown action: {action}");
                break;
        }
    }

    IEnumerator MoveRobot(string robotName, string fromRoom, string toRoom, float duration)
    {
        if (!robots.TryGetValue(robotName, out var robot) || !rooms.TryGetValue(toRoom, out var room))
        {
            Debug.LogWarning($"Missing robot or room: {robotName}, {toRoom}");
            yield break;
        }

        Vector3 targetPosition = new Vector3(
            room.transform.position.x,
            robotHeight,
            room.transform.position.z
        );

        Debug.Log($"Moving {robotName} from {fromRoom} to {toRoom} over {duration} seconds");
        yield return MoveToPosition(robot, targetPosition, duration);
    }

    IEnumerator MoveToPosition(GameObject obj, Vector3 target, float duration)
    {
        Vector3 start = obj.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = target;
    }

    IEnumerator PickObject(string objectName, string roomName, string robotName)
    {
        if (!objects.TryGetValue(objectName, out var obj) || 
            !robots.TryGetValue(robotName, out var robot))
        {
            Debug.LogWarning($"Missing object or robot: {objectName}, {robotName}");
            yield break;
        }

        Debug.Log($"{robotName} picking up {objectName} in {roomName}");

        obj.transform.SetParent(robot.transform);
        obj.transform.localPosition = new Vector3(0, 1f, 0);
        yield return null;
    }

    IEnumerator DropObject(string objectName, string roomName, string robotName)
    {
        if (!robots.TryGetValue(robotName, out var robot) || 
            !objects.TryGetValue(objectName, out var obj) || 
            !rooms.TryGetValue(roomName, out var room))
        {
            Debug.LogWarning($"Missing robot/object/room: {robotName}, {objectName}, {roomName}");
            yield break;
        }

        Debug.Log($"{robotName} dropping {objectName} in {roomName}");

        obj.transform.SetParent(null);
        obj.transform.position = new Vector3(
            room.transform.position.x,
            room.transform.position.y + 1f,
            room.transform.position.z
        );

        yield return null;
    }
}

