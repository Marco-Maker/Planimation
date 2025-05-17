using System.Collections.Generic;
using UnityEngine;

public class RobotProblemGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject corridorPrefab;
    
    [SerializeField] private float corridorHeightOffset = 0.01f; // leggera altezza per non intersecare
    [SerializeField] private float corridorWidth = 1.5f;
    [SerializeField] private float roomSpacing = 6f;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        var planInfo = PlanInfo.GetInstance();

        Dictionary<string, string> atRobbyMap = new();   // robot → room
        Dictionary<string, string> atBallMap = new();    // ball → room
        Dictionary<string, string> carryMap = new();     // robot → ball
        HashSet<string> freeRobots = new();              // robot
        Dictionary<string, List<string>> allowedMap = new(); // robot → rooms
        List<(string, string)> connectedRooms = new();   // roomA ↔ roomB

        // 🔹1. Leggi i predicati
        foreach (var p in planInfo.GetPredicates())
        {
            switch (p.name)
            {
                case "at-robby":
                    atRobbyMap[p.values[0]] = p.values[1];
                    break;
                case "at":
                    atBallMap[p.values[0]] = p.values[1];
                    break;
                case "carry":
                    carryMap[p.values[0]] = p.values[1];
                    break;
                case "free":
                    freeRobots.Add(p.values[0]);
                    break;
                case "allowed":
                    if (!allowedMap.ContainsKey(p.values[0]))
                        allowedMap[p.values[0]] = new List<string>();
                    allowedMap[p.values[0]].Add(p.values[1]);
                    break;
                case "connected":
                    connectedRooms.Add((p.values[0], p.values[1]));
                    break;
            }
        }

        // 🔹2. Crea le stanze
        Dictionary<string, GameObject> roomObjects = new();
        List<string> roomNames = new List<string>(GetUniqueRooms(atRobbyMap, atBallMap, connectedRooms));

        for (int i = 0; i < roomNames.Count; i++)
        {
            Vector3 pos = new Vector3((i % 2 == 0 ? -1 : 1) * roomSpacing, 0, -(i / 2) * roomSpacing);
            GameObject roomGO = Instantiate(roomPrefab, pos, Quaternion.identity, transform);
            roomGO.name = roomNames[i];
            roomObjects[roomNames[i]] = roomGO;
        }

        // 🔹3. Crea i robot
        foreach (var kvp in atRobbyMap)
        {
            string robot = kvp.Key;
            string room = kvp.Value;

            if (!roomObjects.ContainsKey(room)) continue;

            GameObject robotGO = Instantiate(robotPrefab, roomObjects[room].transform.position + Vector3.up, Quaternion.identity, roomObjects[room].transform);
            robotGO.name = robot;

            // Se trasporta una ball
            if (carryMap.ContainsKey(robot))
            {
                string ball = carryMap[robot];
                GameObject ballGO = Instantiate(ballPrefab, robotGO.transform.position + Vector3.up * 0.5f, Quaternion.identity, robotGO.transform);
                ballGO.name = ball;
            }
        }

        // 🔹4. Crea le ball nelle stanze
        foreach (var kvp in atBallMap)
        {
            string ball = kvp.Key;
            string room = kvp.Value;

            if (!roomObjects.ContainsKey(room)) continue;

            GameObject ballGO = Instantiate(ballPrefab, roomObjects[room].transform.position + Vector3.up * 0.5f, Quaternion.identity, roomObjects[room].transform);
            ballGO.name = ball;
        }

        // 🔹5. Visualizza connessioni tra stanze (debug o ponti futuri)
        foreach (var (roomA, roomB) in connectedRooms)
        {
            if (roomObjects.ContainsKey(roomA) && roomObjects.ContainsKey(roomB))
            {
                Debug.DrawLine(roomObjects[roomA].transform.position, roomObjects[roomB].transform.position, Color.green, 10f);
            }
        }

        foreach (var (roomA, roomB) in connectedRooms)
        {
            if (roomObjects.ContainsKey(roomA) && roomObjects.ContainsKey(roomB))
            {
                Vector3 posA = roomObjects[roomA].transform.position;
                Vector3 posB = roomObjects[roomB].transform.position;

                Vector3 centerPos = (posA + posB) / 2f + Vector3.up * corridorHeightOffset;
                Vector3 dir = posB - posA;
                float distance = dir.magnitude;

                GameObject corridor = Instantiate(corridorPrefab, centerPos, Quaternion.LookRotation(dir), transform);
                corridor.transform.localScale = new Vector3(corridorWidth, 1f, distance);
                corridor.name = $"Corridor_{roomA}_{roomB}";
            }
        }

    }

    private HashSet<string> GetUniqueRooms(Dictionary<string, string> atRobby, Dictionary<string, string> atBall, List<(string, string)> connections)
    {
        HashSet<string> rooms = new();
        foreach (var r in atRobby.Values) rooms.Add(r);
        foreach (var r in atBall.Values) rooms.Add(r);
        foreach (var (a, b) in connections)
        {
            rooms.Add(a);
            rooms.Add(b);
        }
        return rooms;
    }
}
