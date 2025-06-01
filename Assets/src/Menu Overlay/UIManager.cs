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
    [SerializeField] private float focusDistance = 5f;      // distanza “orizzontale” dal target, se la vuoi usare
    [SerializeField] private float verticalOffset = 1.5f;    // altezza aggiuntiva in cui guardare il target

    private List<string> focusObjects = new List<string>();
    private int currentFocus = 0;
    private Dictionary<string, GameObject> objectMap;

    // Il Transform dell'oggetto che la camera deve seguire
    private Transform focusedTransform = null;

    // L'offset fisso tra camera e target (vettore)
    private Vector3 followOffset = Vector3.zero;

    void Awake()
    {
        var planInfo = PlanInfo.GetInstance();
        int problemType = (int)planInfo.GetDomainType(); // 0 = logistic, 1 = robot, 2 = elevator

        if (problemType == 1) // ROBOT
        {
            // Prendo solo i robot (senza usare i goal)
            focusObjects = planInfo.GetObjects()
                .Where(o => o.name.StartsWith("robot") || o.name.StartsWith("robby"))
                .Select(o => o.name)
                .OrderBy(n => n)
                .ToList();
        }
        else
        {
            // Per altri domini, usa i goal come primo criterio
            var goals = planInfo.GetGoals();
            focusObjects = goals != null
                ? goals.SelectMany(g => g.values).Distinct().ToList()
                : new List<string>();
        }

        Debug.Log($"[UIManager] focusObjects ({focusObjects.Count}): {string.Join(", ", focusObjects)}");
    }

    void Start()
    {
        // NON chiamiamo BuildObjectMap() direttamente, ma aspettiamo un frame
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        // Aspetta la fine del frame: in quel momento RobotProblemGenerator.Start() avrà già creato stanze/robot/palle
        yield return null;

        BuildObjectMap();

        if (focusObjects.Count > 0)
            ApplyFocus();
    }

    void BuildObjectMap()
    {
        objectMap = new Dictionary<string, GameObject>();
        var allObjs = PlanInfo.GetInstance().GetObjects();

        Debug.Log($"[UIManager] PlanInfo.GetObjects() conta {allObjs?.Count ?? 0} elementi");
        if (allObjs == null) return;

        // Prendiamo tutti i Transform attivi nella scena
        var allTransforms = FindObjectsOfType<Transform>();

        foreach (var obj in allObjs)
        {
            // Se fosse un “floor” (es. nel dominio elevator o logistic), saltiamo
            if (obj.type.StartsWith("floor"))
                continue;

            string targetName = obj.name.ToLower();

            // Cerco match ignorando "(Clone)" e case
            var match = allTransforms.FirstOrDefault(t =>
            {
                // Rimuovo eventuale "(clone)", confronto in minuscolo e rimuovo spazi
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
            focusedTransform = null;
            return;
        }

        string name = focusObjects[currentFocus];
        focusText.text = $"{name} ({currentFocus + 1}/{focusObjects.Count})";

        if (objectMap != null && objectMap.TryGetValue(name, out var go))
        {
            Debug.Log($"[UIManager] Applicando focus su '{name}' at position {go.transform.position}");

            // Impostiamo il Transform da seguire:
            focusedTransform = go.transform;

            // Calcoliamo l'offset tra camera e target (terra la stessa distanza e direzione)
            // includendo l'offset verticale che desideriamo
            Vector3 targetWithOffset = focusedTransform.position + Vector3.up * verticalOffset;
            followOffset = mainCamera.transform.position - targetWithOffset;

            // Teletrasporto iniziale: portiamo subito la camera
            // in targetWithOffset + followOffset
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

        // Ricomputiamo targetWithOffset in base alla posizione corrente del target
        Vector3 targetWithOffset = focusedTransform.position + Vector3.up * verticalOffset;

        // La posizione desiderata della camera è semplicemente:
        // targetWithOffset + followOffset
        Vector3 desiredPos = targetWithOffset + followOffset;

        // Se vuoi che la camera segua *istantaneamente*:
        // mainCamera.transform.position = desiredPos;
        //
        // Se invece vuoi un follow “più smussato”, usa Lerp o SmoothDamp:
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            desiredPos,
            Time.deltaTime * 5f  // regola questo coefficiente per rendere il follow più o meno "morbido"
        );

        // La camera guarda sempre il centro del target (con lo stesso verticalOffset)
        mainCamera.transform.LookAt(targetWithOffset);
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }
}
