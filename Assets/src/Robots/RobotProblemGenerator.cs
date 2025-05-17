using System.Collections.Generic;
using UnityEngine;

public class RobotProblemGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject corridorPrefab;

    [SerializeField] private float corridorHeightOffset = 0.01f;
    [SerializeField] private float corridorWidth = 1.5f;
    [SerializeField] private float roomSpacing = 6f;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        var planInfo = PlanInfo.GetInstance();

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

        Dictionary<string, GameObject> roomObjects = new();
        List<string> roomNames = new List<string>(GetUniqueRooms(atRobbyMap, atBallMap, connectedRooms));
        Dictionary<string, Vector2Int> roomGridPositions = AssignGridPositions(roomNames, connectedRooms);

        foreach (var kvp in roomGridPositions)
        {
            string room = kvp.Key;
            Vector2Int gridPos = kvp.Value;
            Vector3 worldPos = new Vector3(gridPos.x * roomSpacing, 0, gridPos.y * roomSpacing);
            GameObject roomGO = Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);
            roomGO.name = room;
            roomObjects[room] = roomGO;
        }

        foreach (var kvp in atRobbyMap)
        {
            string robot = kvp.Key;
            string room = kvp.Value;

            if (!roomObjects.ContainsKey(room)) continue;

            GameObject robotGO = Instantiate(robotPrefab, roomObjects[room].transform.position + Vector3.up, Quaternion.identity, roomObjects[room].transform);
            robotGO.name = robot;

            if (carryMap.ContainsKey(robot))
            {
                string ball = carryMap[robot];
                GameObject ballGO = Instantiate(ballPrefab, robotGO.transform.position + Vector3.up * 0.5f, Quaternion.identity, robotGO.transform);
                ballGO.name = ball;
            }
        }

        foreach (var kvp in atBallMap)
        {
            string ball = kvp.Key;
            string room = kvp.Value;

            if (!roomObjects.ContainsKey(room)) continue;

            GameObject ballGO = Instantiate(ballPrefab, roomObjects[room].transform.position + Vector3.up * 0.5f, Quaternion.identity, roomObjects[room].transform);
            ballGO.name = ball;
        }

        float roomSize = 4f; // Lato della stanza (assunto quadrato)
        float corridorLength = roomSpacing - roomSize;

        foreach (var (roomA, roomB) in connectedRooms)
        {
            if (roomObjects.ContainsKey(roomA) && roomObjects.ContainsKey(roomB))
            {
                Vector3 posA = roomObjects[roomA].transform.position;
                Vector3 posB = roomObjects[roomB].transform.position;

                Vector3 dir = (posB - posA).normalized;

                // Calcola i bordi delle due stanze (centro ± metà stanza)
                Vector3 edgeA = posA + dir * (roomSize / 2f);
                Vector3 edgeB = posB - dir * (roomSize / 2f);

                Vector3 corridorCenter = (edgeA + edgeB) / 2f;
                corridorCenter.y += corridorHeightOffset;

                float corridorLengthReal = Vector3.Distance(edgeA, edgeB);

                Quaternion rotation = Quaternion.LookRotation(posB - posA);
                rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0); // solo asse Y

                GameObject corridor = Instantiate(corridorPrefab, corridorCenter, rotation, transform);
                corridor.transform.localScale = new Vector3(1f, 1f, 1f);
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

    // Aggiunta: assegna posizioni griglia (rudimentale)
    private Dictionary<string, Vector2Int> AssignGridPositions(List<string> rooms, List<(string, string)> connections)
    {
        Dictionary<string, Vector2Int> grid = new();
        Queue<(string, Vector2Int)> toVisit = new();
        HashSet<string> visited = new();

        if (rooms.Count == 0) return grid;

        string startRoom = rooms[0];
        grid[startRoom] = Vector2Int.zero;
        toVisit.Enqueue((startRoom, Vector2Int.zero));
        visited.Add(startRoom);

        while (toVisit.Count > 0)
        {
            var (room, pos) = toVisit.Dequeue();

            foreach (var (a, b) in connections)
            {
                string neighbor = null;
                if (a == room && !visited.Contains(b)) neighbor = b;
                else if (b == room && !visited.Contains(a)) neighbor = a;

                if (neighbor != null)
                {
                    Vector2Int newPos = GetNextFreeNeighborPosition(grid, pos);
                    grid[neighbor] = newPos;
                    toVisit.Enqueue((neighbor, newPos));
                    visited.Add(neighbor);
                }
            }
        }

        return grid;
    }

    private Vector2Int GetNextFreeNeighborPosition(Dictionary<string, Vector2Int> grid, Vector2Int origin)
    {
        Vector2Int[] directions = new[]
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        foreach (var dir in directions)
        {
            Vector2Int candidate = origin + dir;
            if (!grid.ContainsValue(candidate))
                return candidate;
        }

        return origin + Vector2Int.one; // fallback
    }
}
