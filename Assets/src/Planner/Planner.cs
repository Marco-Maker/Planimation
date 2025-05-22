using System.Diagnostics;
using System.IO;
using UnityEngine;

public class Planner : MonoBehaviour
{

    private string solverJarPath = "";
    private string outputPlanPath = "";
    private string domainPath = "";
    private string problemPath = "";

    private string additionalParameters = ""; //Da modificare se si vuole avere altri parametri

    public void RunShellCommand()
    {
        string path = Directory.GetCurrentDirectory();
        path += Const.PDDL_FOLDER;
        solverJarPath = Const.PDDL_FOLDER + Const.SOLVER;
        outputPlanPath = Const.PDDL_FOLDER + Const.OUTPUT_PLAN;
        domainPath = Const.PDDL_FOLDER + PlanInfo.GetInstance().GetDomainName();
        problemPath = Const.PROBLEM;
        string args = $"java -jar .{solverJarPath} -domain .{domainPath} -problem .{problemPath} {additionalParameters}";
        UnityEngine.Debug.Log(args);
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
        File.WriteAllText(Directory.GetCurrentDirectory() + outputPlanPath, output);
        if (!string.IsNullOrEmpty(error))
            UnityEngine.Debug.LogError("Errore: " + error);
    }
}
