using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;           // serve solo a leggere l’ingombro dei prefab in Editor
#endif

[ExecuteAlways]              // così puoi vederlo anche in modalità Edit
public class CorridorSceneGenerator : MonoBehaviour
{
    /* ---------- Prefab da assegnare ---------- */
    [Header("Prefabs")]
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private GameObject ballPrefab;

    /* ---------- Quantità ---------- */
    [Header("Quantità")]
    [Min(1)]  [SerializeField] private int numberOfRooms  = 4;   // stanze totali (pari → stesse per lato)
    [Min(0)]  [SerializeField] private int numberOfRobots = 2;
    [Min(0)]  [SerializeField] private int numberOfBalls  = 4;

    /* ---------- Parametri di layout ---------- */
    [Header("Layout")]
    [SerializeField] private float spacingBetweenRooms = 0f;     // spazio extra fra le stanze lungo Z
    [SerializeField] private bool  generateOnStart     = true;   // rigenera in Start
    [SerializeField] private float yOffset             = 0f;     // alza o abbassa tutto il piano

    /* ---------- Entry points ---------- */
    private void Start()
    {
        if (Application.isPlaying && generateOnStart)
            Generate();
    }

    /// <summary>Richiamabile dal menù contestuale per rigenerare la scena in qualsiasi momento.</summary>
    [ContextMenu("Generate")]
    public void Generate()
    {
        /* 1. Pulisce tutto ciò che era già stato generato */
#if UNITY_EDITOR
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
#else
        foreach (Transform ch in transform)
            Destroy(ch.gameObject);
#endif

        /* 2. Calcola le dimensioni reali dei prefab */
        Vector3 roomSize     = GetPrefabSize(roomPrefab);      // larghezza = X, profondità = Z
        Vector3 corridorSize = GetPrefabSize(corridorPrefab);  // useremo la X come larghezza corridoio

        int roomsPerSide   = Mathf.CeilToInt(numberOfRooms / 2f);
        float corridorLen  = roomsPerSide * roomSize.z + (roomsPerSide - 1) * spacingBetweenRooms;
        float halfCorridor = corridorSize.x * 0.5f;

        /* 3. Istanzia il corridoio e lo scala in lunghezza */
        GameObject corridor = Instantiate(corridorPrefab, transform);
        corridor.transform.localPosition = new Vector3(0, yOffset, 0);
        Vector3 cScale = corridor.transform.localScale;
        cScale.z = corridorLen / corridorSize.z;               // lo stiriamo lungo Z
        corridor.transform.localScale = cScale;

        /* 4. Posiziona le stanze sui due lati, orientate verso il corridoio */
        float startZ = -corridorLen + roomSize.z * 0.5f;

        for (int i = 0; i < numberOfRooms; i++)
        {
            bool  leftSide = (i % 2 == 0);                 // pari a sinistra, dispari a destra
            int   index    =  i / 2;                       // posizione lungo il corridoio
            float zPos     = startZ + index * (roomSize.z + spacingBetweenRooms);
            float xPos     = halfCorridor + roomSize.x * 0.5f;

            Vector3    pos      = new Vector3(leftSide ? -xPos : xPos, yOffset, zPos);
            Quaternion rotation = leftSide ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);

            Instantiate(roomPrefab, pos, rotation, transform);
        }

        /* 5. Popola il corridoio con robot e palline */
        SpawnInsideCorridor(robotPrefab, numberOfRobots, corridorSize.x, corridorLen, yOffset);
        SpawnInsideCorridor(ballPrefab,  numberOfBalls,  corridorSize.x, corridorLen, yOffset + 0.25f);
    }

    /* ---------- Helpers ---------- */

    /// <summary>Inserisce 'count' istanze del prefab in posizioni casuali dentro al corridoio.</summary>
    private void SpawnInsideCorridor(GameObject prefab, int count, float corridorWidth, float corridorLen, float y)
    {
        if (prefab == null || count <= 0) return;

        float halfW = corridorWidth * 0.5f - 0.25f;   // margine di sicurezza
        float halfL = corridorLen    * 0.5f - 0.25f;

        for (int i = 0; i < count; i++)
        {
            Vector3 p = new Vector3(Random.Range(-halfW, halfW), y, Random.Range(-halfL, halfL));
            Instantiate(prefab, p, Quaternion.identity, transform);
        }
    }

    /// <summary>Restituisce le dimensioni reali (AABB) del prefab.</summary>
    private Vector3 GetPrefabSize(GameObject prefab)
    {
        if (prefab == null) return Vector3.one;

#if UNITY_EDITOR
        // Creiamo un'istanza temporanea per misurarla (solo in Editor)
        GameObject temp = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Vector3 size = CalculateBounds(temp).size;
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
