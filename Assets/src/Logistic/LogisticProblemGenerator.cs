using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogisticProblemGenerator : MonoBehaviour
{

    [SerializeField] private GameObject truckPrefab;
    [SerializeField] private GameObject vanPrefab;
    [SerializeField] private GameObject packagePrefab;
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private GameObject locationPrefab;
    [SerializeField] private GameObject airportPrefab;
    [SerializeField] private GameObject bridgePrefab;

    [SerializeField] private int numberOfCities = 3;
    [SerializeField] private int locationsPerCity = 3;
    [SerializeField] private int numberOfAirports = 0;
    [SerializeField] private int numberOfHubs = 0;

    [SerializeField] private float cityRadius = 100f;
    [SerializeField] private float bridgeWidth = 25f;
    [SerializeField] private float citySpacing = 150f;
    [SerializeField] private float locationRadius = 25f;

    private List<GameObject> cityList = new List<GameObject>();

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        Dictionary<string, List<string>> objectMap = new Dictionary<string, List<string>>();

        // Classifica gli oggetti
        foreach (var o in PlanInfo.GetInstance().GetObjects())
        {
            Debug.Log(o.name);
            string key = "";

            if (o.name.StartsWith("city"))
                key = "cities";
            else if (o.name.StartsWith("location"))
                key = "locations";
            else if (o.name.StartsWith("airport"))
                key = "airports";
            else if (o.name.StartsWith("hub"))
                key = "hubs";
            else if (o.name.StartsWith("truck"))
                key = "trucks";
            else if (o.name.StartsWith("van"))
                key = "vans";
            else if (o.name.StartsWith("package"))
                key = "packages";
            else if (o.name.StartsWith("airplane"))
                key = "airplanes";

            if (!string.IsNullOrEmpty(key))
            {
                if (!objectMap.ContainsKey(key))
                    objectMap[key] = new List<string>();

                objectMap[key].Add(o.name);
            }
        }

        // Crea le città
        List<GameObject> cityList = new List<GameObject>();
        if (objectMap.ContainsKey("cities"))
        {
            for (int i = 0; i < objectMap["cities"].Count; i++)
            {
                Vector3 cityPosition = new Vector3(i * citySpacing, 0, 0);
                GameObject city = Instantiate(cityPrefab, cityPosition, Quaternion.identity, transform);
                city.name = objectMap["cities"][i];
                cityList.Add(city);
            }
        }

        // Crea le location attorno alle città
        if (objectMap.ContainsKey("locations"))
        {
            int locIndex = 0;
            foreach (GameObject city in cityList)
            {
                for (int j = 0; j < 3 && locIndex < objectMap["locations"].Count; j++, locIndex++)
                {
                    float angle = j * Mathf.PI * 2 / 3;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * locationRadius;
                    GameObject location = Instantiate(locationPrefab, city.transform.position + offset, Quaternion.identity, city.transform);
                    location.name = objectMap["locations"][locIndex];
                }
            }
        }

        // Posiziona aeroporti
        if (objectMap.ContainsKey("airports"))
        {
            for (int i = 0; i < objectMap["airports"].Count && i < cityList.Count; i++)
            {
                Instantiate(airportPrefab, cityList[i].transform.position + Vector3.up * 1.5f, Quaternion.identity, cityList[i].transform).name = objectMap["airports"][i];
            }
        }

        // Crea ponti tra città
        for (int i = 0; i < cityList.Count - 1; i++)
        {
            Vector3 from = cityList[i].transform.position;
            Vector3 to = cityList[i + 1].transform.position;
            Vector3 bridgePosition = (from + to) / 2f;
            GameObject bridge = Instantiate(bridgePrefab, bridgePosition, Quaternion.LookRotation(to - from));
            bridge.transform.localScale = new Vector3(bridgeWidth, 1, Vector3.Distance(from, to));
            bridge.name = $"Bridge_{i}_{i + 1}";
        }
    }

}
