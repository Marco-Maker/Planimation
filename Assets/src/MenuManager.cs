using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
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
    public int variant; // 0 = normal, 1 = numeric, 2 = temporal
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

    public void ClosePredicateField(bool add)
    {
        if (add)
        {
            PredicateToAdd p = new PredicateToAdd();
            p.values = new List<string>(); 
            GameObject options = GameObject.Find("PredicateInputOptions");
            p.name = fieldTitle.text;
            foreach (Transform child in options.transform)
            {
                for(int i = 0; i < child.childCount; i++) {
                    if (child.GetChild(i).name == "Dropdown")
                    {
                        for(int j = 0; j < child.GetChild(j).childCount; j++)
                        {
                            string val = child.GetChild(j).GetComponent<TMP_Dropdown>().options[child.GetChild(j).GetComponent<TMP_Dropdown>().value].text;
                            p.values.Add(val);
                        }
                    }
                }
                
            }
            predicatesToAdd.Add(p);
        }
        predicatesAvailable.Clear();
        foreach (Transform child in fieldOptions.transform)
        {
            Destroy(child.gameObject);
        }
        predicateField.SetActive(false);
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
        GoalToAdd g = new GoalToAdd();
        g.values = new List<string>();
        GameObject options = new GameObject(); //GameObject.Find("GoalsInputOptions")
        switch (currentProblem)
        {
            case 0:
                options = GameObject.Find("LogisticGoalInputOptions");
                break;
            case 1:
                options = GameObject.Find("RobotGoalInputOptions");
                break;
            case 2:
                options = GameObject.Find("ElevatorGoalInputOptions");
                break;
        }
        foreach (Transform child in options.transform)
        {
            if (child.name == "GoalName") { 
                g.name = child.GetComponent<TextMeshProUGUI>().text.ToLower();
            }else if (child.name.Contains("Input"))
            {
                g.values.Add(child.GetComponentInChildren<TMP_Dropdown>().options[child.GetComponentInChildren<TMP_Dropdown>().value].text);
            }
        }
        goalsToAdd.Add(g);
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
        PlanInfo.GetInstance().SetObjects(objectsToAdd);
        PlanInfo.GetInstance().SetPredicates(predicatesToAdd);
        PlanInfo.GetInstance().SetGoals(goalsToAdd);
        //objectsToAdd.Clear();
        //predicatesToAdd.Clear();
        //goalsToAdd.Clear();
        switch (currentProblem)
        {
            case 0:
                SceneManager.LoadScene("LogisticScene");
                break;
            case 1:
                SceneManager.LoadScene("RobotScene");
                break;
            case 2:
                SceneManager.LoadScene("ElevatorScene");
                break;
        }
        
    }
}
