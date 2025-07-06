using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.IO;


#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

[Serializable]
public class Problem
{
    public string problem;
    public int domain; // 0 = logistic, 1 = robot, 2 = elevator
    public int type; // 0 = normal, 1 = numeric/temporal, 2 = event
    public List<ObjectItem> objects;
    public List<Predicates> predicates;
    public List<Functions> functions;
}

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
    public List<string> values;
}

public class PredicateToAdd
{
    public string name;
    public List<string> values;
}


[Serializable]
public class Functions
{
    public string name;
    public List<string> value;
}

public class FunctionToAdd
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
    [SerializeField] private List<Problem> problemsList;

    [Header("ERROR")]
    [SerializeField] private GameObject errorArea;
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("COMPOSER")]
    [SerializeField] private GameObject types;
    [SerializeField] private GameObject composer;
    [SerializeField] private GameObject logisticComposer;
    [SerializeField] private GameObject logisticNumericComposer;
    [SerializeField] private GameObject logisticEventComposer;
    [SerializeField] private GameObject robotComposer;
    [SerializeField] private GameObject robotTemporalComposer;
    [SerializeField] private GameObject robotEventComposer;
    [SerializeField] private GameObject elevatorComposer;
    [SerializeField] private GameObject elevatorNumericComposer;
    [SerializeField] private GameObject elevatorEventComposer;

    private List<ObjectItem> objectList;
    private List<ObjectToAdd> objectsToAdd;

    [Header("PREDICATES")]
    [SerializeField] private GameObject predicateField;
    private List<Predicates> predicatesList;
    private List<PredicateToAdd> predicatesToAdd;

    [Header("PREDICATE-FIELD")]
    [SerializeField] private TextMeshProUGUI fieldTitle;
    [SerializeField] private TextMeshProUGUI fieldList;
    [SerializeField] private GameObject fieldOptions;
    [SerializeField] private GameObject predicateOptionPrefab;

    [Header("FUNCTIONS")]
    [SerializeField] private GameObject functions;
    [SerializeField] private GameObject functionsField;
    [SerializeField] private GameObject logisticNumeric;
    [SerializeField] private GameObject logisticEvent;
    [SerializeField] private GameObject robotTemporal;
    [SerializeField] private GameObject robotEvent;
    [SerializeField] private GameObject elevatorNumeric;
    [SerializeField] private GameObject elevatorEvent;
    private List<Functions> functionsList;
    private List<FunctionToAdd> functionsToAdd;

    [Header("FUNCTION-FIELD")]
    [SerializeField] private TextMeshProUGUI functionFieldTitle;
    [SerializeField] private TextMeshProUGUI functionFieldList;
    [SerializeField] private GameObject functionfieldOptions;
    [SerializeField] private GameObject integerOptionPrefab;

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
    private int currentType = -1; // -1 = no type selected, 0 = normal, 1 = numeric/temporal, 2 = event

    private void Start()
    {
        predicatesToAdd = new List<PredicateToAdd>();
        objectsToAdd = new List<ObjectToAdd>();
        functionsToAdd = new List<FunctionToAdd>();
        goalsToAdd = new List<GoalToAdd>();
    }

    private void SetLists()
    {
        Problem problem = new Problem();
        foreach (var p in problemsList)
        {
            if(p.domain == currentProblem && p.type == currentType)
            {
                problem = p;
                break;
            }
        }
        objectList = problem.objects;
        predicatesList = problem.predicates;
        functionsList = problem.functions;
    }

    public void CloseError()
    {
        errorArea.SetActive(false);
        errorText.text = "";
    }

    // ---------------------------------------------------------------START PREDICATES LIST---------------------------------------------------------------

    public void OpenPredicateField(string name)
    {
        predicateField.SetActive(true);
        FillPredicates(name);
    }

    private void FillPredicates(string name)
    {
        FillObjectsToAdd(objectList);
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
            //Debug.Log(predicate.name);
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
        foreach (Transform child in fieldOptions.transform)
            Destroy(child.gameObject);
        predicateField.SetActive(false);
    }

    public void AddPredicate()
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
            errorArea.SetActive(true);
            errorText.text = "You must select all parameters of the predicate.";
            return;
        }

        // ------------------ INIZIO VINCOLI PERSONALIZZATI ------------------

        if (p.name == "in-city")
        {
            string city = p.values[1];
            int count = predicatesToAdd.Count(pred => pred.name == "in-city" && pred.values[1] == city);
            if (count >= 6)
            {
                errorArea.SetActive(true);
                errorText.text = $"{city} can have at most 6 places.";
                return;
            }
        }

        if (p.name == "link")
        {
            string city = p.values[0];
            int count = predicatesToAdd.Count(pred => pred.name == "link" && pred.values[0] == city);
            if (count >= 5)
            {
                errorArea.SetActive(true);
                errorText.text = $"{city} can only be linked to 5 cities.";
                return;
            }
        }

        if (p.name == "connected")
        {
            string room = p.values[0];
            int count = predicatesToAdd.Count(pred => pred.name == "connected" && pred.values[0] == room);
            if (count >= 4)
            {
                errorArea.SetActive(true);
                errorText.text = $"{room} can only be connected to 4 rooms.";
                return;
            }
        }

        // ------------------ FINE VINCOLI PERSONALIZZATI ------------------

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
            errorArea.SetActive(true);
            errorText.text = "You must select all parameters of the predicate.";
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

    // ---------------------------------------------------------------END PREDICATES LIST---------------------------------------------------------------
    // ---------------------------------------------------------------START TYPE------------------------------------------------------------------------

    public void OpenTypes(int problem)
    {
        types.SetActive(true);
        currentProblem = problem;
    }

    public void CloseTypes()
    {
        types.SetActive(false);
        currentProblem = -1;
    }

    // ---------------------------------------------------------------END TYPE---------------------------------------------------------------------------
    // ---------------------------------------------------------------START COMPOSER---------------------------------------------------------------------

    public void OpenComposer(int type)
    {
        types.SetActive(false);
        composer.SetActive(true);
        currentType = type;
        SetLists();
        switch (currentProblem)
        {
            case 0:
                robotComposer.SetActive(false);
                elevatorComposer.SetActive(false);
                switch (currentType)
                {
                    case 0:
                        logisticComposer.SetActive(true);
                        logisticNumericComposer.SetActive(false);
                        logisticEventComposer.SetActive(false);
                        break;
                    case 1:
                        logisticComposer.SetActive(false);
                        logisticNumericComposer.SetActive(true);
                        logisticEventComposer.SetActive(false);
                        break;
                    case 2:
                        logisticComposer.SetActive(false);
                        logisticNumericComposer.SetActive(false);
                        logisticEventComposer.SetActive(true);
                        break;
                    default:
                        Debug.LogError("Tipo di compositore non valido per il problema logistico.");
                        break;
                }
                
                break;
            case 1:
                logisticComposer.SetActive(false);
                elevatorComposer.SetActive(false);
                switch (currentType)
                {
                    case 0:
                        robotComposer.SetActive(true);
                        robotTemporalComposer.SetActive(false);
                        robotEventComposer.SetActive(false);
                        break;
                    case 1:
                        robotComposer.SetActive(false);
                        robotTemporalComposer.SetActive(true);
                        robotEventComposer.SetActive(false);
                        break;
                    case 2:
                        robotComposer.SetActive(false);
                        robotTemporalComposer.SetActive(false);
                        robotEventComposer.SetActive(true);
                        break;
                    default:
                        Debug.LogError("Tipo di compositore non valido per il problema logistico.");
                        break;
                }
                break;
            case 2:
                logisticComposer.SetActive(false);
                robotComposer.SetActive(false);
                switch (currentType)
                {
                    case 0:
                        elevatorComposer.SetActive(true);
                        elevatorNumericComposer.SetActive(false);
                        elevatorEventComposer.SetActive(false);
                        break;
                    case 1:
                        elevatorComposer.SetActive(false);
                        elevatorNumericComposer.SetActive(true);
                        elevatorEventComposer.SetActive(false);
                        break;
                    case 2:
                        elevatorComposer.SetActive(false);
                        elevatorNumericComposer.SetActive(false);
                        elevatorEventComposer.SetActive(true);
                        break;
                    default:
                        Debug.LogError("Tipo di compositore non valido per il problema logistico.");
                        break;
                }
                break;
        }
    }

    public void CloseComposer()
    {
        types.SetActive(true);
        composer.SetActive(false);
        logisticComposer.SetActive(false);
        logisticNumericComposer.SetActive(false);
        logisticEventComposer.SetActive(false);
        robotComposer.SetActive(false);
        robotTemporalComposer.SetActive(false);
        robotEventComposer.SetActive(false);
        elevatorComposer.SetActive(false);
        elevatorNumericComposer.SetActive(false);
        elevatorEventComposer.SetActive(false);
        objectsToAdd.Clear();
        predicatesToAdd.Clear();
        functionsToAdd.Clear();
        goalsToAdd.Clear();
        goalsText.text = "";
        currentType = -1;
    }

    public void AddObjectCount(string name)
    {
        foreach (var obj in objectList) {
            if (obj.name == name)
            {
                int count = int.Parse(obj.number.text);
                count++;
                obj.number.text = count.ToString();
            }
        }
    }

    public void RemoveObjectCount(string name)
    {
        foreach(var obj in objectList) {
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
    }

    // ---------------------------------------------------------------END COMPOSE---------------------------------------------------------------------------
    // ---------------------------------------------------------------START FUNCTION------------------------------------------------------------------------

    public void OpenFunctions()
    {
        functions.SetActive(true);
        switch (currentProblem)
        {
            case 0:
                switch (currentType)
                {
                    case 1:
                        logisticNumeric.SetActive(true);
                        logisticEvent.SetActive(false);
                        break;
                    case 2:
                        logisticNumeric.SetActive(false);
                        logisticEvent.SetActive(true);
                        break;
                }             
                break;
            case 1:
                switch (currentType)
                {
                    case 1:
                        robotTemporal.SetActive(true);
                        robotEvent.SetActive(false);
                        break;
                    case 2:
                        robotTemporal.SetActive(false);
                        robotEvent.SetActive(true);
                        break;
                }
                break;
            case 2:
                switch (currentType)
                {
                    case 1:
                        elevatorNumeric.SetActive(true);
                        elevatorEvent.SetActive(false);
                        break;
                    case 2:
                        elevatorNumeric.SetActive(false);
                        elevatorEvent.SetActive(true);
                        break;
                }
                break;
        }
        //FillFunctions(objectList, functionsList);
    }

    public void CloseFunctions()
    {
        functions.SetActive(false);
        logisticNumeric.SetActive(false);
        logisticEvent.SetActive(false);
        robotTemporal.SetActive(false);
        robotEvent.SetActive(false);
        elevatorNumeric.SetActive(false);
        elevatorEvent.SetActive(false);
        functionsToAdd.Clear();
    }

    // ---------------------------------------------------------------END FUNCTION----------------------------------------------------------------------------------
    // ---------------------------------------------------------------START FUNCTION LIST---------------------------------------------------------------------------
    public void OpenFunctionField(string name)
    {
        functionsField.SetActive(true);
        FillObjectsToAdd(objectList);
        functionFieldTitle.text = name;
        functionFieldList.text = "";
        foreach (var function in functionsToAdd)
        {
            if (function.name == name)
            {
                functionFieldList.text += name + " ";
                foreach (var value in function.values)
                {
                    functionFieldList.text += value + " ";
                }
                functionFieldList.text += "\n";
            }
        }
        foreach (var function in functionsList)
        {
            if (function.name == name)
            {
                foreach (var value in function.value)
                {
                    if (value.Equals("integer"))
                    {
                        GameObject obj = Instantiate(integerOptionPrefab, functionfieldOptions.transform);
                    }
                    else
                    {
                        GameObject obj = Instantiate(predicateOptionPrefab, functionfieldOptions.transform);
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
        }
    }
    public void CloseFunctionField()
    {
        foreach (Transform child in functionfieldOptions.transform)
            Destroy(child.gameObject);
        functionsField.SetActive(false);
    }
    public void AddFunction()
    {
        // Costruisci la funzione dal campo UI
        FunctionToAdd f = new FunctionToAdd
        {
            name = functionFieldTitle.text,
            values = new List<string>()
        };

        GameObject options = GameObject.Find("FunctionInputOptions");
        foreach (Transform child in options.transform)
        {
            if (child.GetComponentInChildren<TMP_Dropdown>() != null)
            {
                TMP_Dropdown dd = child.GetComponentInChildren<TMP_Dropdown>();
                if (dd != null)
                {
                    string val = dd.options[dd.value].text;
                    f.values.Add(val);
                }
            }
            else
            {
                f.values.Add(child.GetComponentInChildren<IntegerInputSetter>().GetValue().ToString());
            }

        }

        // Validazione: tutti i parametri selezionati
        if (f.values.Any(v => string.IsNullOrEmpty(v)))
        {
            errorArea.SetActive(true);
            errorText.text = "You must select all parameters of the function.";
            return;
        }

        // Controllo duplicati
        bool exists = functionsToAdd.Any(x =>
            x.name == f.name && x.values.SequenceEqual(f.values)
        );
        if (exists)
        {
            Debug.LogWarning($"Funzione già aggiunta: {f.name}({string.Join(",", f.values)})");
        }
        else
        {
            functionsToAdd.Add(f);
        }
        FillFunctionList(functionFieldTitle.text);
    }

    public void RemoveFunction()
    {
        FunctionToAdd f = new FunctionToAdd
        {
            name = functionFieldTitle.text,
            values = new List<string>()
        };

        GameObject options = GameObject.Find("FunctionInputOptions");
        foreach (Transform child in options.transform)
        {
            if(child.GetComponentInChildren<TMP_Dropdown>() != null)
            {
                TMP_Dropdown dd = child.GetComponentInChildren<TMP_Dropdown>();
                if (dd != null)
                {
                    string val = dd.options[dd.value].text;
                    f.values.Add(val);
                }
            }
            else
            {
                f.values.Add(child.GetComponentInChildren<IntegerInputSetter>().GetValue().ToString());
            }
            
        }

        // Validazione: tutti i parametri selezionati
        if (f.values.Any(v => string.IsNullOrEmpty(v)))
        {
            errorArea.SetActive(true);
            errorText.text = "You must select all parameters of the function.";
            return;
        }

        // Controlla che c'è e in caso rimuovilo 
        bool exists = functionsToAdd.Any(x =>
            x.name == f.name && x.values.SequenceEqual(f.values)
        );
        if (exists)
        {
            functionsToAdd.RemoveAll(x =>
                x.name == f.name && x.values.SequenceEqual(f.values)
            );
        }
        else
        {
            Debug.LogWarning($"Funzione non trovata: {f.name}({string.Join(",", f.values)})");
        }
        FillFunctionList(functionFieldTitle.text);
    }

    private void FillFunctionList(string name)
    {
        functionFieldList.text = "";
        foreach (var function in functionsToAdd)
        {
            if (function.name == name)
            {
                functionFieldList.text += name + " ";
                foreach (var value in function.values)
                {
                    functionFieldList.text += value + " ";
                }
                functionFieldList.text += "\n";
            }
        }
    }

    // ---------------------------------------------------------------END FUNCTION LIST---------------------------------------------------------------------------
    // ---------------------------------------------------------------START GOALS----------------------------------------------------------------------------

    public void OpenNext()
    {
        if (currentType == 0)
        {
            OpenGoals();
        }
        else
        {
            OpenFunctions();
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
                break;
            case 1:
                logisticGoalsField.SetActive(false);
                robotGoalsField.SetActive(true);
                if(currentType != 2)
                {
                    goalsList[1].dropdown[2].dropdown.SetActive(false);
                    goalsList[1].dropdown[1].dropdown.SetActive(true);
                }
                else
                {
                    goalsList[1].dropdown[2].dropdown.SetActive(true);
                    goalsList[1].dropdown[1].dropdown.SetActive(false);
                }
                elevatorGoalsField.SetActive(false);
                break;
            case 2:
                logisticGoalsField.SetActive(false);
                robotGoalsField.SetActive(false);
                elevatorGoalsField.SetActive(true);
                break;
        }
        FillObjectsToAdd(objectList);
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
            //Debug.Log("Goal: " + g.problemName + " - " + g.name + " - " + g.problem);
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
                            //Debug.Log("Controllo oggetto: " + obj.name + " - " + input.value);
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
                if(currentProblem == 1)
                {
                    if (currentType != 2 && child.name.StartsWith("Garden"))
                    {
                        continue;
                    }
                    else if(currentType == 2 && child.name.StartsWith("Room"))
                    {
                        continue;
                    }
                }
                TMP_Dropdown dd = child.GetComponentInChildren<TMP_Dropdown>();
                if (dd != null)
                    Debug.Log("Aggiungo valore: " + dd.options[dd.value].text + " value interno " + dd.value + " size " + dd.options.Count );
                    g.values.Add(dd.options[dd.value].text);
            }
        }

        // Validazione
        if (string.IsNullOrEmpty(g.name) || g.values.Count == 0 || g.values.Any(v => string.IsNullOrEmpty(v)))
        {
            errorArea.SetActive(true);
            errorText.text = "Invalid goal: missing name or parameters.";
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
                    if (currentProblem == 1)
                    {
                        if (currentType != 2 && child.name.StartsWith("Garden"))
                        {
                            continue;
                        }
                        else if (currentType == 2 && child.name.StartsWith("Room"))
                        {
                            continue;
                        }
                    }
                    TMP_Dropdown dd = child.GetComponentInChildren<TMP_Dropdown>();
                    if (dd != null)
                        g.values.Add(dd.options[dd.value].text);
                }
            }
        }
        // Validazione
        if (string.IsNullOrEmpty(g.name) || g.values.Count == 0 || g.values.Any(v => string.IsNullOrEmpty(v)))
        {
            errorArea.SetActive(true);
            errorText.text = "Invalid goal: missing name or parameters.";
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

    // ---------------------------------------------------------------END GOALS---------------------------------------------------------------------------
    // ---------------------------------------------------------------SIMULATE----------------------------------------------------------------------------

    public void Simulate()
    {
        // --- 1) Validazioni preliminari (invariate) ---
        if (objectsToAdd.Count == 0)
        {
            ShowError("You need at least an object.");
            return;
        }
        if (currentType == 0 && predicatesToAdd.Count == 0)
        {
            ShowError("You need at least a predicate.");
            return;
        }
        if (currentType != 0 && functionsToAdd.Count == 0)
        {
            ShowError("You need at least a function.");
            return;
        }
        if (goalsToAdd.Count == 0)
        {
            ShowError("You need at least a goal.");
            return;
        }

        // --- 2) Genera e salva i PDDL ---
        var pi = PlanInfo.GetInstance();
        pi.SetObjects(objectsToAdd);
        pi.SetPredicates(predicatesToAdd);
        pi.SetFunctions(functionsToAdd);
        pi.SetGoals(goalsToAdd);
        pi.SetDomainType(currentProblem, currentType);

        generator.SetDomainName(pi.GetDomainName());
        generator.GenerateAndSave();

        // --- 3) Costruisci path a dominio e problema ---
        string domainFilename = pi.GetDomainName(); // es. "domain-robot-temporal.pddl"
        string domainPath = Path.Combine(
            Application.dataPath,
            "PDDL", "AllFile",
            GetDomainFolder(currentProblem),
            domainFilename
        );
        string problemPath = Path.Combine(
            Application.dataPath,
            "Generated",
            "problem.pddl"
        );

        // --- 4) Se TEMPORAL → invia a OPTIC in Docker ---
        if (domainFilename.ToLower().Contains("temporal") || domainFilename.ToLower().Contains("2-1"))
        {
            string domainPddl  = File.ReadAllText(domainPath);
            string problemPddl = File.ReadAllText(problemPath);

            StartCoroutine(PddlApiClient.RequestPlan(
                domainPddl,
                problemPddl,
                onSuccess: steps =>
                {
                    if (steps.Count == 0)
                    {
                        ShowError("No plan found by OPTIC.");
                        return;
                    }
                    // usa SetPlan, non SetPlanSteps
                    pi.SetPlan(steps);
                    SceneManager.LoadScene(GetSceneName(currentProblem));
                },
                onError: err =>
                {
                    ShowError("OPTIC Error:\n" + err);
                }
            ));
            return; // non esegue il planner locale
        }

        // --- 5) Altrimenti → planner locale (.jar) ---
        planner.RunShellCommand();
        if (planner.CheckError())
        {
            SceneManager.LoadScene(GetSceneName(currentProblem));
        }
        else
        {
            ShowError("Plan not found.");
        }
    }

    // ─────── helper methods ───────
    private string GetDomainFolder(int problem)
    {
        return problem switch
        {
            0 => "Logistic",
            1 => "Robot",
            2 => "Elevator",
            _ => throw new ArgumentException("Unknown problem")
        };
    }

    private string GetSceneName(int problem)
    {
        return problem switch
        {
            0 => "LogisticScene",
            1 => "RobotScene",
            2 => "ElevatorScene",
            _ => "MainMenu"
        };
    }

    private void ShowError(string msg)
    {
        errorArea.SetActive(true);
        errorText.text = msg;
    }

}
