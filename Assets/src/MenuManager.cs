using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class ObjectItem
{
    public string name;
    public TextMeshProUGUI number;
}

public class ObjectToAdd
{
    public string name;
    public int number;
}

[Serializable]
public class Predicates
{
    public string name;
    public int problem;// 0 = logistic, 1 = robot, 2 = elevator
    public List<string> values;
}

public class PredicateToAdd
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
    [SerializeField] private List<Predicates> predicatesList;
    private Dictionary<string, List<string>> predicatesAvailable;
    private List<PredicateToAdd> predicatesToAdd;

    [Header("PREDICATE-FIELD")]
    [SerializeField] private TextMeshProUGUI fieldTitle;
    [SerializeField] private TextMeshProUGUI fieldList;
    [SerializeField] private GameObject fieldOptions;
    [SerializeField] private GameObject predicateOptionPrefab;

    private int currentProblem = -1; // -1 = no problem selected, 0 = logistic, 1 = robot, 2 = elevator

    private void Start()
    {
        predicatesAvailable = new Dictionary<string, List<string>>();
        predicatesToAdd = new List<PredicateToAdd>();
        objectsToAdd = new List<ObjectToAdd>();
    }

    public void OpenPredicateField(string name)
    {
        predicateField.SetActive(true);
        FillPredicates(name);
    }

    private void FillPredicates(string name)
    {
        foreach (var predicate in predicatesList)
        {
            if(predicate.name == name)
                predicatesAvailable.Add(predicate.name, predicate.values);
        }
        fieldTitle.text = name;
        fieldList.text = "";
        foreach (var predicate in predicatesToAdd)
        {
            Debug.Log(predicate.name + " " + predicate.values.Count);
            if (predicate.name == name)
            {
                fieldList.text = name + " ";
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
                    //Questa è la parte da finire che gestice il dropdown
                    //obj.transform.GetChild(1).GetComponent<TMP_Dropdown>().ClearOptions();
                    /*List<string> options = new List<string>();
                    foreach (var predicate in predicatesList)
                    {
                        if (predicate.name == name)
                        {
                            foreach (var value in predicate.values)
                            {
                                options.Add(value);
                            }
                        }
                    }
                    obj.transform.GetChild(1).GetComponent<TMP_Dropdown>().AddOptions(options);*/
                }
        }
        
    }

    public void ClosePredicateField(bool add)
    {
        if (add)
        {
            PredicateToAdd p = new PredicateToAdd();
            GameObject options = GameObject.Find("PredicatesInputOptions");
            foreach (Transform child in options.transform)
            {
                if (child.name == "Title")
                {
                    p.name = child.GetComponent<TextMeshProUGUI>().text;
                }
                else if (child.name == "InputOptions")
                {
                    List<string> values = new List<string>();
                    foreach (Transform value in child)
                    {
                        values.Add(value.GetComponent<TMP_Dropdown>().options[value.GetComponent<TMP_Dropdown>().value].text);
                    }
                    p.values = values;
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

    public void OpenGoalsTab()
    {

    }
}
