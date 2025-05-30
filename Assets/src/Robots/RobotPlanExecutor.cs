using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RobotPlanExecutor : MonoBehaviour
{
    public float actionDelay = 1.0f;
    public string planText;

    private Dictionary<string, GameObject> rooms;
    private Dictionary<string, GameObject> robots;
    private Dictionary<string, GameObject> objects;

    private string planFilePath;
    private float robotHeight;

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
        rooms = GameObject.FindGameObjectsWithTag("Room").ToDictionary(r => r.name.ToLower());
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
            case "move":
                yield return MoveRobot(parts[1], parts[2], parts[3]);
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

    IEnumerator MoveRobot(string robotName, string fromRoom, string toRoom)
    {
        if (!robots.TryGetValue(robotName, out var robot) || !rooms.TryGetValue(toRoom, out var room))
        {
            Debug.LogWarning($"Missing robot or room: {robotName}, {toRoom}");
            yield break;
        }

        robot.transform.position = new Vector3(
            robot.transform.position.x,
            robotHeight,
            robot.transform.position.z
        );

        Vector3 targetPosition = new Vector3(
            room.transform.position.x,
            robotHeight,
            room.transform.position.z
        );

        Debug.Log($"Moving {robotName} from {fromRoom} to {toRoom}");
        yield return MoveToPosition(robot, targetPosition);
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

        // Attach object to robot
        obj.transform.SetParent(robot.transform);
        obj.transform.localPosition = new Vector3(0, 1f, 0); // Adjust local position
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
    }

    IEnumerator MoveToPosition(GameObject obj, Vector3 target, float speed = 5f)
    {
        while (Vector3.Distance(obj.transform.position, target) > 0.01f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }
}
