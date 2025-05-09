using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogisticProblemGenerator : MonoBehaviour
{
    [SerializeField] private GameObject cityPrefab;
    [SerializeField] private GameObject locationPrefab;
    [SerializeField] private GameObject airportPrefab;
    [SerializeField] private GameObject hubPrefab;
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
        for (int i = 0; i < numberOfCities; i++)
        {
            Vector3 cityPosition = new Vector3(i * citySpacing, 0, 0);
            GameObject city = Instantiate(cityPrefab, cityPosition, Quaternion.identity, transform);
            city.name = $"City_{i}";
            cityList.Add(city);

            for (int j = 0; j < locationsPerCity; j++)
            {
                float angle = j * Mathf.PI * 2 / locationsPerCity;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * locationRadius;
                GameObject location = Instantiate(locationPrefab, cityPosition + offset, Quaternion.identity, city.transform);
                location.name = $"Location_{i}_{j}";
            }
        }

        // Posiziona aeroporti
        for (int i = 0; i < numberOfAirports && i < cityList.Count; i++)
        {
            Instantiate(airportPrefab, cityList[i].transform.position + Vector3.up * 1.5f, Quaternion.identity, cityList[i].transform).name = $"Airport_{i}";
        }

        // Posiziona HUB 
        for (int i = 0; i < numberOfHubs && i < cityList.Count; i++)
        {
            Instantiate(hubPrefab, cityList[cityList.Count - 1 - i].transform.position + Vector3.up * 3f, Quaternion.identity, cityList[cityList.Count - 1 - i].transform).name = $"Hub_{i}";
        }

        // Creazione ponti tra città
        for (int i = 0; i < cityList.Count - 1; i++)
        {
            Vector3 from = cityList[i].transform.position;
            Vector3 to = cityList[i + 1].transform.position;
            Vector3 bridgePosition = (from + to) / 2f;
            Vector3 scale = new Vector3(1, 1, Vector3.Distance(from, to));
            GameObject bridge = Instantiate(bridgePrefab, bridgePosition, Quaternion.LookRotation(to - from));
            bridge.transform.localScale = new Vector3(bridgeWidth, 1, Vector3.Distance(from, to));
            bridge.name = $"Bridge_{i}_{i + 1}";
        }
    }
}
