using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels & Texts")]
    [SerializeField] private GameObject planPanel;
    [SerializeField] private TextMeshProUGUI focusText;
    [SerializeField] private TextMeshProUGUI planText;

    [Header("Camera Focus")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float focusDistance = 5f;      // distanza “orizzontale” dal target, usata solo per offset iniziale
    [SerializeField] private float verticalOffset = 1.5f;    // altezza aggiuntiva a cui la camera guarda il target

    private List<string> focusObjects = new List<string>();
    private int currentFocus = 0;
    private Dictionary<string, GameObject> objectMap;

    // Transform dell'oggetto che la camera sta seguendo
    private Transform focusedTransform = null;

    // Offset fisso tra la posizione della camera e la posizione target (target + verticalOffset)
    private Vector3 followOffset = Vector3.zero;

    void Awake()
    {
        var planInfo = PlanInfo.GetInstance();
        int problemType = (int)planInfo.GetDomainType(); // 0 = logistic, 1 = robot, 2 = elevator

        if (problemType == 1) // ROBOT
        {
            // Nel dominio robot prendiamo solo i robot (ignoro goal e stanze)
            focusObjects = planInfo.GetObjects()
                .Where(o => o.name.StartsWith("robot") || o.name.StartsWith("robby"))
                .Select(o => o.name)
                .OrderBy(n => n)
                .ToList();
        }
        else // LOGISTIC oppure altri domini basati sui goal (es. elevator)
        {
            // Prendo i valori da tutti i predicati di tipo “goal”
            // GetGoals() restituisce una lista di oggetti (predicati) che hanno la proprietà .values
            // Se restituisce null, uso lista vuota.
            var goals = planInfo.GetGoals() ?? new List<GoalToAdd>();
            focusObjects = goals
                .SelectMany(g => g.values)
                .Distinct()
                .ToList();
        }

        Debug.Log($"[UIManager] focusObjects iniziali: {string.Join(", ", focusObjects)}");
    }

    void Start()
    {
        // Non chiamiamo BuildObjectMap() subito: aspettiamo un frame
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        // Attendo un frame: in questo modo RobotProblemGenerator.Start() (o equivalente)
        // ha già instanziato tutti i prefab (stanze, robot, veicoli, ecc.)
        yield return null;

        // Ora la scena è popolata: posso costruire la mappa di tutti gli oggetti
        BuildObjectMap();

        // Se siamo nel dominio LOGISTIC, filtriamo i goal
        var planInfo = PlanInfo.GetInstance();
        int problemType = (int)planInfo.GetDomainType();

        if (problemType == 0) // LOGISTIC
        {
            // Teniamo solo i nomi (dei goal) trovati in objectMap
            focusObjects = focusObjects
                .Where(name => objectMap.ContainsKey(name))
                .ToList();

            // Se non rimane nulla (i goal non corrispondono a GameObject in scena),
            // facciamo “fallback” su tutti gli oggetti mappati in objectMap
            if (focusObjects.Count == 0)
            {
                focusObjects = objectMap.Keys.OrderBy(n => n).ToList();
            }
        }

        // Se c'è almeno un oggetto da focalizzare, iniziamo dal primo
        if (focusObjects.Count > 0)
            ApplyFocus();
    }

    private void BuildObjectMap()
    {
        objectMap = new Dictionary<string, GameObject>();
        var allObjs = PlanInfo.GetInstance().GetObjects();

        Debug.Log($"[UIManager] PlanInfo.GetObjects() conta {allObjs?.Count ?? 0} elementi");
        if (allObjs == null) return;

        // Prendo tutti i Transform attivi nella scena
        var allTransforms = FindObjectsOfType<Transform>();

        foreach (var obj in allObjs)
        {
            // Se è un “floor” (nel dominio elevator/logistic) salto
            if (obj.type.StartsWith("floor"))
                continue;

            string targetName = obj.name.ToLower();

            // Cerco un Transform in scena che corrisponda (ignorando "(Clone)" e case)
            var match = allTransforms.FirstOrDefault(t =>
            {
                string sceneName = t.name.ToLower().Replace("(clone)", "").Trim();
                return sceneName == targetName || sceneName.StartsWith(targetName);
            });

            if (match != null)
            {
                Debug.Log($"[UIManager] ✔ Trovato in scena: '{match.name}' per '{obj.name}'");
                objectMap[obj.name] = match.gameObject;
            }
            else
            {
                Debug.LogWarning($"[UIManager] ❌ Nessun GameObject trovato per '{obj.name}'");
            }
        }

        Debug.Log($"[UIManager] Mappature trovate ({objectMap.Count}):");
        foreach (var kvp in objectMap)
        {
            Debug.Log($"  '{kvp.Key}' -> '{kvp.Value.name}' (pos: {kvp.Value.transform.position})");
        }
    }

    public void ShowPlan()
    {
        planPanel.SetActive(!planPanel.activeSelf);
        if (!planPanel.activeSelf) return;

        var planPath = Path.Combine(Application.dataPath, "PDDL", "output_plan.txt");
        if (!File.Exists(planPath))
        {
            planText.text = "Error: couldn't load plan.";
            return;
        }

        var lines = File.ReadAllLines(planPath);
        planText.text = string.Join("\n",
            lines
            .SkipWhile(l => !l.StartsWith("Found Plan:"))
            .Skip(1)
            .TakeWhile(l =>
                !string.IsNullOrWhiteSpace(l) &&
                !l.StartsWith("Plan-Length") &&
                !l.StartsWith("Metric") &&
                !l.StartsWith("Planning Time"))
            .Select(l => l.Trim())
        );
    }

    public void NextFocus()
    {
        if (focusObjects.Count == 0) return;
        currentFocus = (currentFocus + 1) % focusObjects.Count;
        ApplyFocus();
    }

    public void PrevFocus()
    {
        if (focusObjects.Count == 0) return;
        currentFocus = (currentFocus - 1 + focusObjects.Count) % focusObjects.Count;
        ApplyFocus();
    }

    private void ApplyFocus()
    {
        if (focusObjects.Count == 0)
        {
            // Non c'è nulla da seguire
            focusedTransform = null;
            return;
        }

        string name = focusObjects[currentFocus];
        focusText.text = $"{name} ({currentFocus + 1}/{focusObjects.Count})";

        if (objectMap != null && objectMap.TryGetValue(name, out var go))
        {
            Debug.Log($"[UIManager] Applicando focus su '{name}' at position {go.transform.position}");

            // Salvo il Transform da seguire
            focusedTransform = go.transform;

            // Calcolo l'offset iniziale (con verticalOffset) tra camera e target
            Vector3 targetWithOffset = focusedTransform.position + Vector3.up * verticalOffset;
            followOffset = mainCamera.transform.position - targetWithOffset;

            // Teletrasporto istantaneo per evitare scatti della camera
            mainCamera.transform.position = targetWithOffset + followOffset;
            mainCamera.transform.LookAt(targetWithOffset);
        }
        else
        {
            Debug.LogWarning($"[UIManager] Nessuna mappatura per '{name}'. Oggetti disponibili: {string.Join(", ", objectMap.Keys)}");
            focusedTransform = null;
        }
    }

    private void LateUpdate()
    {
        if (focusedTransform == null)
            return;

        // Ricalcolo la posizione target (aggiungendo verticalOffset)
        Vector3 targetWithOffset = focusedTransform.position + Vector3.up * verticalOffset;

        // La camera deve stare sempre in targetWithOffset + followOffset
        Vector3 desiredPos = targetWithOffset + followOffset;

        // Se vuoi un follow istantaneo, usa:
        //    mainCamera.transform.position = desiredPos;
        // Se invece vuoi un movimento smussato, mantieni il Lerp:
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            desiredPos,
            Time.deltaTime * 5f  // regola questo valore per maggiore/minore “morbidezza”
        );

        // Faccio in modo che la camera guardi sempre il punto targetWithOffset
        mainCamera.transform.LookAt(targetWithOffset);
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }
}
