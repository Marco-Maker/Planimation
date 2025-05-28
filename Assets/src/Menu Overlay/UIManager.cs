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
    [SerializeField] private float focusDistance = 5f;

    private List<string> focusObjects = new List<string>();
    private int currentFocus = 0;
    private Dictionary<string, GameObject> objectMap;

    void Awake()
    {
        // Solo raccolgo i nomi dai goal, senza cercare nulla in scena
        var goals = PlanInfo.GetInstance().GetGoals();
        focusObjects = goals != null
            ? goals.SelectMany(g => g.values).Distinct().ToList()
            : new List<string>();
    }

    void Start()
    {
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

        // Cache di tutti i transform (attivi) in scena
        var allTransforms = FindObjectsOfType<Transform>();

        foreach (var obj in allObjs)
        {
            if (obj.type.StartsWith("floor")) 
                continue;

            Debug.Log($"[UIManager] Cerco candidate per '{obj.name}'");

            // cerco qualsiasi transform il cui name cominci con "obj.name"
            var match = allTransforms
                .FirstOrDefault(t => t.name.StartsWith(obj.name));

            if (match != null)
            {
                Debug.Log($"[UIManager] → trovato in scena: '{match.name}'");
                objectMap[obj.name] = match.gameObject;
            }
            else
            {
                Debug.LogWarning($"[UIManager] **NON** trovato nessun GameObject che inizia con '{obj.name}'");
            }
        }
    }


    public void ShowPlan()
    {
        planPanel.SetActive(!planPanel.activeSelf);
        if (!planPanel.activeSelf) return;

        // Mostra l'intero plan come prima
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
        string name = focusObjects[currentFocus];
        focusText.text = name;

        if (objectMap != null && objectMap.TryGetValue(name, out var go))
            FocusOnObject(go.transform);
        else
            Debug.LogWarning($"[UIManager] Nessuna mappatura per '{name}'");
    }

    private void FocusOnObject(Transform target)
    {
        Vector3 fromPos = mainCamera.transform.position;
        Vector3 toPos   = target.position - mainCamera.transform.forward * focusDistance;
        Vector3 lookAt  = target.position;

        StopAllCoroutines();
        StartCoroutine(MoveCamera(fromPos, toPos, lookAt, 0.5f));
    }

    private IEnumerator MoveCamera(Vector3 fromPos, Vector3 toPos, Vector3 lookAt, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(fromPos, toPos, elapsed / duration);
            mainCamera.transform.LookAt(lookAt);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = toPos;
        mainCamera.transform.LookAt(lookAt);
    }
}
