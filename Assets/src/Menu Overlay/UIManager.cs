using System.Collections.Generic;
using System.IO;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject planPanel;

    [SerializeField] private TextMeshProUGUI focusText;

    [SerializeField] private TextMeshProUGUI planText;


    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
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


    public void SetPlanText()
    {
        string path = Path.Combine(Application.dataPath, "PDDL", "output_plan.txt");
        if (!File.Exists(path))
        {
            print ("que merde putain");
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

}


