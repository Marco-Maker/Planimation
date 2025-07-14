using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class LogisticPlanExecutorEvent : MonoBehaviour
{
    public float actionDelay = 1.0f;
    private Dictionary<string, GameObject> packages;
    private Dictionary<string, GameObject> vehicles;
    private Dictionary<string, GameObject> places;
    private Dictionary<string, float> fuelLevels = new Dictionary<string, float>();
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

            if (trimmed.Contains("-----waiting----"))
                continue;

            if (trimmed.Contains(": (") && trimmed.Contains(")"))
            {
                planLines.Add(trimmed);
            }
        }

        planText = string.Join("\n", planLines);
    }

    void FindSceneObjects()
    {
        packages = GameObject.FindGameObjectsWithTag("Package").ToDictionary(p => p.name.ToLower());
        vehicles = GameObject.FindGameObjectsWithTag("Vehicle").ToDictionary(v => v.name.ToLower());
        places = GameObject.FindGameObjectsWithTag("Place").ToDictionary(p => p.name.ToLower());

        foreach (var pkg in packages)
            initialPositions[pkg.Key] = pkg.Value.transform.position;

        foreach (var veh in vehicles)
            fuelLevels[veh.Key] = 100f;
    }

    IEnumerator ExecutePlan()
    {
        Debug.Log("Plan");
        Debug.Log(planText);
        var lines = planText.Split('\n');
        Debug.Log("Lines");
        Debug.Log(lines);
        Debug.Log("First");
        Debug.Log(lines[0]);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            int parenStart = line.IndexOf('(');
            int parenEnd = line.LastIndexOf(')');
            if (parenStart == -1 || parenEnd == -1) continue;

            string action = line.Substring(parenStart + 1, parenEnd - parenStart - 1).ToLower();
            Debug.Log($"Executing: {action}");

            GameObject.FindWithTag("ActionText").GetComponent<TextMeshProUGUI>().text = action;
            yield return ExecuteAction(action);
            yield return new WaitForSeconds(actionDelay);
        }
    }

    IEnumerator ExecuteAction(string action)
    {
        string[] parts = action.Split(' ');

        switch (parts[0])
        {
            case "load-truck":
            case "unload-truck":
                yield return HandleTruckLoading(parts);
                break;

            case "load-airplane":
            case "unload-airplane":
                yield return HandleAirplaneLoading(parts);
                break;

            case "start-drive":
                yield return DriveVehicle(parts[1], parts[2], parts[3]);
                break;

            case "start-fly":
                yield return FlyAirplane(parts[1], parts[2], parts[3]);
                break;

            case "refuel-truck":
            case "refuel-airplane":
                Refuel(parts[1]);
                break;

            default:
                Debug.LogWarning($"Unknown or unhandled action: {action}");
                break;
        }
    }

    void Refuel(string vehicleName)
    {
        string key = vehicleName.ToLower();
        if (fuelLevels.ContainsKey(key))
        {
            fuelLevels[key] = 100f;
            Debug.Log($"[{vehicleName}] Refueled to 100");
        }
    }

    IEnumerator DriveVehicle(string truckName, string from, string to)
    {
        string key = truckName.ToLower();
        if (!vehicles.ContainsKey(key) || !places.ContainsKey(to)) yield break;

        GameObject truck = vehicles[key];
        Vector3 target = places[to].transform.position;
        target.y = truck.transform.position.y;

        if (!HasFuel(key, 10f)) yield break;

        Debug.Log($"Driving truck {truckName} from {from} to {to}");
        yield return MoveToPosition(truck, target);
        ConsumeFuel(key, 10f);
    }

    IEnumerator FlyAirplane(string airplaneName, string from, string to)
    {
        string key = airplaneName.ToLower();
        if (!vehicles.ContainsKey(key) || !places.ContainsKey(to)) yield break;

        GameObject airplane = vehicles[key];
        Vector3 target = places[to].transform.position;
        target.y = airplane.transform.position.y;

        if (!HasFuel(key, 20f)) yield break;

        Debug.Log($"Flying airplane {airplaneName} from {from} to {to}");
        yield return MoveToPosition(airplane, target);
        ConsumeFuel(key, 20f);
    }

    bool HasFuel(string vehicle, float required)
    {
        if (!fuelLevels.ContainsKey(vehicle)) return false;
        if (fuelLevels[vehicle] < required)
        {
            Debug.LogWarning($"{vehicle} has not enough fuel ({fuelLevels[vehicle]} < {required})");
            return false;
        }
        return true;
    }

    void ConsumeFuel(string vehicle, float amount)
    {
        if (fuelLevels.ContainsKey(vehicle))
        {
            fuelLevels[vehicle] -= amount;
            Debug.Log($"{vehicle} consumed {amount} fuel. Remaining: {fuelLevels[vehicle]}");
        }
    }

    IEnumerator HandleTruckLoading(string[] parts)
    {
        string action = parts[0];
        string pkgName = parts[1];
        string truckName = parts[2];
        string loc = parts[3];

        GameObject pkg = FindObject(packages, pkgName);
        GameObject truck = FindObject(vehicles, truckName);
        if (pkg == null || truck == null || !places.ContainsKey(loc)) yield break;

        if (action == "load-truck")
        {
            yield return MoveToPosition(pkg, truck.transform.position);
            pkg.transform.SetParent(truck.transform);
            pkg.transform.localPosition = Vector3.zero;
        }
        else
        {
            pkg.transform.SetParent(null);
            Vector3 dropPos = places[loc].transform.position;
            dropPos.y = truck.transform.position.y;
            pkg.transform.position = dropPos;
        }
    }

    IEnumerator HandleAirplaneLoading(string[] parts)
    {
        string action = parts[0];
        string pkgName = parts[1];
        string airplaneName = parts[2];
        string airport = parts[3];

        GameObject pkg = FindObject(packages, pkgName);
        GameObject airplane = FindObject(vehicles, airplaneName);
        if (pkg == null || airplane == null || !places.ContainsKey(airport)) yield break;

        if (action == "load-airplane")
        {
            yield return MoveToPosition(pkg, airplane.transform.position);
            pkg.transform.SetParent(airplane.transform);
            pkg.transform.localPosition = Vector3.zero;
        }
        else
        {
            pkg.transform.SetParent(null);
            Vector3 dropPos = places[airport].transform.position;
            dropPos.y = airplane.transform.position.y;
            pkg.transform.position = dropPos;
        }
    }

    IEnumerator MoveToPosition(GameObject obj, Vector3 target, float speed = 10f)
    {
        while (Vector3.Distance(obj.transform.position, target) > 0.01f)
        {
            Vector3 direction = (target - obj.transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            obj.transform.position = Vector3.MoveTowards(obj.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    GameObject FindObject(Dictionary<string, GameObject> dict, string namePart)
    {
        foreach (var kvp in dict)
        {
            if (kvp.Key.Contains(namePart.ToLower()))
                return kvp.Value;
        }
        Debug.LogWarning($"Missing object: {namePart}");
        return null;
    }
}
