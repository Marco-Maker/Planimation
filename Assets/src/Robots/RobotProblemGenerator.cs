using System.Collections.Generic;
using UnityEngine;

public class RobotProblemGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> roomsPrefab;
    [SerializeField] private List<GameObject> gardenPrefab;
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject corridorPrefab;

    [SerializeField] private float corridorHeightOffset = 0.01f;
    [SerializeField] private float roomSpacing = 6f;
    [SerializeField] private float roomSize = 4f;

    private int roomCounter = 0;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        var planInfo = PlanInfo.GetInstance();
        float domainType = planInfo.GetDomainType();

        Dictionary<string, List<string>> objectMap = new();
        Dictionary<string, string> atRobbyMap = new();
        Dictionary<string, string> atBallMap = new();
        Dictionary<string, string> carryMap = new();
        HashSet<string> freeRobots = new();
        Dictionary<string, List<string>> allowedMap = new();
        List<(string, string)> connectedRooms = new();

        foreach (var p in planInfo.GetPredicates())
        {
            switch (p.name)
            {
                case "at-robot":
                    atRobbyMap[p.values[0]] = p.values[1];
                    break;
                case "at-obj":
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
                    if (domainType == 1.0f)
                        connectedRooms.Add((p.values[0], p.values[1]));
                    break;
                case "path":
                    if (domainType == 1.2f)
                        connectedRooms.Add((p.values[0], p.values[1]));
                    break;
            }
        }

        // Se è dominio temporale ma non ci sono "connected", deduciamo dai "allowed"
        if (domainType == 1.1f && connectedRooms.Count == 0)
        {
            HashSet<(string, string)> tempConnections = new();
            foreach (var list in allowedMap.Values)
            {
                for (int i = 0; i < list.Count; i++)
                    for (int j = i + 1; j < list.Count; j++)
                        tempConnections.Add((list[i], list[j]));
            }
            connectedRooms.AddRange(tempConnections);
        }

        foreach (var o in planInfo.GetObjects())
        {
            string key = "";
            if (o.name.StartsWith("room") || o.name.StartsWith("garden")) key = "rooms";
            else if (o.name.StartsWith("robot")) key = "robots";
            else if (o.name.StartsWith("obj")) key = "objs";

            if (!string.IsNullOrEmpty(key))
            {
                if (!objectMap.ContainsKey(key))
                    objectMap[key] = new List<string>();
                objectMap[key].Add(o.name);
            }
        }

        Dictionary<string, GameObject> roomObjects = new();
        HashSet<string> placedRooms = new();
        Dictionary<string, List<string>> roomAdjacency = new();
        foreach (var (a, b) in connectedRooms)
        {
            if (!roomAdjacency.ContainsKey(a)) roomAdjacency[a] = new();
            if (!roomAdjacency.ContainsKey(b)) roomAdjacency[b] = new();
            roomAdjacency[a].Add(b);
            roomAdjacency[b].Add(a);
        }

        Dictionary<Vector2Int, string> gridPositions = new();

        void PlaceRoom(string roomName, Vector2Int gridPos, int depth = 0)
        {
            if (placedRooms.Contains(roomName) || gridPositions.ContainsKey(gridPos)) return;

            Vector3 worldPos = new Vector3(gridPos.x * roomSpacing, 0, gridPos.y * roomSpacing);
            GameObject roomGO;

            if (roomName.StartsWith("garden") && gardenPrefab.Count > 0)
            {
                int randomIndex = Random.Range(0, gardenPrefab.Count);
                roomGO = Instantiate(gardenPrefab[randomIndex], worldPos, Quaternion.identity, transform);
            }
            else
            {
                roomGO = Instantiate(roomsPrefab[roomCounter % roomsPrefab.Count], worldPos, Quaternion.identity, transform);
            }

            roomCounter++;
            roomGO.name = roomName;
            roomObjects[roomName] = roomGO;
            placedRooms.Add(roomName);
            gridPositions[gridPos] = roomName;

            if (!roomAdjacency.ContainsKey(roomName)) return;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

            foreach (var neighbor in roomAdjacency[roomName])
            {
                if (placedRooms.Contains(neighbor)) continue;
                foreach (var dir in directions)
                {
                    Vector2Int neighborPos = gridPos + dir;
                    if (!gridPositions.ContainsKey(neighborPos))
                    {
                        PlaceRoom(neighbor, neighborPos, depth + 1);
                        break;
                    }
                }
            }
        }


        List<string> allRooms = GetUniqueRooms(atRobbyMap, atBallMap, connectedRooms);
        if (allRooms.Count > 0)
        {
            PlaceRoom(allRooms[0], Vector2Int.zero);
        }

        foreach (var room in allRooms)
        {
            if (!placedRooms.Contains(room))
            {
                for (int x = -10; x <= 10; x++)
                {
                    for (int y = -10; y <= 10; y++)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        if (!gridPositions.ContainsKey(pos))
                        {
                            PlaceRoom(room, pos);
                            break;
                        }
                    }
                    if (placedRooms.Contains(room)) break;
                }
            }
        }

        foreach (var (roomA, roomB) in connectedRooms)
        {
            if (roomObjects.ContainsKey(roomA) && roomObjects.ContainsKey(roomB))
            {
                Vector3 posA = roomObjects[roomA].transform.position;
                Vector3 posB = roomObjects[roomB].transform.position;
                Vector3 dir = (posB - posA).normalized;
                float corridorOffset = roomSize / 2f;
                Vector3 edgeA = posA + dir * corridorOffset;
                Vector3 edgeB = posB - dir * corridorOffset;
                Vector3 corridorCenter = (edgeA + edgeB) / 2f;
                corridorCenter.y += corridorHeightOffset;
                float corridorLength = Vector3.Distance(edgeA, edgeB);
                Quaternion rotation = Quaternion.LookRotation(posB - posA);
                rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
                GameObject corridor = Instantiate(corridorPrefab, corridorCenter, rotation, transform);
                corridor.transform.localScale = new Vector3(0.5f, 1f, corridorLength);
                corridor.name = $"Corridor_{roomA}_{roomB}";
            }
        }

        Dictionary<string, GameObject> robotObjects = new();
        foreach (var kvp in atRobbyMap)
        {
            string robot = kvp.Key;
            string room = kvp.Value;

            if (!roomObjects.ContainsKey(room)) continue;

            Vector3 roomPos = roomObjects[room].transform.position;
            Vector3 robotPos = roomPos + new Vector3(0, 1.5f, 0);
            GameObject robotGO = Instantiate(robotPrefab, robotPos, Quaternion.identity, roomObjects[room].transform);
            robotGO.name = robot;
            robotObjects[robot] = robotGO;

            if (carryMap.ContainsKey(robot))
            {
                string ball = carryMap[robot];
                Vector3 ballPos = robotPos + Vector3.up * 0.5f;
                GameObject ballGO = Instantiate(ballPrefab, ballPos, Quaternion.identity, robotGO.transform);
                ballGO.name = ball;
            }
        }

        foreach (var kvp in atBallMap)
        {
            string ball = kvp.Key;
            string room = kvp.Value;

            if (!roomObjects.ContainsKey(room)) continue;

            Vector3 roomPos = roomObjects[room].transform.position;
            Vector3 ballPos = roomPos + Vector3.up * 0.5f;
            GameObject ballGO = Instantiate(ballPrefab, ballPos, Quaternion.identity, roomObjects[room].transform);
            ballGO.name = ball;
        }
    }

    private List<string> GetUniqueRooms(Dictionary<string, string> atRobby, Dictionary<string, string> atBall, List<(string, string)> connections)
    {
        HashSet<string> rooms = new();
        foreach (var r in atRobby.Values) rooms.Add(r);
        foreach (var r in atBall.Values) rooms.Add(r);
        foreach (var (a, b) in connections)
        {
            rooms.Add(a);
            rooms.Add(b);
        }
        return new List<string>(rooms);
    }
}
