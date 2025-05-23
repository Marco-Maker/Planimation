using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Linq;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class ObjectItem
{
    public string name;
    public string type;
    public TextMeshProUGUI number;
}

public class ObjectToAdd
{
    public string name;
    public string type;
}

[Serializable]
public class Predicates
{
    public string name;
    public int variant; // 0 = normal, 1 = numeric, 2 = temporal, 3 = event
    public List<string> values;
}

public class PredicateToAdd
{
    public string name;
    public List<string> values;
}

[Serializable]
public class GoalInput
{
    public string value;
    public GameObject dropdown;
}

[Serializable]
public class Goals
{
    public string problemName;
    public string name;
    public int problem; // 0 = logistic, 1 = robot, 2 = elevator
    public List<GoalInput> dropdown;
}

public class GoalToAdd
{
    public string name;
    public List<string> values;
}

public class MenuManager : MonoBehaviour
{
    [Header("COMPOSER")]
    [SerializeField] private GameObject composer;
    [SerializeField] private GameObject logisticComposer;
    [SerializeField] private GameObject robotComposer;
    [SerializeField] private GameObject elevatorComposer;

    [Header("OBJECTS")]
    [SerializeField] private List<ObjectItem> logisticObjects;
    [SerializeField] private List<ObjectItem> robotObjects;
    [SerializeField] private List<ObjectItem> elevatorObjects;
    private List<ObjectToAdd> objectsToAdd;

    [Header("PREDICATES")]
    [SerializeField] private GameObject predicateField;
    [SerializeField] private List<Predicates> logisticPredicatesList;
    [SerializeField] private List<Predicates> robotPredicatesList;
    [SerializeField] private List<Predicates> elevatorPredicatesList;
    private List<Predicates> predicatesList;
    private Dictionary<string, List<string>> predicatesAvailable;
    private List<PredicateToAdd> predicatesToAdd;

    [Header("PREDICATE-FIELD")]
    [SerializeField] private TextMeshProUGUI fieldTitle;
    [SerializeField] private TextMeshProUGUI fieldList;
    [SerializeField] private GameObject fieldOptions;
    [SerializeField] private GameObject predicateOptionPrefab;

    [Header("GOALS")]
    [SerializeField] private GameObject goalsField;
    [SerializeField] private TextMeshProUGUI goalsText;
    [SerializeField] private GameObject logisticGoalsField;
    [SerializeField] private GameObject robotGoalsField;
    [SerializeField] private GameObject elevatorGoalsField;
    [SerializeField] private List<Goals> goalsList;
    private List<GoalToAdd> goalsToAdd;

    [Header("GENERATOR")]
    [SerializeField] private ProblemGenerator generator;

    private Planner planner = new Planner();
    private int currentProblem = -1; // -1 = no problem selected, 0 = logistic, 1 = robot, 2 = elevator

    private void Start()
    {
        predicatesAvailable = new Dictionary<string, List<string>>();
        predicatesToAdd = new List<PredicateToAdd>();
        objectsToAdd = new List<ObjectToAdd>();
        goalsToAdd = new List<GoalToAdd>();
    }

    public void OpenPredicateField(string name)
    {
        predicateField.SetActive(true);
        FillPredicates(name);
    }

    private void FillPredicates(string name)
    {
        switch (currentProblem)
        {
            case 0:
                predicatesList = logisticPredicatesList;
                FillObjectsToAdd(logisticObjects);
                break;
            case 1:
                predicatesList = robotPredicatesList;
                FillObjectsToAdd(robotObjects);
                break;
            case 2:
                predicatesList = elevatorPredicatesList;
                FillObjectsToAdd(elevatorObjects);
                break;
        }
        foreach (var predicate in predicatesList)
        {
            if(predicate.name == name)
                predicatesAvailable.Add(predicate.name, predicate.values);
        }
        fieldTitle.text = name;
        fieldList.text = "";
        foreach (var predicate in predicatesToAdd)
        {
            if (predicate.name == name)
            {
                fieldList.text += name + " ";
                foreach (var value in predicate.values)
                {
                    fieldList.text += value + " ";
                }
                fieldList.text += "\n";
            }
        }
        foreach (var predicate in predicatesList)
        {
            if (predicate.name == name)
                foreach (var value in predicate.values)
                {
                    GameObject obj = Instantiate(predicateOptionPrefab, fieldOptions.transform);
                    obj.transform.GetComponentInChildren<PredicateInputSetter>().SetLabel(value);
                    obj.transform.GetComponentInChildren<TMP_Dropdown>().ClearOptions();
                    List<string> options = new List<string>();
                    foreach (var o in objectsToAdd)
                    {
                        if (o.type.Contains(value) || o.name.Contains(value))
                        {
                            options.Add(o.name);
                        }

                    }
                    obj.transform.GetComponentInChildren<TMP_Dropdown>().AddOptions(options);
                }
        }
    }

    private void FillFieldList(string name)
    {
        //deve mostrare solo quelle sul predicato corrente
        fieldList.text = "";
        foreach (var predicate in predicatesToAdd)
        {
            Debug.Log(predicate.name);
            if (predicate.name == name)
            {
                fieldList.text += name + " ";
                foreach (var value in predicate.values)
                {
                    fieldList.text += value + " ";
                }
                fieldList.text += "\n";
            }
        }
    }


    public void ClosePredicateField()
    {
        predicatesAvailable.Clear();
        foreach (Transform child in fieldOptions.transform)
            Destroy(child.gameObject);
        predicateField.SetActive(false);
    }

    public void AddPredicate()
    {
        // Costruisci il predicato dal campo UI
        PredicateToAdd p = new PredicateToAdd
        {
            name = fieldTitle.text,
            values = new List<string>()
        };

        GameObject options = GameObject.Find("PredicateInputOptions");
        foreach (Transform child in options.transform)
        {
            TMP_Dropdown dd = child.GetComponentInChildren<TMP_Dropdown>();
            if (dd != null)
            {
                string val = dd.options[dd.value].text;
                p.values.Add(val);
            }
        }

        // Validazione: tutti i parametri selezionati
        if (p.values.Any(v => string.IsNullOrEmpty(v)))
        {
            Debug.LogError("Devi selezionare tutti i parametri del predicato.");
            return;
        }

        // Controllo duplicati
        bool exists = predicatesToAdd.Any(x =>
            x.name == p.name && x.values.SequenceEqual(p.values)
        );
        if (exists)
        {
            Debug.LogWarning($"Predicato già aggiunto: {p.name}({string.Join(",", p.values)})");
        }
        else
        {
            predicatesToAdd.Add(p);
        }
        FillFieldList(fieldTitle.text);
    }

    public void RemovePredicate()
    {
        PredicateToAdd p = new PredicateToAdd
        {
            name = fieldTitle.text,
            values = new List<string>()
        };

        GameObject options = GameObject.Find("PredicateInputOptions");
        foreach (Transform child in options.transform)
        {
            TMP_Dropdown dd = child.GetComponentInChildren<TMP_Dropdown>();
            if (dd != null)
            {
                string val = dd.options[dd.value].text;
                p.values.Add(val);
            }
        }

        // Validazione: tutti i parametri selezionati
        if (p.values.Any(v => string.IsNullOrEmpty(v)))
        {
            Debug.LogError("Devi selezionare tutti i parametri del predicato.");
            return;
        }

        // Controlla che c'è e in caso rimuovilo 
        bool exists = predicatesToAdd.Any(x =>
            x.name == p.name && x.values.SequenceEqual(p.values)
        );
        if (exists)
        {
            predicatesToAdd.RemoveAll(x =>
                x.name == p.name && x.values.SequenceEqual(p.values)
            );
        }
        else
        {
            Debug.LogWarning($"Predicato non trovato: {p.name}({string.Join(",", p.values)})");
        }
        FillFieldList(fieldTitle.text);
    }


    public void OpenComposer(int problem)
    {
        composer.SetActive(true);
        switch (problem)
        {
            case 0:
                logisticComposer.SetActive(true);
                robotComposer.SetActive(false);
                elevatorComposer.SetActive(false);
                currentProblem = 0;
                break;
            case 1:
                logisticComposer.SetActive(false);
                robotComposer.SetActive(true);
                elevatorComposer.SetActive(false);
                currentProblem = 1;
                break;
            case 2:
                logisticComposer.SetActive(false);
                robotComposer.SetActive(false);
                elevatorComposer.SetActive(true);
                currentProblem = 2;
                break;
        }
    }

    public void CloseComposer()
    {
        composer.SetActive(false);
        logisticComposer.SetActive(false);
        robotComposer.SetActive(false);
        elevatorComposer.SetActive(false);
        objectsToAdd.Clear();
        predicatesToAdd.Clear();
        goalsToAdd.Clear();
        goalsText.text = "";
        currentProblem = -1;
    }

    public void AddObjectCount(string name)
    {
        switch (currentProblem)
        {
            case 0:
                foreach (var obj in logisticObjects)
                {
                    if (obj.name == name)
                    {
                        int count = int.Parse(obj.number.text);
                        count++;
                        obj.number.text = count.ToString();
                    }
                }
                break;
            case 1:
                foreach (var obj in robotObjects)
                {
                    if (obj.name == name)
                    {
                        int count = int.Parse(obj.number.text);
                        count++;
                        obj.number.text = count.ToString();
                    }
                }
                break;
            case 2:
                foreach (var obj in elevatorObjects)
                {
                    if (obj.name == name)
                    {
                        int count = int.Parse(obj.number.text);
                        count++;
                        obj.number.text = count.ToString();
                    }
                }
                break;
        }
    }

    public void RemoveObjectCount(string name)
    {
        switch (currentProblem)
        {
            case 0:
                foreach (var obj in logisticObjects)
                {
                    if (obj.name == name)
                    {
                        int count = int.Parse(obj.number.text);
                        if (count > 0)
                        {
                            count--;
                            obj.number.text = count.ToString();
                        }
                    }
                }
                break;
            case 1:
                foreach (var obj in robotObjects)
                {
                    if (obj.name == name)
                    {
                        int count = int.Parse(obj.number.text);
                        if (count > 0)
                        {
                            count--;
                            obj.number.text = count.ToString();
                        }
                    }
                }
                break;
            case 2:
                foreach (var obj in elevatorObjects)
                {
                    if (obj.name == name)
                    {
                        int count = int.Parse(obj.number.text);
                        if (count > 0)
                        {
                            count--;
                            obj.number.text = count.ToString();
                        }
                    }
                }
                break;
        }
    }

    public void OpenGoals()
    {
        goalsField.SetActive(true);
        
        switch (currentProblem)
        {
            case 0:
                logisticGoalsField.SetActive(true);
                robotGoalsField.SetActive(false);
                elevatorGoalsField.SetActive(false);
                FillObjectsToAdd(logisticObjects);
                break;
            case 1:
                logisticGoalsField.SetActive(false);
                robotGoalsField.SetActive(true);
                elevatorGoalsField.SetActive(false);
                FillObjectsToAdd(robotObjects);
                break;
            case 2:
                logisticGoalsField.SetActive(false);
                robotGoalsField.SetActive(false);
                elevatorGoalsField.SetActive(true);
                FillObjectsToAdd(elevatorObjects);
                break;
        }
        FillGoals();

    }

    private void FillObjectsToAdd(List<ObjectItem> l)
    {
        objectsToAdd.Clear();
        foreach (var obj in l)
        {
            int count = int.Parse(obj.number.text);
            if (count > 0)
            {
                for (int i = 1; i <= count; i++)
                {
                    ObjectToAdd o = new ObjectToAdd();
                    o.name = obj.name.ToLower() + i;
                    o.type = obj.type;
                    objectsToAdd.Add(o);
                }
            }
        }
    }
    private void FillGoals()
    {
        foreach (var g in goalsList)
        {
            if (g.problem == currentProblem)
            {
                foreach (var input in g.dropdown)
                {
                    TMP_Dropdown dropdown = input.dropdown.GetComponentInChildren<TMP_Dropdown>();

                    if (dropdown != null)
                    {
                        dropdown.ClearOptions(); // Rimuove opzioni precedenti

                        List<string> options = new List<string>();
                        foreach (var obj in objectsToAdd)
                        {
                            if (obj.name.StartsWith(input.value))
                            {
                                options.Add(obj.name); 
                            }
                            
                        }

                        dropdown.AddOptions(options);
                    }
                    else
                    {
                        Debug.LogWarning("Dropdown TMP non trovato in " + input.dropdown.name);
                    }
                }
            }
        }
    }

    public void AddGoal()
    {
        // Costruisci il goal dal campo UI
        GoalToAdd g = new GoalToAdd
        {
            values = new List<string>()
        };
        GameObject options = null;
        switch (currentProblem)
        {
            case 0: options = GameObject.Find("LogisticGoalInputOptions"); break;
            case 1: options = GameObject.Find("RobotGoalInputOptions"); break;
            case 2: options = GameObject.Find("ElevatorGoalInputOptions"); break;
        }

        foreach (Transform child in options.transform)
        {
            if (child.name == "GoalName")
            {
                g.name = child.GetComponent<TextMeshProUGUI>().text
                    .ToLower().Replace("\n", "").Trim();
            }
            else if (child.name.Contains("Input"))
            {
                TMP_Dropdown dd = child.GetComponentInChildren<TMP_Dropdown>();
                if (dd != null)
                    g.values.Add(dd.options[dd.value].text);
            }
        }

        // Validazione
        if (string.IsNullOrEmpty(g.name) || g.values.Count == 0 || g.values.Any(v => string.IsNullOrEmpty(v)))
        {
            Debug.LogError("Goal non valido: nome o parametri mancanti.");
            return;
        }

        // Controllo duplicati
        bool goalExists = goalsToAdd.Any(x =>
            x.name == g.name && x.values.SequenceEqual(g.values)
        );
        if (goalExists)
        {
            Debug.LogWarning($"Goal già presente: {g.name}({string.Join(",", g.values)})");
            return;
        }

        // Aggiungi e aggiorna UI
        goalsToAdd.Add(g);
        UpdateGoalText();
    }

    public void RemoveGoal()
    {
        //fai la stessa cosa di AddGoal ma per rimuovere
        GoalToAdd g = new GoalToAdd
        {
            values = new List<string>()
        };
        GameObject options = null;
        switch (currentProblem)
        {
            case 0: options = GameObject.Find("LogisticGoalInputOptions"); break;
            case 1: options = GameObject.Find("RobotGoalInputOptions"); break;
            case 2: options = GameObject.Find("ElevatorGoalInputOptions"); break;
        }
        if (options != null)
        {
            foreach (Transform child in options.transform)
            {
                if (child.name == "GoalName")
                {
                    g.name = child.GetComponent<TextMeshProUGUI>().text
                        .ToLower().Replace("\n", "").Trim();
                }
                else if (child.name.Contains("Input"))
                {
                    TMP_Dropdown dd = child.GetComponentInChildren<TMP_Dropdown>();
                    if (dd != null)
                        g.values.Add(dd.options[dd.value].text);
                }
            }
        }
        // Validazione
        if (string.IsNullOrEmpty(g.name) || g.values.Count == 0 || g.values.Any(v => string.IsNullOrEmpty(v)))
        {
            Debug.LogError("Goal non valido: nome o parametri mancanti.");
            return;
        }
        // Controllo duplicati
        bool goalExists = goalsToAdd.Any(x =>
            x.name == g.name && x.values.SequenceEqual(g.values)
        );
        if (goalExists)
        {
            goalsToAdd.RemoveAll(x =>
                x.name == g.name && x.values.SequenceEqual(g.values)
            );
        }
        else
        {
            Debug.LogWarning($"Goal non trovato: {g.name}({string.Join(",", g.values)})");
        }
        // aggiorna UI
        UpdateGoalText();
    }


    private void UpdateGoalText()
    {
        goalsText.text = "";
        foreach (var g in goalsToAdd)
        {
            goalsText.text += g.name + " ";
            foreach (var value in g.values)
            {
                goalsText.text += value + " ";
            }
            goalsText.text += "\n";
        }
    }

    public void CloseGoals()
    {
        goalsField.SetActive(false);
        logisticGoalsField.SetActive(false);
        robotGoalsField.SetActive(false);
        elevatorGoalsField.SetActive(false);
    }

    public void Simulate()
    {
        // Validazioni preliminari
        if (objectsToAdd.Count == 0)
        {
            Debug.LogError("Devi aggiungere almeno un oggetto.");
            return;
        }
        if (predicatesToAdd.Count == 0)
        {
            Debug.LogError("Devi aggiungere almeno un predicato.");
            return;
        }
        if (goalsToAdd.Count == 0)
        {
            Debug.LogError("Devi aggiungere almeno un goal.");
            return;
        }

        // Salva dati e genera PDDL
        PlanInfo.GetInstance().SetObjects(objectsToAdd);
        PlanInfo.GetInstance().SetPredicates(predicatesToAdd);
        PlanInfo.GetInstance().SetGoals(goalsToAdd);
        PlanInfo.GetInstance().SetDomainType(currentProblem);

        generator.GenerateAndSave();
        planner.RunShellCommand();

        // Carica la scena corretta
        switch (currentProblem)
        {
            case 0: SceneManager.LoadScene("LogisticScene"); break;
            case 1: SceneManager.LoadScene("RobotScene"); break;
            case 2: SceneManager.LoadScene("ElevatorScene"); break;
        }
    }

}
