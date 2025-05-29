using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogisticProblemGenerator : MonoBehaviour
{

    [SerializeField] private GameObject airplanePrefab;
    [SerializeField] private GameObject truckPrefab;
    [SerializeField] private GameObject vanPrefab;
    [SerializeField] private GameObject packagePrefab;
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private GameObject locationPrefab;
    [SerializeField] private GameObject airportPrefab;
    [SerializeField] private GameObject bridgePrefab;

    [SerializeField] private float cityRadius = 100f;
    [SerializeField] private float bridgeWidth = 25f;
    [SerializeField] private float citySpacing = 150f;
    [SerializeField] private float locationRadius = 25f;

    void Awake()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        var planInfo = PlanInfo.GetInstance();

        Dictionary<string, List<string>> objectMap = new Dictionary<string, List<string>>();
        Dictionary<string, string> inCityMap = new Dictionary<string, string>();  // place → city
        List<(string, string)> cityLinks = new List<(string, string)>();         // (cityA, cityB)
        Dictionary<string, string> atMap = new Dictionary<string, string>();     // physobj → place
        Dictionary<string, string> inMap = new Dictionary<string, string>();     // package → vehicle

        // 🔹 1. Leggo i predicati e costruisco le mappe
        foreach (var p in planInfo.GetPredicates())
        {
            if (p.name == "in-city" && p.values.Count == 2)
            {
                inCityMap[p.values[0]] = p.values[1];
            }
            else if (p.name == "link" && p.values.Count == 2)
            {
                cityLinks.Add((p.values[0], p.values[1]));
            }
            else if (p.name == "at" && p.values.Count == 2)
            {
                atMap[p.values[0]] = p.values[1];
            }
            else if (p.name == "in" && p.values.Count == 2)
            {
                inMap[p.values[0]] = p.values[1];
            }
        }

        // 🔹 2. Classifica gli oggetti come prima
        foreach (var o in planInfo.GetObjects())
        {
            string key = "";

            if (o.name.StartsWith("city")) key = "cities";
            else if (o.name.StartsWith("location")) key = "locations";
            else if (o.name.StartsWith("airport")) key = "airports";
            else if (o.name.StartsWith("hub")) key = "hubs";
            else if (o.name.StartsWith("truck")) key = "trucks";
            else if (o.name.StartsWith("van")) key = "vans";
            else if (o.name.StartsWith("package")) key = "packages";
            else if (o.name.StartsWith("airplane")) key = "airplanes";

            if (!string.IsNullOrEmpty(key))
            {
                if (!objectMap.ContainsKey(key))
                    objectMap[key] = new List<string>();

                objectMap[key].Add(o.name);
            }
        }

        // 🔹 3. Crea le città in modo radiale attorno alle città collegate
        Dictionary<string, GameObject> cityGameObjects = new Dictionary<string, GameObject>();
        HashSet<string> placedCities = new HashSet<string>();
        Dictionary<string, List<string>> cityAdjacency = new Dictionary<string, List<string>>();

        // Mappa città → città collegate
        foreach (var (a, b) in cityLinks)
        {
            if (!cityAdjacency.ContainsKey(a)) cityAdjacency[a] = new List<string>();
            if (!cityAdjacency.ContainsKey(b)) cityAdjacency[b] = new List<string>();
            cityAdjacency[a].Add(b);
            cityAdjacency[b].Add(a);
        }

        // Posizioni già usate (con tolleranza)
        List<Vector3> usedPositions = new List<Vector3>();

        bool IsPositionOccupied(Vector3 pos, float threshold = 1f)
        {
            foreach (var p in usedPositions)
            {
                if (Vector3.Distance(p, pos) < threshold)
                    return true;
            }
            return false;
        }

        void PlaceCity(string cityName, Vector3 position, string parentCity = null, int depth = 0)
        {
            if (placedCities.Contains(cityName)) return;

            if (IsPositionOccupied(position))
            {
                // Cerca una posizione libera attorno
                const int maxAttempts = 12;
                for (int i = 0; i < maxAttempts; i++)
                {
                    float angle = i * Mathf.PI * 2 / maxAttempts;
                    Vector3 tryOffset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * citySpacing;
                    Vector3 tryPosition = position + tryOffset;

                    if (!IsPositionOccupied(tryPosition))
                    {
                        position = tryPosition;
                        break;
                    }
                }
            }

            GameObject city = Instantiate(cityPrefab, position, Quaternion.identity, transform);
            city.name = cityName;
            cityGameObjects[cityName] = city;
            placedCities.Add(cityName);
            usedPositions.Add(position);

            if (!cityAdjacency.ContainsKey(cityName)) return;

            List<string> neighbors = cityAdjacency[cityName];

            float angleStep = 360f / Mathf.Max(neighbors.Count, 1);
            float baseAngle = Random.Range(0f, 360f); // per varietà

            int index = 0;
            foreach (var neighbor in neighbors)
            {
                if (placedCities.Contains(neighbor)) continue;

                float angle = (baseAngle + index * angleStep) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * citySpacing;
                Vector3 neighborPosition = position + offset;

                PlaceCity(neighbor, neighborPosition, cityName, depth + 1);
                index++;
            }
        }


        if (objectMap.ContainsKey("cities") && objectMap["cities"].Count > 0)
        {
            string startCity = objectMap["cities"][0];
            PlaceCity(startCity, Vector3.zero);
        }

        // 🔹 4. Crea locations e airports usando in-city
        Dictionary<string, GameObject> placeGameObjects = new Dictionary<string, GameObject>();

        foreach (var kvp in inCityMap)
        {
            string placeName = kvp.Key;
            string cityName = kvp.Value;

            if (!cityGameObjects.ContainsKey(cityName))
                continue;

            GameObject cityGO = cityGameObjects[cityName];

            GameObject prefab = null;
            float radius = locationRadius;

            if (placeName.StartsWith("location"))
                prefab = locationPrefab;
            else if (placeName.StartsWith("airport"))
                prefab = airportPrefab;

            if (prefab != null)
            {
                int siblingCount = cityGO.transform.childCount;
                float angle = siblingCount * Mathf.PI * 2 / 6;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0.01f, Mathf.Sin(angle)) * radius;

                GameObject placeGO = Instantiate(prefab, cityGO.transform.position + offset, Quaternion.identity, cityGO.transform);
                placeGO.name = placeName;

                placeGameObjects[placeName] = placeGO;
            }
        }

        // 🔹 5. Crea ponti in base ai link
        foreach (var link in cityLinks)
        {
            if (cityGameObjects.ContainsKey(link.Item1) && cityGameObjects.ContainsKey(link.Item2))
            {
                Vector3 from = cityGameObjects[link.Item1].transform.position;
                Vector3 to = cityGameObjects[link.Item2].transform.position;
                Vector3 bridgePosition = (from + to) / 2f;

                GameObject bridge = Instantiate(bridgePrefab, bridgePosition, Quaternion.LookRotation(to - from));
                bridge.transform.localScale = new Vector3(bridgeWidth, 1, Vector3.Distance(from, to));
                bridge.name = $"Bridge_{link.Item1}_{link.Item2}";
            }
        }

        // 🔹 6. Crea physobj (airplane, truck, package) usando at
        Dictionary<string, GameObject> physObjGameObjects = new Dictionary<string, GameObject>();

        foreach (var kvp in atMap)
        {
            string physObjName = kvp.Key;
            string placeName = kvp.Value;

            if (!placeGameObjects.ContainsKey(placeName))
                continue;

            GameObject placeGO = placeGameObjects[placeName];

            GameObject prefab = null;

            if (physObjName.StartsWith("truck"))
                prefab = truckPrefab;
            else if (physObjName.StartsWith("airplane"))
                prefab = airplanePrefab;
            else if (physObjName.StartsWith("package"))
                prefab = packagePrefab;

            if (prefab != null)
            {
                int siblingCount = placeGO.transform.childCount;
                Vector3 offset = Vector3.up * (1 + siblingCount * 0.5f);

                GameObject physObjGO = Instantiate(prefab, placeGO.transform.position, Quaternion.identity, placeGO.transform);
                physObjGO.name = physObjName;

                physObjGameObjects[physObjName] = physObjGO;
            }
        }

        // 🔹 7. Gestisci i package che sono dentro i vehicles (in)
        foreach (var kvp in inMap)
        {
            string packageName = kvp.Key;
            string vehicleName = kvp.Value;

            if (physObjGameObjects.ContainsKey(packageName) && physObjGameObjects.ContainsKey(vehicleName))
            {
                GameObject packageGO = physObjGameObjects[packageName];
                GameObject vehicleGO = physObjGameObjects[vehicleName];

                // Reparent il pacchetto nel vehicle
                packageGO.transform.SetParent(vehicleGO.transform);

                // Posiziona il package sopra al vehicle (piccolo offset visivo)
                packageGO.transform.localPosition = Vector3.up * 0.5f;
            }
        }
    }

    
}
