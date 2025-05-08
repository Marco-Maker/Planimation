using System.Diagnostics;
using System.IO;
using UnityEngine;
using TMPro;
using System;

[Serializable]
public class PDDLDomain
{
    public string name;
    
}

public class Planner : MonoBehaviour
{

    private string pddlFolderPath = @"\Assets\PDDL\";

    private string solverJarPath = "";
    private string outputPlanPath = "";
    private string domainPath = "";
    private string problemPath = "";

    [SerializeField] private string solverName = "enhsp-20.jar";
    [SerializeField] private string outputFileName = "output_plan.txt";
    [SerializeField] private string domainName = @"2025-03-03-PDDL\domain-gripper.pddl";
    [SerializeField] private string problemName = @"2025-03-03-PDDL\problem-g1.pddl";

    // Parametri aggiuntivi se richiesti dal solver
    [SerializeField] private string additionalParameters = ""; // es: "--search-timeout 30"

    [SerializeField] private TextMeshProUGUI domainField;
    [SerializeField] private TextMeshProUGUI problemField;
    [SerializeField] private TextMeshProUGUI planField;

    private void Start()
    {
        GetCurrentDirectory();
        string path = Directory.GetCurrentDirectory();
        solverJarPath = path + pddlFolderPath + solverName;
        outputPlanPath = path + pddlFolderPath + outputFileName;
        domainPath = path + pddlFolderPath + domainName;
        problemPath = path + pddlFolderPath + problemName;
        Display();
    }

    public void GeneratePlan()
    {
        if (!File.Exists(solverJarPath))
        {
            UnityEngine.Debug.LogError("Solver jar non trovato: " + solverJarPath);
            return;
        }

        string args = $"-jar .{pddlFolderPath + solverName} -domain .{pddlFolderPath + domainName} -problem .{pddlFolderPath + problemName} {additionalParameters}";

        UnityEngine.Debug.Log(args);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "java",
            Arguments = args,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                File.WriteAllText(outputPlanPath, output);
                UnityEngine.Debug.Log("Piano generato e salvato in: " + outputPlanPath);
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Errore durante l'esecuzione del planner: " + e.Message);
        }
    }

    public void Display()
    {
        DisplayFileInText(domainField, domainPath);
        DisplayFileInText(problemField, problemPath);
        DisplayFileInText(planField, outputPlanPath);
    }

    private void DisplayFileInText(TextMeshProUGUI textField, string filePath)
    {
        if (!File.Exists(filePath))
        {
            textField.text = $"[ERRORE] File non trovato: {filePath}";
            return;
        }

        string content = File.ReadAllText(filePath);
        textField.text = content;
    }

    public string GetCurrentDirectory()
    {
        string path = Directory.GetCurrentDirectory();
        UnityEngine.Debug.Log("Path corrente: " + path);
        return path;
    }

}
