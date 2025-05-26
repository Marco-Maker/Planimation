using System.Collections.Generic;
using System.IO;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Linq;
using System.Text.RegularExpressions;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject planPanel;

    [SerializeField] private TextMeshProUGUI focusText;

    [SerializeField] private TextMeshProUGUI planText;

    private List<string> focusObjects = new List<string>();
    private int currentFocus = 0;


    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
        LoadFocusObjectsFromPlan(Path.Combine(Application.dataPath, "PDDL", "output_plan.txt"));
    }


    public void ShowPlan()
    {
        if (planPanel.activeSelf)
        {
            planPanel.SetActive(false);
            return;
        }
        planPanel.SetActive(true);

        SetPlanText();
    }

    public void SetFocusText(string text)
    {
        focusText.text = text;
    }

    void LoadFocusObjectsFromPlan(string planPath)
    {
        var lines = File.ReadAllLines(planPath);
        bool planStarted = false;
        foreach (var l in lines)
        {
            if (!planStarted)
            {
                if (l.StartsWith("Found Plan:")) planStarted = true;
                continue;
            }
            var m = Regex.Matches(l, @"\(\w+\s+([^\s\)]+)");
            foreach (Match sub in m)
                focusObjects.Add(sub.Groups[1].Value);
        }
        focusObjects = focusObjects.Distinct().ToList();
    }

    public void SetPlanText()
    {
        string path = Path.Combine(Application.dataPath, "PDDL", "output_plan.txt");
        if (!File.Exists(path))
        {
            print("que merde putain");
            planText.text = "Error: couldn't load plan.";
            return;
        }

        string[] lines = File.ReadAllLines(path);
        var plan = new List<string>();

        bool isPlan = false;

        foreach (var line in lines)
        {
            if (!isPlan)
            {
                if (line.StartsWith("Found Plan:"))
                    isPlan = true;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line) ||
                 line.StartsWith("Plan-Length") ||
                 line.StartsWith("Metric") ||
                 line.StartsWith("Planning Time"))
                break;

            plan.Add(line.Trim());
        }

        planText.text = string.Join("\n", plan);
    }



    public void PrevFocus() {
        if (focusObjects.Count == 0) return;
        currentFocus = (currentFocus - 1 + focusObjects.Count) % focusObjects.Count;
        ApplyFocus();
    }

    public void NextFocus() {
        if (focusObjects.Count == 0) return;
        currentFocus = (currentFocus + 1) % focusObjects.Count;
        ApplyFocus();
    }

    private void ApplyFocus() {
        var name = focusObjects[currentFocus];
        focusText.text = name;
        //FocusOnObject(name); TODO 
    }

}


