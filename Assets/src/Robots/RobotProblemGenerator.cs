
using System.Collections.Generic;
using UnityEngine;

public class RobotProblemGenerator : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject corridorPrefab;

    [SerializeField] private float corridorHeightOffset = 0.01f;
    [SerializeField] private float roomSpacing = 6f;
    [SerializeField] private float roomSize = 4f;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        var planInfo = PlanInfo.GetInstance();

        // 🔹 1. Costruire le mappe dai predicati
        Dictionary<string, List<string>> objectMap = new Dictionary<string, List<string>>();
        Dictionary<string, string> atRobbyMap = new Dictionary<string, string>();  // robot → room
        Dictionary<string, string> atBallMap = new Dictionary<string, string>();   // ball → room
        Dictionary<string, string> carryMap = new Dictionary<string, string>();    // robot → ball
        HashSet<string> freeRobots = new HashSet<string>();
        Dictionary<string, List<string>> allowedMap = new Dictionary<string, List<string>>();  // room → rooms allowed
        List<(string, string)> connectedRooms = new List<(string, string)>();     // (roomA, roomB)

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

        // 🔹 2. Classificare gli oggetti per tipo
        foreach (var o in planInfo.GetObjects())
        {
            string key = "";

            if (o.name.StartsWith("room")) key = "rooms";
            else if (o.name.StartsWith("robot")) key = "robots";
            else if (o.name.StartsWith("ball")) key = "balls";

            if (!string.IsNullOrEmpty(key))
            {
                if (!objectMap.ContainsKey(key))
                    objectMap[key] = new List<string>();

                objectMap[key].Add(o.name);
            }
        }

        // 🔹 3. Crea le stanze con posizionamento intelligente
        Dictionary<string, GameObject> roomObjects = new Dictionary<string, GameObject>();
        HashSet<string> placedRooms = new HashSet<string>();
        Dictionary<string, List<string>> roomAdjacency = new Dictionary<string, List<string>>();

        // Costruisci la mappa di adiacenza delle stanze
        foreach (var (a, b) in connectedRooms)
        {
            if (!roomAdjacency.ContainsKey(a)) roomAdjacency[a] = new List<string>();
            if (!roomAdjacency.ContainsKey(b)) roomAdjacency[b] = new List<string>();
            roomAdjacency[a].Add(b);
            roomAdjacency[b].Add(a);
        }

        // Lista di posizioni già occupate
        Dictionary<Vector2Int, string> gridPositions = new Dictionary<Vector2Int, string>();

        void PlaceRoom(string roomName, Vector2Int gridPos, int depth = 0)
        {
            if (placedRooms.Contains(roomName)) return;
            if (gridPositions.ContainsKey(gridPos)) return;

            // Calcola posizione mondo dalla griglia
            Vector3 worldPos = new Vector3(gridPos.x * roomSpacing, 0, gridPos.y * roomSpacing);

            // Crea la stanza
            GameObject roomGO = Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);
            roomGO.name = roomName;
            roomObjects[roomName] = roomGO;
            placedRooms.Add(roomName);
            gridPositions[gridPos] = roomName;

            // Se non ha vicini, termina
            if (!roomAdjacency.ContainsKey(roomName)) return;

            // Direzioni possibili (nord, est, sud, ovest)
            Vector2Int[] directions = new[]
            {
                Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
            };

            // Collega le stanze vicine ricorsivamente
            foreach (var neighbor in roomAdjacency[roomName])
            {
                if (placedRooms.Contains(neighbor)) continue;

                // Cerca una direzione libera
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

        // Inizia con il posizionamento dalla prima stanza
        List<string> allRooms = GetUniqueRooms(atRobbyMap, atBallMap, connectedRooms);
        if (allRooms.Count > 0)
        {
            PlaceRoom(allRooms[0], Vector2Int.zero);
        }

        // Verifica se ci sono stanze non posizionate
        foreach (var room in allRooms)
        {
            if (!placedRooms.Contains(room))
            {
                // Cerca di trovare una posizione libera
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

        // 🔹 4. Crea i corridoi tra le stanze connesse
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

                float corridorLength = Vector3.Distance(edgeA, edgeB);

                Quaternion rotation = Quaternion.LookRotation(posB - posA);
                rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0); // solo asse Y

                GameObject corridor = Instantiate(corridorPrefab, corridorCenter, rotation, transform);
                corridor.transform.localScale = new Vector3(1f, 1f, corridorLength);
                corridor.name = $"Corridor_{roomA}_{roomB}";
            }
        }

        // 🔹 5. Crea i robot nelle rispettive stanze
        Dictionary<string, GameObject> robotObjects = new Dictionary<string, GameObject>();

        foreach (var kvp in atRobbyMap)
        {
            string robot = kvp.Key;
            string room = kvp.Value;

            if (!roomObjects.ContainsKey(room)) continue;

            Vector3 roomPos = roomObjects[room].transform.position;
            Vector3 robotPos = roomPos + Vector3.up;

            GameObject robotGO = Instantiate(robotPrefab, robotPos, Quaternion.identity, roomObjects[room].transform);
            robotGO.name = robot;
            robotObjects[robot] = robotGO;

            // Se il robot trasporta una palla
            if (carryMap.ContainsKey(robot))
            {
                string ball = carryMap[robot];
                Vector3 ballPos = robotPos + Vector3.up * 0.5f;

                GameObject ballGO = Instantiate(ballPrefab, ballPos, Quaternion.identity, robotGO.transform);
                ballGO.name = ball;
            }
        }

        // 🔹 6. Crea le palle nelle rispettive stanze
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
        HashSet<string> rooms = new HashSet<string>();
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