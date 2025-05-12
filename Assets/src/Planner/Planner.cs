using System.Diagnostics;
using System.IO;
using UnityEngine;
using TMPro;
using System;

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
    [SerializeField] private string problemName = @"2025-03-03-PDDL\problem-g1-types.pddl";

    // Parametri aggiuntivi se richiesti dal solver
    [SerializeField] private string additionalParameters = ""; // es: "--search-timeout 30"

    [SerializeField] private TextMeshProUGUI domainField;
    [SerializeField] private TextMeshProUGUI problemField;
    [SerializeField] private TextMeshProUGUI planField;

    private void Start()
    {
        string path = Directory.GetCurrentDirectory();
        solverJarPath = path + pddlFolderPath + solverName;
        outputPlanPath = path + pddlFolderPath + outputFileName;
        domainPath = path + pddlFolderPath + domainName;
        problemPath = path + pddlFolderPath + problemName;
        UnityEngine.Debug.Log(ProblemToGenerator.printDictionary(problemPath));
        UnityEngine.Debug.Log(ProblemToGenerator.printDictionaryInit(problemPath));
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

    public void RunShellCommand()
    {
        string args = $"java -jar .{pddlFolderPath + solverName} -domain .{pddlFolderPath + domainName} -problem .{pddlFolderPath + problemName} {additionalParameters}";
        ExecuteCommand(args);
    }

    private void ExecuteCommand(string command)
    {
    #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command);
    #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
            ProcessStartInfo psi = new ProcessStartInfo("/bin/bash", "-c \"" + command + "\"");
    #else
            Debug.LogWarning("Piattaforma non supportata.");
            return;
    #endif
        psi.WorkingDirectory = Directory.GetCurrentDirectory();
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        Process proc = new Process();
        proc.StartInfo = psi;
        proc.Start();

        string output = proc.StandardOutput.ReadToEnd();
        string error = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        File.WriteAllText("." + pddlFolderPath + outputFileName, output);
        if (!string.IsNullOrEmpty(error))
            UnityEngine.Debug.LogError("Errore: " + error);
        Display();
    }
}
