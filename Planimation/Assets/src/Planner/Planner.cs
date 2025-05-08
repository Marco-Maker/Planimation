using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using TMPro;
using System;

[Serializable]
public class PDDLDomain
{
    public string name;
    public TextAsset domainFile;
    public string domainPath;
    public TextAsset problemFile;
    public string problemPath;
}

public class Planner : MonoBehaviour
{
    // I vari domini
    [SerializeField] private List<PDDLDomain> domains = new List<PDDLDomain>();

    // Path completo al file JAR del solver
    [SerializeField] private string solverJarPath = @"C:\Percorso\A\enhsp.jar";

    // Parametri aggiuntivi se richiesti dal solver
    [SerializeField] private string additionalParameters = ""; // es: "--search-timeout 30"

    // Dove salvare il piano risultante
    [SerializeField] private string outputPlanPath = @"C:\Percorso\A\output_plan.txt";

    /*public void GeneratePlan()
    {
        if (!File.Exists(solverJarPath))
        {
            UnityEngine.Debug.LogError("Solver jar non trovato: " + solverJarPath);
            return;
        }

        string args = $"-jar \"{solverJarPath}\" -o \"{domainFilePath}\" -f \"{problemFilePath}\" {additionalArgs}";

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


    public void DisplayFileInText(TextMeshProUGUI textField, string filePath)
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

    */

}
