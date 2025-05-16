/* CorridorSceneGenerator.cs
 * genera corridoio + stanze + oggetti in modo parametrico
 */
using UnityEngine;
using System.Collections.Generic;          // <-- per la lista di stanze
#if UNITY_EDITOR
using UnityEditor;                         // <-- per misurare i prefab e usare Undo
#endif

[ExecuteAlways]
public class CorridorSceneGenerator : MonoBehaviour
{
    /* ---------- Prefab ---------- */
    [Header("Prefabs")]
    [Tooltip("Elenco dei prefab stanza da usare in ordine ciclico")]
    [SerializeField] private List<GameObject> roomPrefabs = new();   // ← lista, non più singolo prefab
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private GameObject ballPrefab;

    /* ---------- Quantità ---------- */
    [Header("Quantità")]
    [Min(1)] [SerializeField] private int numberOfRooms  = 4;   // totali (pari = uguali per lato)
    [Min(0)] [SerializeField] private int numberOfRobots = 2;
    [Min(0)] [SerializeField] private int numberOfBalls  = 4;

    /* ---------- Layout ---------- */
    [Header("Layout")]
    [Tooltip("Spazio extra (Z) fra una stanza e la successiva")]
    [SerializeField] private float spacingBetweenRooms = .5f;
    [Tooltip("Eventuale aria fra stanza e corridoio (0 = combacia perfettamente)")]
    [SerializeField] private float lateralGap = 0f;
    [SerializeField] private float yOffset = 0f;
    [SerializeField] private bool generateOnStart = true;

    /* ---------- Cache delle stanze generate ---------- */
    private readonly List<GameObject> _spawnedRooms = new();

    /* ---------- Entry points ---------- */
    private void Start()
    {
        if (Application.isPlaying && generateOnStart)
            Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        ClearChildren();

        if (roomPrefabs is null || roomPrefabs.Count == 0)
        {
            Debug.LogWarning("Nessun prefab di stanza assegnato.");
            return;
        }

        /* ===== 1. Calcola ingombri ===== */
        Vector3 firstRoomSize = GetPrefabSize(roomPrefabs[0]);   // usiamo la prima come riferimento
        Vector3 corridorSize  = GetPrefabSize(corridorPrefab);

        int    roomsPerSide   = Mathf.CeilToInt(numberOfRooms / 2f);

        /* ===== 2. Genera i segmenti del corridoio ===== */
        float corridorTotalLen = roomsPerSide * corridorSize.z;              // un pezzo per stanza
        float corridorStartZ   = -corridorTotalLen * .5f + corridorSize.z*.5f;

        for (int i = 0; i < roomsPerSide; i++)
        {
            Vector3 pos = new Vector3(0, yOffset, corridorStartZ + i * corridorSize.z);
            GameObject piece = Instantiate(corridorPrefab, pos, Quaternion.identity, transform);
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(piece, "Generate Corridor Piece");
#endif
        }

        float corridorHalfWidth = corridorSize.x * .5f;                       // metà larghezza reale

        /* ===== 3. Genera le stanze ===== */
        float totalRowLen = roomsPerSide * firstRoomSize.z +
                            (roomsPerSide - 1) * spacingBetweenRooms;
        float roomStartZ  = -totalRowLen * .5f + firstRoomSize.z * .5f;

        for (int i = 0; i < numberOfRooms; i++)
        {
            bool  leftSide = (i % 2 == 0);                   // pari = sinistra, dispari = destra
            int   index    =  i / 2;                        // posizione lungo il corridoio
            float zPos     = roomStartZ + index * (firstRoomSize.z + spacingBetweenRooms);

            float roomDepth = firstRoomSize.z;              // lato che “tocca” il corridoio
            float xPos      = corridorHalfWidth + lateralGap + roomDepth * .5f;

            Vector3    pos      = new(leftSide ? -xPos : xPos, yOffset, zPos);
            Quaternion rot      = leftSide ? Quaternion.Euler(0, -90, 0)
                                           : Quaternion.Euler(0,  90, 0);

            GameObject prefab = roomPrefabs[i % roomPrefabs.Count];
            GameObject room   = Instantiate(prefab, pos, rot, transform);
            _spawnedRooms.Add(room);

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(room, "Generate Room");
#endif
        }

        /* ===== 4. Popola le stanze con robot e palline ===== */
        PlaceObjectsInsideRooms(robotPrefab, numberOfRobots);
        PlaceObjectsInsideRooms(ballPrefab,  numberOfBalls);
    }

    /* ====================================================================== */
    /* ===========================   HELPERS   ============================== */
    /* ====================================================================== */

    /// <summary>Posiziona 'count' oggetti all'interno delle stanze (localPosition = (1,1,1)).</summary>
    private void PlaceObjectsInsideRooms(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0 || _spawnedRooms.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            GameObject room = _spawnedRooms[i % _spawnedRooms.Count];

            // posizione locale (1,1,1) → convertita in mondo, poi parentata alla stanza
            Vector3 worldPos = room.transform.TransformPoint(new Vector3(1f, 1f, 1f));
            Instantiate(prefab, worldPos, Quaternion.identity, room.transform);
        }
    }

    /// <summary>Rimuove tutto ciò che era stato generato in precedenza.</summary>
    private void ClearChildren()
    {
#if UNITY_EDITOR
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
#else
        foreach (Transform ch in transform)
            Destroy(ch.gameObject);
#endif
        _spawnedRooms.Clear();
    }

    /// <summary>Restituisce le dimensioni AABB del prefab.</summary>
    private Vector3 GetPrefabSize(GameObject prefab)
    {
        if (prefab == null) return Vector3.one;

#if UNITY_EDITOR
        GameObject temp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Vector3 size     = CalculateBounds(temp).size;
        DestroyImmediate(temp);
        return size;
#else
        return CalculateBounds(prefab).size;
#endif
    }

    private static Bounds CalculateBounds(GameObject go)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return new Bounds(go.transform.position, Vector3.one);

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b;
    }
}
