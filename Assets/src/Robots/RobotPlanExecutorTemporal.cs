using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class RobotPlanExecutorTemporal : MonoBehaviour
{
    public float defaultActionDelay = 1f; // fallback se OPTIC non fornisce [durata]
    public string planText;

    private Dictionary<string, GameObject> rooms, robots, objects;
    private float robotHeight;

    private class TimedAction
    {
        public float startTime;
        public float duration;
        public string action;
    }

    void Start()
    {
        string problemPath = Application.dataPath + "/Generated/problem.pddl";
        string domainPath = Application.dataPath + "/PDDL/AllFile/Robot/domain-robot-temporal.pddl";

        if (!File.Exists(problemPath))
        {
            Debug.LogError($"❌ Problema PDDL non trovato: {problemPath}");
            return;
        }

        if (!File.Exists(domainPath))
        {
            Debug.LogError($"❌ Dominio PDDL non trovato: {domainPath}");
            return;
        }

        string problemPddl = File.ReadAllText(problemPath);
        string domainPddl = File.ReadAllText(domainPath);

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
                err => errorMsg = err
            )
        );

        if (!string.IsNullOrEmpty(errorMsg))
        {
            Debug.LogError("❌ Errore OPTIC: " + errorMsg);
            yield break;
        }

        if (planLines == null || planLines.Count == 0)
        {
            Debug.LogWarning("⚠️ Nessun piano ricevuto.");
            yield break;
        }

        planText = string.Join("\n", planLines);
        Debug.Log("📄 Piano OPTIC ricevuto:\n" + planText);

        FindSceneObjects();
        StartCoroutine(ExecutePlan());
    }

    private void FindSceneObjects()
    {
        rooms = GameObject.FindGameObjectsWithTag("Room").ToDictionary(r => r.name.ToLower());
        robots = GameObject.FindGameObjectsWithTag("Robot").ToDictionary(r => r.name.ToLower());
        objects = GameObject.FindGameObjectsWithTag("Ball").ToDictionary(o => o.name.ToLower());

        if (robots.Count > 0)
            robotHeight = robots.Values.First().transform.position.y;
        else
            robotHeight = 1.5f;
    }

    private List<TimedAction> ParsePlan()
    {
        var actions = new List<TimedAction>();
        foreach (var line in planText.Split('\n'))
        {
            try
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                int sep = trimmed.IndexOf(':');
                float start = float.Parse(trimmed.Substring(0, sep), CultureInfo.InvariantCulture);

                string rest = trimmed.Substring(sep + 1).Trim();
                int durStart = rest.LastIndexOf('[');
                float duration = defaultActionDelay;

                if (durStart >= 0)
                {
                    duration = float.Parse(rest.Substring(durStart + 1).Trim(' ', ']'), CultureInfo.InvariantCulture);
                    rest = rest.Substring(0, durStart).Trim();
                }

                string action = rest.Trim('(', ')').ToLower();
                actions.Add(new TimedAction { startTime = start, duration = duration, action = action });
            }
            catch (Exception e)
            {
                Debug.LogWarning("❗ Errore parsing piano: " + e.Message);
            }
        }

        return actions.OrderBy(a => a.startTime).ToList();
    }

    private IEnumerator ExecutePlan()
    {
        Debug.Log("▶️ Inizio esecuzione piano…");

        var actions = ParsePlan();
        if (actions.Count == 0)
        {
            Debug.LogWarning("❗ Nessuna azione parsata.");
            yield break;
        }

        float currentTime = 0f;

        foreach (var act in actions)
        {
            Debug.Log($"🕒 Azione pianificata: {act.action} (start {act.startTime}s, durata {act.duration}s)");

            float waitTime = act.startTime - currentTime;
            if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

            currentTime = act.startTime;
            yield return ExecuteAction(act.action, act.duration);
        }

        Debug.Log("✅ Piano completato.");
    }

    private IEnumerator ExecuteAction(string action, float duration)
    {
        string[] parts = action.Split(' ');

        switch (parts[0])
        {
            case "move":
                if (parts.Length == 4)
                    yield return MoveRobot(parts[1], parts[2], parts[3], duration);
                break;
            case "pick":
                if (parts.Length == 4)
                    yield return PickObject(parts[1], parts[2], parts[3]);
                break;
            case "drop":
                if (parts.Length == 4)
                    yield return DropObject(parts[1], parts[2], parts[3]);
                break;
            default:
                Debug.LogWarning("⚠️ Azione non riconosciuta: " + action);
                break;
        }

        Debug.Log($"▶️ Eseguo azione: {action} (durata: {duration})");
    }

    private IEnumerator MoveRobot(string robotName, string fromRoom, string toRoom, float duration)
    {
        if (!robots.TryGetValue(robotName, out var robot) ||
            !rooms.TryGetValue(toRoom, out var destRoom))
        {
            Debug.LogWarning($"❗ move: robot '{robotName}' o room '{toRoom}' mancante");
            yield break;
        }

        Vector3 target = new Vector3(destRoom.transform.position.x, robotHeight, destRoom.transform.position.z);
        Vector3 start = robot.transform.position;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            robot.transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        robot.transform.position = target;
        Debug.Log($"✅ Robot '{robotName}' spostato da '{fromRoom}' a '{toRoom}' in {duration} secondi.");
    }

    private IEnumerator PickObject(string objName, string roomName, string robotName)
    {
        if (!objects.TryGetValue(objName, out var obj) ||
            !robots.TryGetValue(robotName, out var robot))
        {
            Debug.LogWarning($"❗ pick: oggetto '{objName}' o robot '{robotName}' mancante");
            yield break;
        }

        obj.transform.SetParent(robot.transform);
        obj.transform.localPosition = Vector3.up * 1f;
        yield return null;
    }

    private IEnumerator DropObject(string objName, string roomName, string robotName)
    {
        if (!objects.TryGetValue(objName, out var obj) ||
            !rooms.TryGetValue(roomName, out var room))
        {
            Debug.LogWarning($"❗ drop: oggetto '{objName}' o room '{roomName}' mancante");
            yield break;
        }

        obj.transform.SetParent(null);
        obj.transform.position = room.transform.position + Vector3.up * 1f;
        yield return null;
    }
}
