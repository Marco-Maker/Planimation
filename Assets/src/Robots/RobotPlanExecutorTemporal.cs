using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(ProblemGenerator))]
public class RobotPlanExecutorTemporal : MonoBehaviour
{
    [Header("Domain PDDL (assegna in Inspector)")]
    [Tooltip("File .pddl del dominio, importato come TextAsset")]
    [SerializeField] private TextAsset domainPddlAsset;

    [Header("Default action delay (fallback)")]
    public float defaultActionDelay = 1.0f;

    // verrà popolato con il testo restituito dal planner
    public string planText;

    private Dictionary<string, GameObject> rooms, robots, objects;
    private float robotHeight;

    // oggetto di supporto per le azioni temporali
    private class TimedAction
    {
        public float startTime;
        public float duration;
        public string action;
    }

    void Start()
    {
        // 0) Controlli preliminari
        if (domainPddlAsset == null)
        {
            Debug.LogError("⚠️ Assegna il TextAsset del dominio PDDL in Inspector!");
            return;
        }

        // 1) Carica il problema dal file generato
        string problemRel = Const.PDDL_FOLDER.TrimStart('/', '\\') + Const.PROBLEM; 
        string problemPath = Path.Combine(Application.dataPath, problemRel);
        if (!File.Exists(problemPath))
        {
            Debug.LogError($"PDDL problem non trovato: {problemPath}");
            return;
        }
        string problemPddl = File.ReadAllText(problemPath);

        // 2) Prendi il dominio dal TextAsset
        string domainPddl = domainPddlAsset.text;

        Debug.Log("[DEBUG] Domain PDDL (prima di invio):\n" +
                  domainPddl.Substring(0, Mathf.Min(200, domainPddl.Length)) + "…");
        Debug.Log("[DEBUG] Problem PDDL (prima di invio):\n" +
                  problemPddl.Substring(0, Mathf.Min(200, problemPddl.Length)) + "…");

        // 3) Chiamata al planner remoto
        StartCoroutine(RemotePlan(domainPddl, problemPddl));
    }

    private IEnumerator RemotePlan(string domainPddl, string problemPddl)
    {
        Debug.Log("[RobotPlanExecutorTemporal] Invio al planner…");

        List<string> planLines = null;
        string errorMsg = null;

        yield return StartCoroutine(
            PddlApiClient.RequestPlan(
                domainPddl,
                problemPddl,
                steps => planLines = steps,
                err   => errorMsg  = err
            )
        );

        if (!string.IsNullOrEmpty(errorMsg))
        {
            Debug.LogError("Errore OPTIC: " + errorMsg);
            yield break;
        }

        // 4) Prepara esecuzione
        planText = string.Join("\n", planLines);
        Debug.Log("📄 Piano OPTIC ricevuto:\n" + planText);

        // 5) Inizializza riferimenti scena
        rooms  = GameObject.FindGameObjectsWithTag("Room")
                    .ToDictionary(r => r.name.ToLower());
        robots = GameObject.FindGameObjectsWithTag("Robot")
                    .ToDictionary(r => r.name.ToLower());
        objects= GameObject.FindGameObjectsWithTag("Ball")
                    .ToDictionary(o => o.name.ToLower());
        robotHeight = robots.Values.First().transform.position.y;

        // 6) Esegui il piano
        StartCoroutine(ExecutePlan());
    }

    private List<TimedAction> ParsePlan()
    {
        var actions = new List<TimedAction>();

        foreach (var raw in planText.Split('\n'))
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            try
            {
                int ci = line.IndexOf(':');
                float start = float.Parse(line.Substring(0, ci), CultureInfo.InvariantCulture);

                string after = line.Substring(ci + 1).Trim();
                int bi = after.LastIndexOf('[');

                float dur = defaultActionDelay;
                string part = after;
                if (bi >= 0)
                {
                    dur = float.Parse(after.Substring(bi + 1).Trim(' ', ']'),
                                      CultureInfo.InvariantCulture);
                    part = after.Substring(0, bi).Trim();
                }

                string act = part.Trim('(', ')').ToLower();
                actions.Add(new TimedAction { startTime = start, duration = dur, action = act });
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to parse plan line: " + line + " — " + ex.Message);
            }
        }

        return actions;
    }

    private IEnumerator ExecutePlan()
    {
        var actions = ParsePlan()
            .OrderBy(a => a.startTime)
            .ToList();

        float current = 0f;
        foreach (var ta in actions)
        {
            float wait = ta.startTime - current;
            if (wait > 0f) yield return new WaitForSeconds(wait);
            current = ta.startTime;

            yield return ExecuteAction(ta.action, ta.duration);
        }
    }

    private IEnumerator ExecuteAction(string action, float duration)
    {
        var parts = action.Split(' ');
        switch (parts[0])
        {
            case "move":
                if (parts.Length >= 4)
                    yield return MoveRobot(parts[1], parts[2], parts[3], duration);
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
                Debug.LogWarning($"Unknown action: {action}");
                break;
        }
    }

    private IEnumerator MoveRobot(string robotName, string fromRoom, string toRoom, float duration)
    {
        if (!robots.TryGetValue(robotName, out var robot) ||
            !rooms.TryGetValue(toRoom,   out var room))
        {
            Debug.LogWarning($"Missing robot or room: {robotName}, {toRoom}");
            yield break;
        }

        Vector3 target = new Vector3(
            room.transform.position.x,
            robotHeight,
            room.transform.position.z
        );
        yield return MoveToPosition(robot, target, duration);
    }

    private IEnumerator MoveToPosition(GameObject obj, Vector3 target, float duration)
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

    private IEnumerator PickObject(string objectName, string roomName, string robotName)
    {
        if (!objects.TryGetValue(objectName, out var obj) ||
            !robots.TryGetValue(robotName, out var robot))
        {
            Debug.LogWarning($"Missing object or robot: {objectName}, {robotName}");
            yield break;
        }
        obj.transform.SetParent(robot.transform);
        obj.transform.localPosition = Vector3.up * 1f;
        yield return null;
    }

    private IEnumerator DropObject(string objectName, string roomName, string robotName)
    {
        if (!robots.TryGetValue(robotName, out var robot) ||
            !objects.TryGetValue(objectName, out var obj)   ||
            !rooms.TryGetValue(roomName,   out var room))
        {
            Debug.LogWarning($"Missing robot/object/room: {robotName}, {objectName}, {roomName}");
            yield break;
        }
        obj.transform.SetParent(null);
        obj.transform.position = room.transform.position + Vector3.up * 1f;
        yield return null;
    }
}
