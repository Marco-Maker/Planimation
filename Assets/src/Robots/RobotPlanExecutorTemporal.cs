using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class RobotPlanExecutorTemporal : MonoBehaviour
{
    public float actionDelay = 1.0f;
    private Dictionary<string, GameObject> rooms;
    private Dictionary<string, GameObject> robots;
    private Dictionary<string, GameObject> objects;
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
            Debug.LogError($"❌ Piano non trovato: {planFilePath}");
            return;
        }
        string[] allLines = File.ReadAllLines(planFilePath);
        List<string> planLines = new List<string>();

        foreach (string line in allLines)
        {
            Debug.Log($"📄 Linea del piano: {line}");
            string trimmed = line.Trim();
            if (trimmed.Length > 0 && char.IsDigit(trimmed[0]) && trimmed.Contains(": ("))
            {
                planLines.Add(trimmed);
            }
        }

        planText = string.Join("\n", planLines);
    }

    void FindSceneObjects()
    {
        rooms = GameObject.FindGameObjectsWithTag("Room").ToDictionary(r => r.name.ToLower());
        robots = GameObject.FindGameObjectsWithTag("Robot").ToDictionary(r => r.name.ToLower());
        objects = GameObject.FindGameObjectsWithTag("Ball").ToDictionary(o => o.name.ToLower());

        foreach (var obj in objects)
        {
            initialPositions[obj.Key] = obj.Value.transform.position;
        }
    }

    IEnumerator ExecutePlan()
    {
        var lines = planText.Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            int index = line.IndexOf(": (");
            if (index == -1) continue;

            string action = line.Substring(index + 3);
            int durationStart = action.LastIndexOf(')');
            if (durationStart != -1)
                action = action.Substring(0, durationStart);

            yield return ExecuteAction(action.ToLower().Trim());
            yield return new WaitForSeconds(actionDelay);
        }

        Debug.Log("✅ Piano completato.");
    }

    IEnumerator ExecuteAction(string action)
    {
        Debug.Log($"⚙️ Eseguo: {action}");
        GameObject.FindWithTag("ActionText").GetComponent<TextMeshProUGUI>().text = action;
        string[] parts = action.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0) yield break;

        switch (parts[0])
        {
            case "move":
                if (parts.Length >= 4)
                    yield return MoveRobot(parts[1], parts[2], parts[3]);
                break;
            case "pick":
                if (parts.Length >= 4)
                    yield return PickObject(parts[1], parts[2], parts[3]);
                break;
            case "drop":
                if (parts.Length >= 4)
                    yield return DropObject(parts[1], parts[2], parts[3]);
                break;
            default:
                Debug.LogWarning($"⚠️ Azione sconosciuta: {action}");
                break;
        }
    }

    IEnumerator MoveRobot(string robotName, string from, string to)
    {
        robotName = robotName.ToLower();
        to = to.ToLower();

        if (!robots.ContainsKey(robotName) || !rooms.ContainsKey(to))
        {
            Debug.LogWarning($"❌ move: '{robotName}' o '{to}' non trovati");
            yield break;
        }

        GameObject robot = robots[robotName];
        GameObject destination = rooms[to];
        Vector3 target = destination.transform.position;
        target.y = robot.transform.position.y;

        yield return MoveToPosition(robot, target);
    }

    IEnumerator PickObject(string objName, string roomName, string robotName)
    {
        objName = objName.ToLower();
        robotName = robotName.ToLower();

        if (!objects.ContainsKey(objName) || !robots.ContainsKey(robotName))
        {
            Debug.LogWarning($"❌ pick: '{objName}' o '{robotName}' non trovati");
            yield break;
        }

        GameObject obj = objects[objName];
        GameObject robot = robots[robotName];

        yield return MoveToPosition(obj, robot.transform.position);
        obj.transform.SetParent(robot.transform);
        obj.transform.localPosition = Vector3.up * 1f;
    }

    IEnumerator DropObject(string objName, string roomName, string robotName)
    {
        objName = objName.ToLower();
        roomName = roomName.ToLower();

        if (!objects.ContainsKey(objName) || !rooms.ContainsKey(roomName))
        {
            Debug.LogWarning($"❌ drop: '{objName}' o '{roomName}' non trovati");
            yield break;
        }

        GameObject obj = objects[objName];
        GameObject room = rooms[roomName];

        obj.transform.SetParent(null);
        Vector3 dropPos = room.transform.position;
        dropPos.y += 1f;
        obj.transform.position = dropPos;
    }

    IEnumerator MoveToPosition(GameObject obj, Vector3 target, float speed = 10f)
    {
        while (Vector3.Distance(obj.transform.position, target) > 0.01f)
        {
            Vector3 dir = (target - obj.transform.position).normalized;

            if (dir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, rot, Time.deltaTime * 5f);
            }

            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }
}
