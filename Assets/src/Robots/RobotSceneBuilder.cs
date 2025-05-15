// PDDLSceneBuilder.cs
// ------------------------------------------------------------
// A Unity C# MonoBehaviour that generates a 3-D scene of rooms
// facing a central corridor, and populates them with objects
// (robots, balls, grippers). All settings and object placements
// are taken from the Inspector (Unity), no PDDL parsing.
//
// CONFIGURATION (Inspector):
//  • roomCount: total number of rooms (even recommended)
//  • lateralSpacing: distance between adjacent rooms on same side (X axis)
//  • frontalSpacing: distance from corridor center to room centers (Z axis)
//  • robotRoomIndices: list of room indices for each Robot (at-robby)
//  • ballRoomIndices: list of room indices for each Ball (at-ball)
//  • gripperCount: number of Gripper instances (distributed evenly)
//  • roomPrefabs: list of possible room prefabs (randomized)
//  • corridorPrefab: prefab for corridor segments
//  • robotPrefab, ballPrefab, gripperPrefab: object prefabs
//  • roomOrigin: base origin for layout
//
// BEHAVIOR:
//  - Rooms on left side (indices [0, floor(roomCount/2))) are placed at z=-frontalSpacing, rotation Y=0
//  - Rooms on right side (indices [floor(roomCount/2), roomCount)) are at z=+frontalSpacing, rotation Y=180
//  - Corridor segments count = ceil(roomCount/2), placed along X at z=0
//  - Robots/Balls: placed in rooms by index lists, at Y=0.16
//  - Grippers: distributed evenly across rooms, at Y=0.5
// ------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

public class PDDLSceneBuilder : MonoBehaviour
{
    [Header("Room & Corridor Settings")]

    [Tooltip("Total number of rooms (even recommended)")]
    [SerializeField] private int roomCount = 4;

    [Tooltip("Spacing between adjacent rooms on same side along X axis")]
    [SerializeField] private float lateralSpacing = 8f;

    [Tooltip("Distance from corridor center to room centers on Z axis")]
    [SerializeField] private float frontalSpacing = 12f;

    [Header("Object Placement via Predicates")]
    [Tooltip("Room index for each Robot (at-robby)")]
    [SerializeField] private List<int> robotRoomIndices = new List<int>();
    [Tooltip("Room index for each Ball (at-ball)")]
    [SerializeField] private List<int> ballRoomIndices = new List<int>();
    [Tooltip("Number of Gripper instances to spawn (distributed evenly)")]
    [SerializeField] private int gripperCount = 2;

    [Header("Prefabs (assign in Inspector)")]
    [Tooltip("Possible room prefabs – one chosen at random per room")]
    [SerializeField] private List<GameObject> roomPrefabs = new List<GameObject>();
    [Tooltip("Prefab for corridor segments")]
    [SerializeField] private GameObject corridorPrefab;
    [Tooltip("Robot prefab")]
    [SerializeField] private GameObject robotPrefab;
    [Tooltip("Ball prefab")]
    [SerializeField] private GameObject ballPrefab;
    [Tooltip("Gripper prefab")]
    [SerializeField] private GameObject gripperPrefab;

    [Header("Layout Origin")]
    [Tooltip("Base origin for layout")]
    [SerializeField] private Vector3 roomOrigin = Vector3.zero;

    // Internal list of room transforms
    private readonly List<Transform> rooms = new List<Transform>();

    private void Start()
    {
        BuildScene();
    }

    private void BuildScene()
    {
        InstantiateRooms();
        InstantiateCorridor();
        InstantiateRobots();
        InstantiateBalls();
        InstantiateGrippers();
    }

    private void InstantiateRooms()
    {
        rooms.Clear();
        int half = roomCount / 2;

        // Left side rooms (rotation 0°)
        for (int i = 0; i < half; i++)
        {
            SpawnRoom(i, -frontalSpacing, Quaternion.identity);
        }
        // Right side rooms (rotation 180°)
        for (int i = half; i < roomCount; i++)
        {
            SpawnRoom(i, frontalSpacing, Quaternion.Euler(0f, 180f, 0f));
        }
    }

    private void SpawnRoom(int index, float zOffset, Quaternion rotation)
    {
        if (roomPrefabs.Count == 0)
        {
            Debug.LogWarning("No room prefab assigned for room " + index);
            return;
        }
        GameObject prefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];

        int half = roomCount / 2;
        int sideIndex = (zOffset < 0f) ? index : index - half;
        float x = roomOrigin.x + sideIndex * lateralSpacing;
        float y = roomOrigin.y;
        float z = roomOrigin.z + zOffset;

        Transform parent = transform;
        GameObject room = Instantiate(prefab, new Vector3(x, y, z), rotation, parent);
        room.name = $"Room_{index}";
        rooms.Add(room.transform);
    }

    private void InstantiateCorridor()
    {
        if (corridorPrefab == null) return;
        int segCount = Mathf.CeilToInt(roomCount / 2f);
        for (int i = 0; i < segCount; i++)
        {
            float x = roomOrigin.x + i * lateralSpacing;
            float y = roomOrigin.y;
            float z = roomOrigin.z;
            GameObject cor = Instantiate(corridorPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
            cor.name = $"Corridor_{i}";
        }
    }

    private void InstantiateRobots()
    {
        if (robotPrefab == null) return;
        for (int i = 0; i < robotRoomIndices.Count; i++)
        {
            int idx = Mathf.Clamp(robotRoomIndices[i], 0, rooms.Count - 1);
            Transform roomT = rooms[idx];
            Vector3 spawnPos = roomT.position + new Vector3(0f, 0.16f, 0f);
            GameObject obj = Instantiate(robotPrefab, spawnPos, Quaternion.identity, roomT);
            obj.name = $"Robot_{i}";
        }
    }

    private void InstantiateBalls()
    {
        if (ballPrefab == null) return;
        for (int i = 0; i < ballRoomIndices.Count; i++)
        {
            int idx = Mathf.Clamp(ballRoomIndices[i], 0, rooms.Count - 1);
            Transform roomT = rooms[idx];
            Vector3 spawnPos = roomT.position + new Vector3(0f, 0.16f, 0f);
            GameObject obj = Instantiate(ballPrefab, spawnPos, Quaternion.identity, roomT);
            obj.name = $"Ball_{i}";
        }
    }

    private void InstantiateGrippers()
    {
        if (gripperPrefab == null) return;
        int total = rooms.Count;
        for (int i = 0; i < gripperCount; i++)
        {
            int idx = total > 0 ? i % total : 0;
            Transform roomT = total > 0 ? rooms[idx] : transform;
            Vector3 spawnPos = roomT.position + new Vector3(0f, 0.5f, 0f);
            GameObject obj = Instantiate(gripperPrefab, spawnPos, Quaternion.identity, roomT);
            obj.name = $"Gripper_{i}";
        }
    }
}
