using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DynamicSceneGenerator costruisce in runtime (o in Editor tramite Context‑Menu)
/// una scena composta da stack di negozi con uno o più ascensori posizionati fra
/// le colonne di negozi.
/// 
/// Layout risultante (con N = elevators):
///   ColonnaNegozio 0 | Ascensore 0 | ColonnaNegozio 1 | Ascensore 1 | ... | ColonnaNegozio N
/// 
/// ‑ Ogni colonna contiene "floors" piani, quindi i negozi vengono impilati
///   verticalmente senza sovrapporsi.
/// ‑ Tutti i prefab (ascensore, negozi, persone) vengono specificati via
///   SerializeField nell'Inspector.
/// ‑ La scena generata è figlia dell'oggetto che possiede questo script, così
///   da poterla cancellare/rigenerare facilmente.
/// </summary>

/*
public class ElevatorSceneBuilder : MonoBehaviour
{
    #region Public parameters (Inspector)

    [Header("Layout settings")]
    [SerializeField, Min(1)] private int floors = 3;
    [SerializeField, Min(1)] private int elevators = 1;
    [SerializeField, Min(1)] private int peopleCount = 10;
    [SerializeField, Min(1)] private int elevatorCapacity = 4;

    [Space]

    [Tooltip("Altezza di ogni piano in Unity units")]
    [SerializeField] private float floorHeight = 3f;

    [Tooltip("Larghezza del prefab di un negozio (X axis)")]
    [SerializeField] private float shopWidth = 4f;

    [Tooltip("Larghezza del prefab dell'ascensore (X axis)")]
    [SerializeField] private float elevatorWidth = 2f;
    [SerializeField] private float horizontalPadding = 0.5f;


    [Tooltip("Punto di partenza (angolo in basso a sinistra del piano terra)")]
    [SerializeField] private Vector3 origin = Vector3.zero;


    [Header("Prefabs")]
    [SerializeField] private GameObject elevatorPrefab;
    [SerializeField] private List<GameObject> shopPrefabs = new();
    [SerializeField] private List<GameObject> peoplePrefabs = new();

    #endregion

    #region Internals

    private readonly List<GameObject> spawnedElevators = new();
    private readonly List<GameObject> spawnedShops = new();
    private readonly List<GameObject> spawnedPeople = new();

    #endregion

    #region Unity events

    private void Start()
    {
        // Genera la scena all'avvio in Play‑Mode
        GenerateScene();
    }

#if UNITY_EDITOR
    // Regenera la scena anche in Editor (tasto destro sul componente → Generator/Regenerate Scene)
    [ContextMenu("Generator/Regenerate Scene")]
#endif
    public void GenerateScene()
    {
        ClearPrevious();
        GenerateGrid();
        SpawnPeople();
    }

    #endregion

    #region Scene generation helpers

    private void ClearPrevious()
    {
        foreach (var go in spawnedElevators) if (go) DestroyImmediate(go);
        foreach (var go in spawnedShops) if (go) DestroyImmediate(go);
        foreach (var go in spawnedPeople) if (go) DestroyImmediate(go);

        spawnedElevators.Clear();
        spawnedShops.Clear();
        spawnedPeople.Clear();
    }

    private void GenerateGrid()
    {
        if (elevatorPrefab == null || shopPrefabs.Count == 0)
        {
            Debug.LogError("DynamicSceneGenerator: Prefab mancanti.");
            return;
        }

        int totalColumns = elevators * 2 + 1; // shop, elevator, shop, ...
        float currentX = origin.x;

        for (int columnIndex = 0; columnIndex < totalColumns; columnIndex++)
        {
            bool isElevatorColumn = columnIndex % 2 == 1; // 0=shop,1=elevator,2=shop,3=elevator...

            if (isElevatorColumn)
            {
                Vector3 pos = new Vector3(currentX + elevatorWidth * 0.5f, origin.y, origin.z);
                GameObject elev = Instantiate(elevatorPrefab, pos, Quaternion.identity, transform);

                // Inizializzazione opzionale del controller ascensore
                var controller = elev.GetComponent<ElevatorController>();
                if (controller != null)
                {
                    controller.Initialize(floors, elevatorCapacity);
                }

                spawnedElevators.Add(elev);
                currentX += elevatorWidth + horizontalPadding;
            }
            else // shop column
            {
                for (int floor = 0; floor < floors; floor++)
                {
                    GameObject shopPrefab = shopPrefabs[Random.Range(0, shopPrefabs.Count)];
                    Vector3 pos = new Vector3(currentX + shopWidth * 0.5f, origin.y + floor * floorHeight, origin.z);
                    GameObject shop = Instantiate(shopPrefab, pos, Quaternion.identity, transform);
                    spawnedShops.Add(shop);
                }
                currentX += shopWidth + horizontalPadding;
            }
        }
    }

    private void SpawnPeople()
    {
        if (peoplePrefabs.Count == 0) return;

        for (int i = 0; i < peopleCount; i++)
        {
            GameObject prefab = peoplePrefabs[Random.Range(0, peoplePrefabs.Count)];
            Vector3 offset = new Vector3(Random.Range(-shopWidth, shopWidth), 0f, Random.Range(-shopWidth, shopWidth));
            GameObject person = Instantiate(prefab, origin + offset, Quaternion.identity, transform);

            // Target casuale per demo: un piano qualsiasi sopra il piano terra
            var personCtrl = person.GetComponent<PersonController>();
            if (personCtrl != null)
            {
                personCtrl.SetTargetFloor(Random.Range(0, floors));
            }

            spawnedPeople.Add(person);
        }
    }

    #endregion

    #region Gizmos (Editor)

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        int totalColumns = elevators * 2 + 1;
        float currentX = origin.x;

        for (int columnIndex = 0; columnIndex < totalColumns; columnIndex++)
        {
            bool isElevatorColumn = columnIndex % 2 == 1;
            float width = isElevatorColumn ? elevatorWidth : shopWidth;
            Gizmos.DrawWireCube(new Vector3(currentX + width * 0.5f, origin.y + (floors - 1) * floorHeight * 0.5f, origin.z),
                                new Vector3(width, floors * floorHeight, 0.5f));
            currentX += width + horizontalPadding;
        }
    }
#endif

    #endregion
}


*/